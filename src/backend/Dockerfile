# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restaurar dependencias usando solo los .csproj (mejor uso de la caché de capas)
COPY nuget.config ./
COPY EventosVivos.Api/EventosVivos.Api.csproj EventosVivos.Api/
COPY EventosVivos.Application/EventosVivos.Application.csproj EventosVivos.Application/
COPY EventosVivos.Domain/EventosVivos.Domain.csproj EventosVivos.Domain/
COPY EventosVivos.Infrastructure/EventosVivos.Infrastructure.csproj EventosVivos.Infrastructure/
RUN dotnet restore EventosVivos.Api/EventosVivos.Api.csproj

# Copiar el resto del código y publicar la API
COPY . .
RUN dotnet publish EventosVivos.Api/EventosVivos.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
# Ejecutar como usuario no root (definido por la imagen de .NET) por seguridad.
USER $APP_UID
ENTRYPOINT ["dotnet", "EventosVivos.Api.dll"]
