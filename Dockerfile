# Build context: the server/ folder (contains all projects).

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, using only the project files, so the restore layer is cached
COPY final_project_Core/final_project_Core.csproj final_project_Core/
COPY final_project_Data/final_project_Data.csproj final_project_Data/
COPY final_project_Service/final_project_Service.csproj final_project_Service/
COPY final_project_Api/final_project_API.csproj final_project_Api/
RUN dotnet restore final_project_Api/final_project_API.csproj

COPY . .
RUN dotnet publish final_project_Api/final_project_API.csproj -c Release -o /app --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

# Render provides the port in $PORT at runtime; fall back to 8080 locally
CMD ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet final_project_API.dll
