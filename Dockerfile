# ── Stage 1: Build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (better layer caching)
COPY StudentManagementSystem.sln ./
COPY src/SMS.API/SMS.API.csproj              src/SMS.API/
COPY src/SMS.Application/SMS.Application.csproj  src/SMS.Application/
COPY src/SMS.Domain/SMS.Domain.csproj        src/SMS.Domain/
COPY src/SMS.Infrastructure/SMS.Infrastructure.csproj src/SMS.Infrastructure/
COPY src/SMS.Tests/SMS.Tests.csproj          src/SMS.Tests/

# Restore NuGet packages
RUN dotnet restore

# Copy all remaining source files
COPY . .

# Publish the API project in Release configuration
RUN dotnet publish src/SMS.API/SMS.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose HTTP port
EXPOSE 80

ENTRYPOINT ["dotnet", "SMS.API.dll"]
