FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH

COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["NuGet.config", "NuGet.config"]

WORKDIR /src
COPY ["src/Payroc.LoadBalancer.WorkerService", "Payroc.LoadBalancer.WorkerService"]
COPY ["src/Payroc.LoadBalancer.Core", "Payroc.LoadBalancer.Core"]

RUN dotnet restore "Payroc.LoadBalancer.WorkerService/Payroc.LoadBalancer.WorkerService.csproj" -a $TARGETARCH
RUN dotnet publish "Payroc.LoadBalancer.WorkerService/Payroc.LoadBalancer.WorkerService.csproj" -a $TARGETARCH -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Payroc.LoadBalancer.WorkerService.dll"]
