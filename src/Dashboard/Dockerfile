FROM node:24-alpine AS build
WORKDIR /app

COPY src/Dashboard/package.json src/Dashboard/package-lock.json ./
RUN npm ci

COPY src/Dashboard/ ./
RUN npm run build -- --output-path=/app/dist/sitewatch-dashboard

FROM caddy:2.11.7-alpine
COPY deploy/Caddyfile /etc/caddy/Caddyfile
COPY --from=build /app/dist/sitewatch-dashboard/browser /srv/dashboard
