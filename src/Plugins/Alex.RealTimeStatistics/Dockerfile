FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["./Alex.RealTimeStatistics.csproj", "app/"]
RUN dotnet restore "app/Alex.RealTimeStatistics.csproj"
COPY . .
RUN apt-get update -yq 
RUN apt-get install curl gnupg -yq 
RUN curl -sL https://deb.nodesource.com/setup_13.x | bash -
RUN apt-get install -y nodejs
RUN dotnet build "/src/Alex.RealTimeStatistics.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/src/Alex.RealTimeStatistics.csproj" -c Release -o /app/publish

FROM base AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Alex.RealTimeStatistics.dll"]
