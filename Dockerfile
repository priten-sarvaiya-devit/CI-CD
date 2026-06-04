# ---------------------------------------------------------------------------
# Build stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first to leverage Docker layer caching on restore.
COPY CI-CD/CI-CD.csproj CI-CD/
RUN dotnet restore CI-CD/CI-CD.csproj

# Copy the rest of the source and publish.
COPY CI-CD/ CI-CD/
RUN dotnet publish CI-CD/CI-CD.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------------------------------------------------------------------------
# Runtime stage
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render injects $PORT; bind Kestrel to it. Default to 8080 for local runs.
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "CI-CD.dll"]
