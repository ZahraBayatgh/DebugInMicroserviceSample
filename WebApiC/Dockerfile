#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebApiC/WebApiC.csproj", "WebApiC/"]
COPY ["Logging/Logging.csproj", "Logging/"]
RUN dotnet restore "WebApiC/WebApiC.csproj"
COPY . .
WORKDIR "/src/WebApiC"
RUN dotnet build "WebApiC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApiC.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApiC.dll"]