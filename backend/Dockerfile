# ---------- restore ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY MomVibe.sln ./
COPY src/MomVibe.Domain/MomVibe.Domain.csproj              src/MomVibe.Domain/
COPY src/MomVibe.Application/MomVibe.Application.csproj    src/MomVibe.Application/
COPY src/MomVibe.Infrastructure/MomVibe.Infrastructure.csproj src/MomVibe.Infrastructure/
COPY src/MomVibe.WebApi/MomVibe.WebApi.csproj              src/MomVibe.WebApi/

RUN dotnet restore

# ---------- build ----------
FROM restore AS build
COPY src/ src/
RUN dotnet publish src/MomVibe.WebApi/MomVibe.WebApi.csproj \
    -c Release -o /app/publish --no-restore

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MomVibe.WebApi.dll"]
