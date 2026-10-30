#!/usr/bin/env bash
set -euo pipefail

sitewatch_volume="/mnt/HC_Volume_107250753"
sitewatch_data_root="${sitewatch_volume}/sitewatch"

if ! mountpoint --quiet "${sitewatch_volume}"; then
    echo "Expected mounted Hetzner volume not found: ${sitewatch_volume}" >&2
    exit 1
fi

# SQL Server 2022 Linux containers run as UID 10001 with root as the group.
install -d -m 0750 -o root -g root "${sitewatch_data_root}"
install -d -m 0770 "${sitewatch_data_root}/mssql"
install -d -m 0770 "${sitewatch_data_root}/backups"
chown -R +10001:+0 "${sitewatch_data_root}/mssql" "${sitewatch_data_root}/backups"
chmod -R u+rwX,g+rwX,o-rwx "${sitewatch_data_root}/mssql" "${sitewatch_data_root}/backups"

# The pinned ASP.NET image runs the API as APP_UID 1654.
install -d -m 0700 "${sitewatch_data_root}/data-protection"
chown -R +1654:+1654 "${sitewatch_data_root}/data-protection"
chmod -R u+rwX,go-rwx "${sitewatch_data_root}/data-protection"

# Azurite's Node user is UID/GID 1000. Caddy binds public ports as root.
install -d -m 0770 "${sitewatch_data_root}/azurite"
chown -R +1000:+1000 "${sitewatch_data_root}/azurite"
chmod -R u+rwX,g+rwX,o-rwx "${sitewatch_data_root}/azurite"

install -d -m 0750 -o root -g root \
    "${sitewatch_data_root}/caddy" \
    "${sitewatch_data_root}/caddy/data" \
    "${sitewatch_data_root}/caddy/config"

echo "Prepared SiteWatch storage at ${sitewatch_data_root}"
