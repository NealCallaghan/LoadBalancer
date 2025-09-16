FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH

COPY ["Directory.Build.targets", "Directory.Build.targets"]
COPY ["NuGet.config", "NuGet.config"]

WORKDIR /src
COPY ["src/Payroc.LoadBalancer.WorkerService", "Payroc.LoadBalancer.WorkerService"]
COPY ["src/Payroc.LoadBalancer.Core", "Payroc.LoadBalancer.Core"]

RUN dotnet restore "Payroc.LoadBalancer.WorkerService/Payroc.LoadBalancer.WorkerService.csproj" -a $TARGETARCH
RUN dotnet publish "Payroc.LoadBalancer.WorkerService/Payroc.LoadBalancer.WorkerService.csproj" -a $TARGETARCH -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

RUN adduser --disabled-password --shell /sbin/nologin -g "" -u 10001 dotnet && \
    chown -R dotnet /app

COPY --from=build /app/publish .

USER dotnet

ENTRYPOINT ["./Payroc.LoadBalancer.WorkerService"]
