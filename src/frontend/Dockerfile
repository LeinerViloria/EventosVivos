# Build stage
FROM node:22-alpine AS build
WORKDIR /app
RUN corepack enable

# Instalar dependencias con pnpm a partir del lockfile
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copiar el resto del código y construir
COPY . .
RUN pnpm run build

# Serve stage (Nginx)
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /docker-entrypoint.d/40-config.sh
RUN chmod +x /docker-entrypoint.d/40-config.sh
COPY --from=build /app/dist/eventosvivos/browser /usr/share/nginx/html
EXPOSE 80
