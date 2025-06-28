# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SmartHomeAutomation.API/SmartHomeAutomation.API.csproj", "SmartHomeAutomation.API/"]
COPY ["SmartHomeAutomation.Core/SmartHomeAutomation.Core.csproj", "SmartHomeAutomation.Core/"]
COPY ["SmartHomeAutomation.Infrastructure/SmartHomeAutomation.Infrastructure.csproj", "SmartHomeAutomation.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SmartHomeAutomation.API/SmartHomeAutomation.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/SmartHomeAutomation.API"
RUN dotnet build "SmartHomeAutomation.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SmartHomeAutomation.API.csproj" -c Release -o /app/publish

# Final stage - runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "SmartHomeAutomation.API.dll"] 