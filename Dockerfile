# syntax=docker/dockerfile:1

# ---- Build & publish ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy every project file first so `dotnet restore` can be cached independently of source changes.
COPY MaisonAeternum.Domain/*.csproj MaisonAeternum.Domain/
COPY MaisonAeternum.Application/*.csproj MaisonAeternum.Application/
COPY MaisonAeternum.Infrastructure/*.csproj MaisonAeternum.Infrastructure/
COPY MaisonAeternum.Common/*.csproj MaisonAeternum.Common/
COPY g2soire/*.csproj g2soire/

RUN dotnet restore g2soire/MaisonAeternum.Web.csproj

COPY . .
RUN dotnet publish g2soire/MaisonAeternum.Web.csproj -c Release -o /app/publish --no-restore

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "MaisonAeternum.Web.dll"]
