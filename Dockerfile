# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY OdevAPI/*.csproj ./OdevAPI/
RUN dotnet restore OdevAPI/OdevAPI.csproj

# Copy everything and build
COPY . .
WORKDIR /src/OdevAPI
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "OdevAPI.dll"]
