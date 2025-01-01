# Use the official ASP.NET Core runtime Data as the base Data for the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 300
EXPOSE 304
EXPOSE 302/udp


# Use the SDK Data to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the .csproj files and restore dependencies
COPY ["Server/SignalRHub.Server.csproj", "Server/"]
COPY ["Client/SignalRHub.Client.csproj", "Client/"]
COPY ["SignalRServer/SignalRServer.csproj", "SignalRServer/"]
COPY ["CoreTCP.CoreUDP/CoreTCP.CoreUDP.csproj", "CoreTCP.CoreUDP/"]
COPY ["SignalRClient/SignalRClient.csproj", "SignalRClient/"]
COPY ["PromethusClient/PromethusClient.csproj", "PromethusClient/"]
COPY ["RedisClient/RedisClient.csproj", "RedisClient/"]
COPY ["SihnalRHub.Infrastructure/SihnalRHub.Infrastructure.csproj", "SihnalRHub.Infrastructure/"]
COPY ["SignalRHub.Modals/SignalRHub.Modals.csproj", "SignalRHub.Modals/"]
RUN dotnet restore "./Server/SignalRHub.Server.csproj"

# Copy everything else and build the app
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "./SignalRHub.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the app as a standalone Blazor WebAssembly app
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SignalRHub.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage - run the app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRHub.Server.dll"]
