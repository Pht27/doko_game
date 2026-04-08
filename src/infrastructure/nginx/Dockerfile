FROM node:22-alpine AS frontend-build
WORKDIR /app
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend/ ./
RUN npm run build

FROM nginx:alpine
COPY --from=frontend-build /app/dist /usr/share/nginx/html
COPY src/infrastructure/nginx/nginx.conf /etc/nginx/conf.d/default.conf
