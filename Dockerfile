# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto para restaurar dependencias
COPY ["Entregas.API/Entregas.API.csproj", "Entregas.API/"]
COPY ["Entregas.Shared/Entregas.Shared.csproj", "Entregas.Shared/"]
RUN dotnet restore "Entregas.API/Entregas.API.csproj"

# Copiamos todo el código y compilamos
COPY . .
WORKDIR "/src" 
RUN dotnet publish "Entregas.API/Entregas.API.csproj" -c Release -o /app/publish

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponemos el puerto estándar de .NET 8/9 en Docker
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Entregas.API.dll"]