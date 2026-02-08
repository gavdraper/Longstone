FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/Longstone.Domain/Longstone.Domain.csproj src/Longstone.Domain/
COPY src/Longstone.Application/Longstone.Application.csproj src/Longstone.Application/
COPY src/Longstone.Infrastructure/Longstone.Infrastructure.csproj src/Longstone.Infrastructure/
COPY src/Longstone.ServiceDefaults/Longstone.ServiceDefaults.csproj src/Longstone.ServiceDefaults/
COPY src/Longstone.Web/Longstone.Web.csproj src/Longstone.Web/

RUN dotnet restore src/Longstone.Web/Longstone.Web.csproj

COPY src/ src/

RUN dotnet publish src/Longstone.Web/Longstone.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S longstone && adduser -S longstone -G longstone

COPY --from=build /app/publish .

RUN mkdir -p /app/data && chown -R longstone:longstone /app/data
VOLUME /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__Longstone="Data Source=/app/data/longstone.db;Cache=Shared"

EXPOSE 8080

USER longstone

ENTRYPOINT ["dotnet", "Longstone.Web.dll"]
