FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY ["signalR/signalR.csproj", "signalR/"]
RUN dotnet restore "signalR/signalR.csproj"

COPY . .
WORKDIR /source/signalR
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "signalR.dll"]