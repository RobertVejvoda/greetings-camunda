#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0.12-bullseye-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src

COPY ["src/greetings-camunda.csproj", "src/"]
RUN dotnet restore "src/greetings-camunda.csproj"

COPY . .
WORKDIR "/src/src/"
RUN dotnet build "greetings-camunda.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "greetings-camunda.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "greetings-camunda.dll"]