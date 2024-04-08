FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["actorSystem/actorSystem.csproj", "actorSystem/"]
RUN dotnet restore "actorSystem/actorSystem.csproj"

COPY . .
WORKDIR /source/actorSystem
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "actorSystem.dll"]