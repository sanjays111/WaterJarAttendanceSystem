# ✅ Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# ✅ Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY WaterJarAttendanceSystem.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . . 
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ✅ Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Optional: Health check (uncomment if needed)
# HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
#   CMD curl --fail http://localhost:80/ || exit 1

# ✅ Entry point
ENTRYPOINT ["dotnet", "WaterJarAttendanceSystem.dll"]
