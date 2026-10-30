FROM mcr.microsoft.com/dotnet/sdk:10.0.300-noble AS build
WORKDIR /source

COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj src/Api/
RUN dotnet restore src/Api/Api.csproj

COPY src/Domain/ src/Domain/
COPY src/Application/ src/Application/
COPY src/Infrastructure/ src/Infrastructure/
COPY src/Api/ src/Api/
RUN dotnet publish src/Api/Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13-noble AS runtime
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl ffmpeg fontconfig fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Api.dll"]
