# syntax=docker/dockerfile:1

# --- compilacion -----------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Directory.Build.props ./
COPY src/Collective.Api/Collective.Api.csproj src/Collective.Api/
RUN dotnet restore src/Collective.Api/Collective.Api.csproj

COPY src/ src/
RUN dotnet publish src/Collective.Api/Collective.Api.csproj \
    -c Release -o /app --no-restore

# --- ejecucion -------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Minimo privilegio: la imagen de aspnet ya trae el usuario "app" sin root.
USER app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_TieredPGO=1

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD ["dotnet", "Collective.Api.dll", "--healthcheck"]

ENTRYPOINT ["dotnet", "Collective.Api.dll"]
