# Caddy con el módulo DNS de DuckDNS, para emitir el certificado por DNS-01.
FROM caddy:2-builder AS builder
RUN xcaddy build --with github.com/caddy-dns/duckdns

FROM caddy:2-alpine
COPY --from=builder /usr/bin/caddy /usr/bin/caddy
