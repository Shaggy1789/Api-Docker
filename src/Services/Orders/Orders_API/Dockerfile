FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Services/Orders/Orders_API/Orders_API.csproj", "Services/Orders/Orders_API/"]
RUN dotnet restore "Services/Orders/Orders_API/Orders_API.csproj"
COPY . .
WORKDIR "/src/Services/Orders/Orders_API"
RUN dotnet build "Orders_API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders_API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Evitar el error de inotify en Render (límite de 128 instancias):
# - usa polling en vez de inotify para el FileSystemWatcher
# - deshabilita recarga de config en caliente (no es necesaria en produccion)
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders_API.dll"]
