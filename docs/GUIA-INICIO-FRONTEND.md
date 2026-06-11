# Guía de Inicio del Frontend (Angular 22)

Este documento registra, paso a paso, los comandos y las configuraciones que se aplican para montar el frontend, con una explicación de cada uno. Su propósito es servir de guía de aprendizaje y permitir reproducir el montaje desde cero. Es un documento vivo: se amplía a medida que se configura el proyecto.

---

## 1. Requisitos previos

### Node.js

Angular 22 (en concreto el CLI 22.0.1) exige una versión de Node igual o superior a **22.22.3**, o bien 24.15.0 o superior. Una versión anterior, como la 20.x, hace que el CLI se niegue a ejecutarse. Se verifica con:

```bash
node --version
```

### pnpm como gestor de paquetes

El proyecto usa **pnpm**, no npm. pnpm se habilita con **corepack**, una herramienta que viene incluida con Node, de modo que no hace falta instalar nada con npm:

```bash
corepack enable pnpm
```

```bash
pnpm --version
```

---

## 2. Generación del workspace de Angular

El proyecto se genera con el CLI de Angular ejecutado mediante `npx`, sin instalarlo de forma global:

```bash
npx -y @angular/cli@22 new eventosvivos --directory src/frontend --package-manager pnpm --style tailwind --routing --ssr=false --zoneless --ai-config none --skip-git --defaults
```

Cada opción cumple un propósito concreto:

| Opción | Para qué sirve |
|--------|----------------|
| `eventosvivos` | Nombre del workspace y del proyecto inicial. Debe ir en minúsculas por las reglas de nombres de paquetes. |
| `--directory src/frontend` | Crea el proyecto dentro de `src/frontend`, respetando la estructura del monorepo. |
| `--package-manager pnpm` | Usa pnpm para instalar las dependencias, en lugar de npm. |
| `--style tailwind` | Configura Tailwind como sistema de estilos directamente desde el CLI, sin montarlo a mano. |
| `--routing` | Habilita el enrutamiento, necesario para navegar entre las vistas de las features. |
| `--ssr=false` | No configura renderizado del lado del servidor; la aplicación es una SPA que consume la API. |
| `--zoneless` | Genera la aplicación sin `zone.js`, en línea con la detección de cambios basada en signals de Angular 22. |
| `--ai-config none` | No genera archivos de configuración para herramientas de IA; el proyecto ya tiene su propio `CLAUDE.md` en la raíz. |
| `--skip-git` | No inicializa un repositorio Git nuevo, porque el monorepo ya está versionado. |
| `--defaults` | Desactiva los prompts interactivos para las opciones que tienen valor por defecto, de modo que la generación sea no interactiva. |

Una nota relevante de Angular 22: la opción `--test-runner` toma **vitest** como valor por defecto, de modo que el proyecto queda configurado con Vitest sin necesidad de indicarlo, en línea con la decisión de pruebas del proyecto.

---

## 3. Personalización de la estructura

Por defecto, el CLI genera la aplicación en `src/frontend/src/app`, lo que produce una doble carpeta `src`. Para evitarlo y que el código viva en `src/frontend/app`, se hacen dos cosas: mover los archivos y ajustar la configuración.

### Mover los archivos

Se sube el contenido de `src/frontend/src` un nivel, hasta `src/frontend`. Como `main.ts` y la carpeta `app` se mueven juntos, los imports relativos entre ellos se conservan sin cambios.

```powershell
Move-Item src/frontend/src/main.ts src/frontend/main.ts
```

```powershell
Move-Item src/frontend/src/index.html src/frontend/index.html
```

```powershell
Move-Item src/frontend/src/styles.css src/frontend/styles.css
```

```powershell
Move-Item src/frontend/src/app src/frontend/app
```

```powershell
Remove-Item src/frontend/src -Recurse -Force
```

### Ajustar la configuración

En `angular.json`, dentro del proyecto, se actualizan las rutas para que apunten a la nueva ubicación: `sourceRoot` pasa a ser la raíz del proyecto (cadena vacía), `browser` apunta a `main.ts`, se añade `index` con `index.html`, y `styles` referencia `styles.css`.

```jsonc
"sourceRoot": "",
...
"browser": "main.ts",
"index": "index.html",
...
"styles": [ "styles.css" ]
```

En `tsconfig.app.json` se ajustan los patrones de inclusión y exclusión a la nueva estructura:

```jsonc
"include": [ "main.ts", "app/**/*.ts" ],
"exclude": [ "app/**/*.spec.ts" ]
```

En `tsconfig.spec.json` se ajusta la inclusión de los archivos de prueba:

```jsonc
"include": [ "app/**/*.d.ts", "app/**/*.spec.ts" ]
```

---

## 4. Verificación

Se comprueba que la reestructuración no rompió nada compilando y ejecutando las pruebas. Los comandos usan `-C src/frontend` para ejecutarse dentro del proyecto del frontend sin cambiar de directorio.

Compilación:

```bash
pnpm -C src/frontend run build
```

Pruebas con Vitest:

```bash
pnpm -C src/frontend test
```

La opción `--watch` del ejecutor de pruebas toma el valor `true` solo en entornos interactivos (TTY) y `false` en el resto, de modo que en una terminal no interactiva o en la integración continua las pruebas se ejecutan una sola vez y el proceso termina por sí mismo.
