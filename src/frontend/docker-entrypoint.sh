#!/bin/sh
# Genera la configuración de la aplicación en tiempo de ejecución a partir de las
# variables de entorno del contenedor. El frontend lee /config.json al iniciar.
# Este script lo ejecuta automáticamente la imagen de Nginx antes de arrancar.
set -e

: "${API_URL:=http://localhost:8080}"

cat > /usr/share/nginx/html/config.json <<EOF
{
  "apiUrl": "${API_URL}"
}
EOF
