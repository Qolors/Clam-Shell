#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ClamShell/ClamShell.Bot.csproj", "ClamShell/"]
# Ensure the file exists before copying
COPY ["ClamShell.MessageBus/ClamShell.MessageBus.csproj", "ClamShell.MessageBus/"]
RUN dotnet restore "ClamShell/ClamShell.Bot.csproj"
COPY . .
WORKDIR "/src/ClamShell"
RUN dotnet build "ClamShell.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClamShell.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClamShell.Bot.dll"]

