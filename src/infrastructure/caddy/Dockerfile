FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend/ ./
RUN npm run build

FROM caddy:alpine
COPY --from=frontend-build /app/dist /srv
COPY src/infrastructure/caddy/Caddyfile /etc/caddy/Caddyfile
