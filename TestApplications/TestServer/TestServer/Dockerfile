FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj first and restore
COPY ["TestServer.csproj", "./"]
RUN dotnet restore "./TestServer.csproj"

# Copy the rest of the files
COPY . .

# Build
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "TestServer.dll"]
