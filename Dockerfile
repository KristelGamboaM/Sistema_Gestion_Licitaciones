# Dockerfile multi-stage genérico para los tres ejecutables de la solución
# (Licitaciones.Web, Licitaciones.Api, Licitaciones.Migrator). El proyecto a
# publicar se selecciona con --build-arg PROJECT=Licitaciones.Web (ver
# docker-compose.yml), evitando duplicar tres Dockerfiles casi idénticos.

ARG PROJECT=Licitaciones.Web

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG PROJECT
WORKDIR /src

# Copiar solo los archivos de proyecto primero para aprovechar la caché de
# capas de Docker: `dotnet restore` solo se reejecuta si cambian los .csproj.
COPY Directory.Packages.props ./
COPY src/Licitaciones.Domain/*.csproj src/Licitaciones.Domain/
COPY src/Licitaciones.Application/*.csproj src/Licitaciones.Application/
COPY src/Licitaciones.Infrastructure/*.csproj src/Licitaciones.Infrastructure/
COPY src/Licitaciones.Web/*.csproj src/Licitaciones.Web/
COPY src/Licitaciones.Api/*.csproj src/Licitaciones.Api/
COPY src/Licitaciones.Migrator/*.csproj src/Licitaciones.Migrator/
RUN dotnet restore "src/${PROJECT}/${PROJECT}.csproj"

COPY src/ src/
RUN dotnet publish "src/${PROJECT}/${PROJECT}.csproj" -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
ARG PROJECT
ENV PROJECT_DLL=${PROJECT}.dll
WORKDIR /app

# curl: usado únicamente por los healthcheck de docker-compose/Kubernetes.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Usuario no privilegiado (spec §13.1).
RUN adduser --disabled-password --gecos "" --uid 5678 appuser
COPY --from=build /app .
RUN chown -R appuser:appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["sh", "-c", "exec dotnet ${PROJECT_DLL}"]
