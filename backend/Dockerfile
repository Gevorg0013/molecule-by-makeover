FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MoleculeByMakeover.sln ./
COPY src/Domain/MoleculeByMakeover.Domain.csproj src/Domain/
COPY src/Shared/MoleculeByMakeover.Shared.csproj src/Shared/
COPY src/Application/MoleculeByMakeover.Application.csproj src/Application/
COPY src/Infrastructure/MoleculeByMakeover.Infrastructure.csproj src/Infrastructure/
COPY src/API/MoleculeByMakeover.API.csproj src/API/
RUN dotnet restore src/API/MoleculeByMakeover.API.csproj

COPY src/ src/
RUN dotnet publish src/API/MoleculeByMakeover.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && addgroup --system --gid 1000 appgroup \
    && adduser --system --uid 1000 --ingroup appgroup appuser

COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads /app/logs && chown -R appuser:appgroup /app

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MoleculeByMakeover.API.dll"]
