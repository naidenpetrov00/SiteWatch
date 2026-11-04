# SiteWatch production runbook

This runbook documents the less-common commands used to operate the SiteWatch
production deployment. It is safe to keep this file in Git because it contains
procedures, public service names, and paths, but no credentials.

Never add any of the following to this document or to Git:

- `.env.production` contents
- passwords, tokens, SMTP credentials, or connection strings
- private SSH keys
- the Borg passphrase or exported Borg recovery key
- Healthchecks.io ping URLs
- database backup files

## Production layout

The production repository is located at `/opt/sitewatch` on the server. Unless
stated otherwise, run all commands in that directory:

```bash
cd /opt/sitewatch
```

The production stack is defined by:

```text
compose.production.yaml
.env.production
```

The attached Hetzner Volume stores persistent data under:

```text
/mnt/HC_Volume_107250753/sitewatch
```

Important subdirectories are:

```text
mssql/           SQL Server data
backups/         Local SQL Server .bak backups
azurite/         Blob storage data
data-protection/ ASP.NET Core data-protection keys
caddy/           HTTPS certificates and Caddy state
```

The production server follows the `deploy/hetzner-production` Git branch.
Feature work is merged into `main` and is promoted to production by merging
`main` into `deploy/hetzner-production`.

## Docker Compose command structure

Production commands normally use both of these options:

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml
```

`--env-file` supplies production configuration without committing it to Git.
`--file` selects the production Compose definition instead of the local one.

## Validate the production configuration

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  config --quiet
```

This expands and validates the Compose configuration. No containers or data are
changed. No output means validation succeeded.

## Build the application images

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  build --pull api web
```

This builds new API and dashboard images. `--pull` checks for newer base images.
Building does not replace currently running containers.

## Apply normal incremental database migrations

The production API does not apply migrations during normal startup. Migrations
are applied explicitly through the optional `migrate` service:

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  --profile tools \
  run --rm migrate
```

`--profile tools` enables maintenance-only services. `run` creates a one-off
container, and `--rm` removes that container after it exits. A successful command
must return exit status zero before the new API is started.

Before applying a production migration:

1. Create and verify a fresh database backup.
2. Review the generated migration and SQL.
3. Stop `web` and `api` if the schema change is incompatible with the running
   version.
4. Run the migration service.
5. Start and verify the application.

## Start or update production services

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  up --detach database azurite api web
```

`up --detach` creates or replaces containers whose image or configuration
changed and leaves them running in the background. Persistent bind-mounted data
is not removed.

Check service state:

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  ps
```

Expected services are `database`, `azurite`, `api`, and `web`. Services with a
health check should report `healthy`.

## Stop only the public application

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  stop web api
```

This creates downtime but leaves SQL Server and Azurite running. It does not
delete containers, volumes, or data.

## Inspect service logs

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  logs --tail=150 api
```

Replace `api` with `web`, `database`, or `azurite` to inspect another service.
`--tail=150` limits the output to the most recent 150 lines.

## Verify the public endpoints

```bash
curl --fail --silent --show-error \
  --output /dev/null \
  --write-out 'dashboard: %{http_code}\n' \
  https://sitewatch-dashboard.com

curl --fail --silent --show-error \
  --output /dev/null \
  --write-out 'api health: %{http_code}\n' \
  https://api.sitewatch-dashboard.com/healthz

curl --silent --show-error \
  --output /dev/null \
  --write-out 'www: %{http_code} -> %{redirect_url}\n' \
  https://www.sitewatch-dashboard.com
```

Expected results are HTTP `200` for the dashboard and API health endpoint and a
redirect from `www` to the main domain.

## Create an administrator

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  --profile tools \
  run --rm create-administrator
```

The command prompts for a username, email, and password. It can be run again to
create additional administrators. Credentials are entered interactively and are
not stored in shell history by the command.

## Create and verify a database backup manually

```bash
sudo ./deploy/backup-database.sh
```

The script performs all of the following:

1. Creates a SQL Server `COPY_ONLY` backup with checksums.
2. Runs `RESTORE VERIFYONLY` against the new backup.
3. Keeps the newest seven local `.bak` files.
4. Creates an encrypted Borg archive on the Hetzner Storage Box.
5. Applies remote retention when enabled.
6. Notifies Healthchecks.io of success or failure when configured.

Local backups are stored at:

```text
/mnt/HC_Volume_107250753/sitewatch/backups
```

List them without modifying anything:

```bash
sudo ls -lh /mnt/HC_Volume_107250753/sitewatch/backups
```

## Inspect the automatic backup schedule

```bash
sudo systemctl list-timers \
  sitewatch-database-backup.timer \
  --all
```

The timer is configured to run daily at 03:00 Europe/Sofia. `Persistent=true`
means systemd runs a missed backup after the server starts again.

Inspect the latest backup execution:

```bash
sudo systemctl status \
  sitewatch-database-backup.service \
  --no-pager

sudo journalctl \
  --unit sitewatch-database-backup.service \
  --lines 150 \
  --no-pager
```

A completed one-shot service normally shows `inactive (dead)` together with
`status=0/SUCCESS`. That state means the job finished; it does not mean it failed.

## Preview remote backup retention

```bash
sudo ./deploy/backup-database.sh --preview-retention
```

This shows which Borg archives would be kept or pruned without creating a backup
and without deleting any remote archive.

The configured remote policy keeps:

- 7 daily archives
- 4 weekly archives
- 6 monthly archives

## List encrypted Storage Box archives

```bash
sudo bash -c '
  source /etc/sitewatch/backup/borg.env
  borg list --format "{archive}  {time}{NL}"
'
```

The environment file supplies the repository, SSH command, and passphrase
command. The command lists archive metadata without extracting or deleting data.

## Test extraction of a Borg archive

Replace `ARCHIVE_NAME` with a value returned by `borg list`:

```bash
sudo bash -c '
  source /etc/sitewatch/backup/borg.env

  restore_directory="$(mktemp --directory /tmp/sitewatch-borg-restore.XXXXXX)"
  cd "${restore_directory}"
  borg extract "::ARCHIVE_NAME"
  echo "Extracted into: ${restore_directory}"
'
```

This only extracts files into a temporary directory. It does not restore or
modify the live database. Inspect the printed directory and remove that exact
temporary directory manually after verification.

## Exceptional procedure: recreate an empty production database

Use this procedure only while production contains no data that must be retained.
Once real data exists, preserve the migration chain and use incremental
migrations instead.

### 1. Create a final verified backup

```bash
sudo ./deploy/backup-database.sh
```

Do not continue unless the local backup is verified and the encrypted Borg
archive is created successfully.

### 2. Confirm the expected database name

```bash
grep -qx 'MSSQL_DATABASE=SiteWatch' .env.production \
  && echo "Confirmed database: SiteWatch" \
  || echo "STOP: unexpected database name"
```

Continue only when the database name is exactly `SiteWatch`.

### 3. Stop the public application

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  stop web api
```

### 4. Drop only the SiteWatch database

The following command disconnects active sessions and permanently drops only
the explicitly named `SiteWatch` database. SQL Server and the attached Volume
remain in place.

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  exec --no-TTY database \
  bash -euo pipefail -c '
    export SQLCMDPASSWORD="${MSSQL_SA_PASSWORD}"

    /opt/mssql-tools18/bin/sqlcmd \
      -S localhost \
      -U sa \
      -C \
      -b \
      -d master \
      -Q "
        IF DB_ID(N'\''SiteWatch'\'') IS NULL
            THROW 50000, N'\''Expected database SiteWatch does not exist.'\'', 1;

        ALTER DATABASE [SiteWatch]
            SET SINGLE_USER
            WITH ROLLBACK IMMEDIATE;

        DROP DATABASE [SiteWatch];
      "
  '
```

Important command options:

- `exec --no-TTY` runs inside the existing database container without an
  interactive terminal.
- `bash -euo pipefail` stops on errors, unset variables, or failed pipelines.
- `SQLCMDPASSWORD` supplies the password without printing it in the command.
- `-C` accepts the SQL Server container's TLS certificate.
- `-b` makes `sqlcmd` return a failure exit code when SQL fails.
- `SINGLE_USER WITH ROLLBACK IMMEDIATE` disconnects active database sessions.

### 5. Create the new database schema

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  --profile tools \
  run --rm migrate
```

Stop if this command fails. Do not start the API against a partially migrated
database.

### 6. Verify migration history

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  exec --no-TTY database \
  bash -euo pipefail -c '
    export SQLCMDPASSWORD="${MSSQL_SA_PASSWORD}"

    /opt/mssql-tools18/bin/sqlcmd \
      -S localhost \
      -U sa \
      -C \
      -b \
      -d SiteWatch \
      -Q "SELECT [MigrationId], [ProductVersion] FROM [__EFMigrationsHistory] ORDER BY [MigrationId];"
  '
```

### 7. Start and verify the application

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  up --detach api web

sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  ps
```

Verify the HTTPS endpoints, recreate the administrator, test login, and create a
new backup after the reset.

## Commands that must not be used casually

Do not run these commands as part of normal deployment:

```text
docker compose down --volumes
docker volume prune
rm -rf /mnt/HC_Volume_107250753/sitewatch
git reset --hard
```

They can remove persistent data or discard work. Investigate the exact target
and recovery plan before using any destructive command.

