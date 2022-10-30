#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG NET_IMAGE=6.0-bullseye-slim-arm64v8
FROM mcr.microsoft.com/dotnet/aspnet:${NET_IMAGE} AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:${NET_IMAGE} AS build
WORKDIR /src

COPY ["greetings-camunda.csproj", "./"]
#COPY ["Nuget.config", "./"]
RUN dotnet restore "./greetings-camunda.csproj"

COPY . .
RUN dotnet build "./greetings-camunda.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish --no-restore "./greetings-camunda.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "greetings-camunda.dll"]