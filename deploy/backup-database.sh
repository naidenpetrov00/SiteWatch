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

        backup_directory="/var/opt/mssql/backups"
        mapfile -t backups_to_delete < <(
            find "${backup_directory}" \
                -maxdepth 1 \
                -type f \
                -name "sitewatch_*.bak" \
                -printf "%f\n" \
                | sort --reverse \
                | tail --lines=+8
        )

        for backup_to_delete in "${backups_to_delete[@]}"; do
            rm -- "${backup_directory}/${backup_to_delete}"
            echo "Removed expired local backup: ${backup_to_delete}"
        done
    '

echo "Created and verified database backup: ${container_backup_path}"
