FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TechDebtGame/TechDebtGame.csproj", "TechDebtGame/"]
RUN dotnet restore "TechDebtGame/TechDebtGame.csproj"
COPY . .
WORKDIR "/src/TechDebtGame"
RUN dotnet build "./TechDebtGame.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TechDebtGame.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TechDebtGame.dll"]
