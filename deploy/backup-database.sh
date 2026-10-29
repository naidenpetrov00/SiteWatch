#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
environment_file="${repository_root}/.env.production"
compose_file="${repository_root}/compose.production.yaml"
borg_environment_file="/etc/sitewatch/backup/borg.env"
healthchecks_ping_file="/etc/sitewatch/backup/healthchecks-ping-url"
sitewatch_data_root="/mnt/HC_Volume_107250753/sitewatch"
healthchecks_ping_url=""

usage() {
    echo "Usage: $0 [--preview-retention]" >&2
}

run_borg_retention() {
    local dry_run="$1"
    local -a prune_arguments=(
        --list
        --glob-archives "sitewatch-database-*"
        --keep-daily 7
        --keep-weekly 4
        --keep-monthly 6
    )

    if [[ "${dry_run}" == "true" ]]; then
        prune_arguments+=(--dry-run)
        echo "Previewing Borg retention; no remote archives will be changed."
    else
        echo "Pruning remote Borg archives."
    fi

    borg prune "${prune_arguments[@]}"

    if [[ "${dry_run}" == "false" ]]; then
        echo "Compacting the Borg repository."
        borg compact
    fi
}

ping_healthchecks() {
    local event="$1"
    local ping_url="${healthchecks_ping_url%/}"

    case "${event}" in
        start)
            ping_url="${ping_url}/start"
            ;;
        success)
            ;;
        failure)
            ping_url="${ping_url}/fail"
            ;;
        *)
            echo "Unsupported Healthchecks.io event: ${event}" >&2
            return
            ;;
    esac

    if ! curl \
        --fail \
        --silent \
        --show-error \
        --max-time 10 \
        --retry 3 \
        --output /dev/null \
        "${ping_url}"; then
        echo "Unable to send the Healthchecks.io ${event} signal." >&2
    fi
}

report_backup_result() {
    local exit_code=$?
    trap - EXIT

    if [[ "${exit_code}" -eq 0 ]]; then
        ping_healthchecks success
    else
        ping_healthchecks failure
    fi

    exit "${exit_code}"
}

case "${1:-}" in
    "")
        mode="backup"
        ;;
    --preview-retention)
        mode="preview-retention"
        ;;
    *)
        usage
        exit 1
        ;;
esac

if [[ ! -f "${borg_environment_file}" ]]; then
    echo "Borg environment file not found: ${borg_environment_file}" >&2
    exit 1
fi

# shellcheck disable=SC1091
source "${borg_environment_file}"

if [[ "${mode}" == "preview-retention" ]]; then
    run_borg_retention true
    exit
fi

if [[ ! -f "${environment_file}" ]]; then
    echo "Production environment file not found: ${environment_file}" >&2
    exit 1
fi

if [[ -f "${healthchecks_ping_file}" ]]; then
    healthchecks_ping_url="$(<"${healthchecks_ping_file}")"

    if [[ -n "${healthchecks_ping_url}" ]]; then
        trap report_backup_result EXIT
        ping_healthchecks start
    else
        echo "Healthchecks.io ping file is empty; monitoring is disabled." >&2
    fi
else
    echo "Healthchecks.io monitoring is not configured; continuing without notifications."
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

cd -- "${sitewatch_data_root}"
borg create \
    --stats \
    --compression lz4 \
    "::sitewatch-database-{now:%Y-%m-%dT%H-%M-%SZ}" \
    backups

echo "Created encrypted off-server Borg archive."

case "${BORG_RETENTION_ENABLED:-0}" in
    0)
        echo "Remote Borg retention is disabled; run --preview-retention before enabling it."
        ;;
    1)
        run_borg_retention false
        ;;
    *)
        echo "BORG_RETENTION_ENABLED must be set to 0 or 1." >&2
        exit 1
        ;;
esac
