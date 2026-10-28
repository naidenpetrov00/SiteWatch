#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
environment_file="${repository_root}/.env.production"
compose_file="${repository_root}/compose.production.yaml"

if [[ ! -f "${environment_file}" ]]; then
    echo "Production environment file not found: ${environment_file}" >&2
    exit 1
fi

timestamp="$(date --utc +%Y%m%dT%H%M%SZ)"
backup_name="sitewatch_${timestamp}.bak"
container_backup_path="/var/opt/mssql/backups/${backup_name}"

docker compose \
    --env-file "${environment_file}" \
    --file "${compose_file}" \
    exec \
    --no-TTY \
    --env "SITEWATCH_BACKUP_PATH=${container_backup_path}" \
    database \
    bash -euo pipefail -c '
        export SQLCMDPASSWORD="${MSSQL_SA_PASSWORD}"
        query="BACKUP DATABASE [\$(DatabaseName)] TO DISK = N'\''\$(BackupPath)'\'' WITH COPY_ONLY, INIT, CHECKSUM, STOP_ON_ERROR, STATS = 10; RESTORE VERIFYONLY FROM DISK = N'\''\$(BackupPath)'\'' WITH CHECKSUM, STOP_ON_ERROR;"

        /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U sa \
            -C \
            -b \
            -v DatabaseName="${MSSQL_DATABASE}" BackupPath="${SITEWATCH_BACKUP_PATH}" \
            -Q "${query}"
    '

echo "Created and verified database backup: ${container_backup_path}"
