FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["HabitTracker.Server/HabitTracker.Server.csproj", "HabitTracker.Server/"]
COPY ["HabitTracker.Shared/HabitTracker.Shared.csproj", "HabitTracker.Shared/"]
RUN dotnet restore "HabitTracker.Server/HabitTracker.Server.csproj"
COPY . .
WORKDIR "/src/HabitTracker.Server"
RUN dotnet build "HabitTracker.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HabitTracker.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HabitTracker.Server.dll"]