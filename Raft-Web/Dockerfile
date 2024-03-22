#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Raft-Web/Raft-Web.csproj", "Raft-Web/"]
RUN pwd
COPY ["../Raft-Shared/Raft-Shared.csproj", "Raft-Shared/"]
RUN dotnet clean "./Raft-Web/./Raft-Web.csproj"
RUN dotnet restore "./Raft-Web/./Raft-Web.csproj"
COPY . .
WORKDIR "/src/Raft-Web"
RUN dotnet build "./Raft-Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

ARG GATEWAY_URL_ARG
ENV GATEWAY_URL=$GATEWAY_URL_ARG
RUN sed -i "s|\${GATEWAY_URL}|$GATEWAY_URL|g" wwwroot/appsettings.json

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Raft-Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY Raft-Web/nginx.conf /etc/nginx/nginx.conf
