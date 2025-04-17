# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DogWalk_API/DogWalk_API.csproj", "DogWalk_API/"]
COPY ["DogWalk_Application/DogWalk_Application.csproj", "DogWalk_Application/"]
COPY ["DogWalk_Domain/DogWalk_Domain.csproj", "DogWalk_Domain/"]
COPY ["DogWalk_Infrastructure/DogWalk_Infrastructure.csproj", "DogWalk_Infrastructure/"]
RUN dotnet restore "DogWalk_API/DogWalk_API.csproj"
COPY . .
WORKDIR "/src/DogWalk_API"
RUN dotnet build "DogWalk_API.csproj" -c Release -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
RUN dotnet publish "DogWalk_API.csproj" -c Release -o /app/publish

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DogWalk_API.dll"]