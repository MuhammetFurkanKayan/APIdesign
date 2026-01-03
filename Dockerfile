FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OdevAPI/OdevAPI.csproj", "./"]
RUN dotnet restore "OdevAPI.csproj"
COPY . .
WORKDIR /src/OdevAPI
RUN dotnet build "OdevAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OdevAPI.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM build AS migrations
RUN dotnet tool install dotnet-ef \
    --tool-path /tools \
    --version 9.0.*
ENV PATH="$PATH:/tools"
RUN dotnet ef migrations bundle \
    --project OdevAPI.csproj \
    --startup-project OdevAPI.csproj \
    --configuration Release \
    --output /app/efbundle

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=migrations /app/efbundle /app/efbundle
RUN chmod +x /app/efbundle
ENTRYPOINT ["dotnet", "OdevAPI.dll"]
