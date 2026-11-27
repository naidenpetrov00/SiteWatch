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
deploy/mediamtx.yml
```

MediaMTX has no persistent data directory. Camera paths and active sessions are
runtime state that the API recreates lazily after a restart.

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
  up --detach database azurite mediamtx api web
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

Expected services are `database`, `azurite`, `mediamtx`, `api`, and `web`.
Services with a health check should report `healthy`.

## Stop only the public application

```bash
sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  stop web api mediamtx
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

Replace `api` with `web`, `mediamtx`, `database`, or `azurite` to inspect
another service. `--tail=150` limits the output to the most recent 150 lines.

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

## MediaMTX WebRTC gateway

Production uses a dedicated streaming hostname and a direct UDP media port:

```text
HTTPS WHEP signaling: https://STREAM_DOMAIN/<path>/whep
WebRTC media:         MEDIAMTX_WEBRTC_MEDIA_PORT/udp
Control API:          mediamtx:9997 inside Docker only
```

Set these values in `.env.production`:

| Variable | Purpose |
| --- | --- |
| `STREAM_DOMAIN` | Public hostname where Caddy terminates signaling HTTPS |
| `MEDIAMTX_WEBRTC_ADVERTISED_HOSTS` | Comma-separated public DNS names or IPs placed in ICE candidates |
| `MEDIAMTX_WEBRTC_MEDIA_PORT` | Matching MediaMTX listener and public host UDP port |

Compose maps these settings to MediaMTX as follows:

```text
MEDIAMTX_WEBRTC_MEDIA_PORT       -> MTX_WEBRTCLOCALUDPADDRESS
MEDIAMTX_WEBRTC_ADVERTISED_HOSTS -> MTX_WEBRTCADDITIONALHOSTS
https://DASHBOARD_DOMAIN         -> MTX_WEBRTCALLOWORIGINS
false                            -> MTX_WEBRTCIPSFROMINTERFACES
http                             -> MTX_AUTHMETHOD
http://api:8080/internal/mediamtx/auth -> MTX_AUTHHTTPADDRESS
```

Compose also supplies the API with `MediaMtx__ControlApiBaseUrl` set to
`http://mediamtx:9997` and `MediaMtx__PublicWhepBaseUrl` set to
`https://STREAM_DOMAIN`. These are internal deployment settings derived from
the Compose topology and do not contain secrets.

The host and container UDP media ports intentionally use the same value. Do not
change only one side of the mapping: MediaMTX advertises its listener port in
ICE candidates.

Create an `A` record for `STREAM_DOMAIN` pointing to the production host. Add an
`AAAA` record only after verifying both HTTPS and direct UDP media over IPv6.
Caddy obtains the HTTPS certificate automatically. Allow the configured media
port as inbound UDP in both the Hetzner Cloud firewall and the host firewall.
If another NAT device is present, forward the same UDP port to the host.

MediaMTX must also have outbound RTSP/TCP connectivity to every camera. Cameras
on a private remote network require an existing VPN or private route; do not
expose raw RTSP publicly as a workaround.

### Public signaling surface

Caddy forwards only the WHEP negotiation endpoint `/<path>/whep` and its
single-segment session subresources `/<path>/whep/<session-id>`. Only `OPTIONS`,
`POST`, `PATCH`, and `DELETE` are proxied. Every other request on
`STREAM_DOMAIN` receives `404`; the general MediaMTX HTTP interface and Control
API are not public.

The dashboard viewer planned for Step 3 must be a programmatic WHEP client, not
an iframe using MediaMTX's built-in page.

### Authorization and Control API

MediaMTX uses SiteWatch's external HTTP authorization callback at
`http://api:8080/internal/mediamtx/auth`. Caddy returns `404` for
`/internal/mediamtx` and every descendant on the public API hostname; only the
direct container route reaches the callback.

An administrator requests stream access through:

```text
POST /dashboard/sites/{siteId}/cameras/{cameraId}/stream-access
```

The API verifies the camera/site relationship, lazily reconciles the MediaMTX
path, and returns an absolute WHEP URL, an opaque bearer ticket, and a connection
deadline. The viewer ticket is valid for two minutes and covers only WebRTC
`read` access to its exact path. The callback rechecks that the camera still
belongs to the site and that the ticketed user remains an administrator. It
does not return MediaMTX's `expireAt`, so an established session may continue
until disconnected.

Every Control API request uses a separate, 30-second Data Protection ticket.
Both ticket payloads contain the absolute expiry used by the time-limited data
protector, and the callback explicitly validates that expiry. No action is
excluded from HTTP authorization: `publish`, `playback`, `metrics`, `pprof`,
unknown actions, invalid paths, and invalid or expired tickets are denied.

Production does not publish Control API port `9997` on the host, and Caddy never
proxies it. MediaMTX joins both the `frontend` and `backend` networks, however,
so containers on either network may be able to reach the listener. Docker
network placement is not the sole authorization boundary; the private callback
must approve the `api` action for every request.

SiteWatch is the sole writer of paths named
`camera-{cameraId:N}-main` and `camera-{cameraId:N}-sub`. Do not add, patch,
replace, or delete these paths manually or from another service. MediaMTX 1.21.4
redacts source credentials in Control API reads, so a credential-only external
change cannot be detected while SiteWatch's in-memory source fingerprint is
still valid. Change camera settings through SiteWatch instead. If an external
credential-only change occurs accidentally, restart the API to clear the
fingerprint cache; the next administrator access request performs one
conservative path replacement.

Path configuration is runtime-only. MediaMTX reconnects to its RTSP source only
while at least one viewer is active, and the API recreates missing paths lazily
after a restart.

Never put camera credentials, credential-bearing RTSP URLs, tokens, or Control
API request bodies in documentation or logs. Runtime path configurations can
contain camera credentials and must be treated as sensitive.

### Health, capacity, and media limitations

The container health check runs:

```text
/mediamtx --validate-conf=/mediamtx.yml
```

This checks the mounted configuration and `MTX_*` overrides. It does not prove
that the running WebRTC listener, Control API listener, or authorization
callback is responsive. A safe authenticated runtime readiness probe remains a
future deployment improvement.

The initial `256m` memory limit must be measured later under the expected number,
resolution, and bitrate of concurrent streams. Docker retains at most three
10 MB log files through the shared production logging policy.

MediaMTX passes camera media through without transcoding. H.264 cameras must use
a browser-compatible profile without B-frames. H.265 is unavailable or
conditional in many browsers. No FFmpeg, TURN server, TCP ICE listener,
recording, or persistent MediaMTX storage is included. Direct UDP
media can therefore fail on restrictive client networks; TURN is a possible
later requirement.

The configuration disables the inbound RTSP server while allowing MediaMTX to
act as an outbound RTSP client. Configuration validation cannot prove that
behavior; verify it later with an authenticated, dynamically registered test
path and an actual camera source.

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

## Import researched retailers and extraction-profile drafts

The reviewed import is stored at:

```text
deploy/import-retailers-and-extraction-drafts.sql
```

It contains 19 company configurations. House of Sport is intentionally absent
because the current extraction-rule model accepts only EUR prices.

For each company, the script validates that the referenced `Person` exists and
is a company. It then applies these independent rules:

- If any retailer already exists for the company, every existing retailer is
  preserved and no retailer is inserted for that company.
- Otherwise, one active retailer is inserted with `IsDefaultForOffers = 0`.
- If any extraction-profile version already exists for the company, every
  existing profile, host, and rule is preserved and no profile data is inserted.
- Otherwise, draft version 1 is inserted with two allowed hosts, one enabled
  rule, and `ConfigurationRevision = 2`.

The script never updates or deletes a retailer, profile, host, rule,
observation, or listing. New profiles are inactive drafts with no publication
or successful-test metadata. Test URLs are printed for later UI testing but are
not stored in the database.

### 1. Validate the production Compose configuration

```bash
cd /opt/sitewatch

sudo docker compose \
  --env-file .env.production \
  --file compose.production.yaml \
  config --quiet
```

### 2. Preview the complete transaction

`APPLY=0` executes the guarded insert path inside one transaction, prints the
simulated retailer, profile, host, and rule results, and then rolls back. It
finishes with a 38-row action report: one retailer action and one profile action
for each of the 19 companies.

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
      -d "${MSSQL_DATABASE}" \
      -v APPLY=0 \
      -i /dev/stdin
  ' < ./deploy/import-retailers-and-extraction-drafts.sql
```

Stop if the preview returns a nonzero exit code or reports an unexpected action.
The schema guard requires migration
`20261114100024_MovePriceBasisToRetailerListing`, the removed rule
`PriceBasis` column, and the replacement listing `OnlinePriceBasis` column.

Expected preview statuses are:

```text
WOULD_CREATE_RETAILER
SKIPPED_EXISTING_RETAILER
WOULD_CREATE_DRAFT
SKIPPED_EXISTING_PROFILE
```

Maxxmart will normally report skipped actions because it may already have a
retailer and profile. Always trust the report from the current database rather
than assuming which companies are new.

### 3. Create and verify a fresh backup

```bash
sudo ./deploy/backup-database.sh
```

Do not apply the import unless the command creates and verifies the SQL Server
backup and completes the encrypted off-server Borg archive successfully.

### 4. Apply the import

Use the same command with `APPLY=1`:

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
      -d "${MSSQL_DATABASE}" \
      -v APPLY=1 \
      -i /dev/stdin
  ' < ./deploy/import-retailers-and-extraction-drafts.sql
```

Expected apply statuses are:

```text
CREATED_RETAILER
SKIPPED_EXISTING_RETAILER
CREATED_DRAFT
SKIPPED_EXISTING_PROFILE
```

A successful repeated run should report only skipped actions and must not alter
any existing configuration.

### 5. Verify the persisted data without changing it

This command is read-only. It displays every retailer, profile, allowed host,
and rule associated with the 19 company Persons.

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
      -d "${MSSQL_DATABASE}" \
      -i /dev/stdin
  ' <<'SQL'
DECLARE @AffectedCompanies TABLE
(
    SortOrder int NOT NULL,
    PersonId uniqueidentifier NOT NULL
);

INSERT INTO @AffectedCompanies (SortOrder, PersonId)
VALUES
    (1, '034c567a-76ae-4d9c-bb9b-a4becb5053ec'),
    (2, 'f3b11722-2748-4d3b-a9b0-d3bfb42429c6'),
    (3, 'aa1a8433-eeae-44e6-bb14-dbe8054366e7'),
    (4, 'f62ad111-dfb8-42eb-825f-ffbd6c14f047'),
    (5, 'a50fa825-634e-4a0c-a1f0-c1b1c4353ba0'),
    (6, '569459b2-4a65-43ea-ab79-2f3f6176bf12'),
    (7, 'c1241830-7377-4d32-bbd9-8f8ad77a74f8'),
    (8, '376b37b7-21a0-43b8-a5ac-932b0cd42e6e'),
    (9, '8f7e9641-5559-46e6-a85f-5141daf0e51a'),
    (10, 'd8479494-ad9b-4ed0-b477-b46520e59b52'),
    (11, '6301c610-6512-454e-ae5d-786ca84f1ce7'),
    (12, '0f4400ab-de38-4a2c-a683-09ce82363df5'),
    (13, '9afdc138-e7fd-4dbb-a345-a479eb68ad6a'),
    (14, '4ce57854-b2d8-4b95-b7db-52b0ed571ad2'),
    (15, '60f4cf9a-0afe-4db4-b4e0-ae124ac85199'),
    (16, 'c64f0130-933a-4535-bd3a-9f7f33f0414c'),
    (17, '29f0246e-c469-4a11-90dd-1d21a60b7c41'),
    (18, '51f4e65f-271f-45e4-b3fa-c14e540bdd3a'),
    (19, '76ba2290-1d14-4028-8aa8-dda8574fbcaf');

SELECT
    affected.PersonId AS CompanyPersonId,
    retailer.Id AS RetailerId,
    retailer.DisplayName,
    retailer.BaseWebsiteUrl,
    retailer.NormalizedWebsiteHost,
    retailer.IsActive,
    retailer.IsDefaultForOffers
FROM @AffectedCompanies AS affected
LEFT JOIN Retailers AS retailer
    ON retailer.CompanyPersonId = affected.PersonId
ORDER BY affected.SortOrder, retailer.DisplayName, retailer.Id;

SELECT
    affected.PersonId AS CompanyPersonId,
    profile.Id AS ProfileId,
    profile.Version,
    profile.Status,
    profile.IsActive,
    profile.ConfigurationRevision,
    profile.ValidatedConfigurationRevision,
    profile.PublishedAt,
    profile.PublishedBy,
    profile.LastSuccessfulTestAt,
    profile.LastSuccessfulTestRuleId
FROM @AffectedCompanies AS affected
LEFT JOIN RetailerExtractionProfiles AS profile
    ON profile.CompanyPersonId = affected.PersonId
ORDER BY affected.SortOrder, profile.Version, profile.Id;

SELECT
    affected.PersonId AS CompanyPersonId,
    profile.Id AS ProfileId,
    allowedHost.Id AS AllowedHostId,
    allowedHost.NormalizedHost
FROM @AffectedCompanies AS affected
LEFT JOIN RetailerExtractionProfiles AS profile
    ON profile.CompanyPersonId = affected.PersonId
LEFT JOIN RetailerExtractionAllowedHosts AS allowedHost
    ON allowedHost.ExtractionProfileId = profile.Id
ORDER BY affected.SortOrder, profile.Version, allowedHost.NormalizedHost;

SELECT
    affected.PersonId AS CompanyPersonId,
    profile.Id AS ProfileId,
    extractionRule.Id AS RuleId,
    extractionRule.Name,
    extractionRule.Priority,
    extractionRule.IsEnabled,
    extractionRule.RuleType,
    extractionRule.JsonLdObjectType,
    extractionRule.JsonLdPricePath,
    extractionRule.JsonLdCurrencyPath,
    extractionRule.CssSelector,
    extractionRule.CssValueSource,
    extractionRule.CssAttributeName,
    extractionRule.DecimalSeparator,
    extractionRule.ThousandsSeparator,
    extractionRule.ExpectedCurrencyCode,
    extractionRule.MinimumValue,
    extractionRule.MaximumValue
FROM @AffectedCompanies AS affected
LEFT JOIN RetailerExtractionProfiles AS profile
    ON profile.CompanyPersonId = affected.PersonId
LEFT JOIN RetailerExtractionRules AS extractionRule
    ON extractionRule.ExtractionProfileId = profile.Id
ORDER BY affected.SortOrder, profile.Version, extractionRule.Priority, extractionRule.Id;
SQL
```

Every newly created draft must still be tested with its reported test URL and
then explicitly published through the application UI. The import does not
fabricate successful-test metadata or publish/activate any profile.

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

