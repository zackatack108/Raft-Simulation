#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

RUN mkdir -p /RaftLogs && chown -R app:app /RaftLogs && chmod -R 777 /RaftLogs

USER app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Node-API/Node-API.csproj", "Node-API/"]
RUN pwd
COPY ["../Raft-Shared/Raft-Shared.csproj", "Raft-Shared/"]
RUN dotnet restore "./Node-API/./Node-API.csproj"
COPY . .
WORKDIR "/src/Node-API"
RUN dotnet build "./Node-API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Node-API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .