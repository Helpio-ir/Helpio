# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Presentation/Helpio.Dashboard/Helpio.Dashboard.csproj src/Presentation/Helpio.Dashboard/
COPY src/Core/Helpio.Application/Helpio.Application.csproj src/Core/Helpio.Application/
COPY src/Core/Helpio.Domain/Helpio.Domain.csproj src/Core/Helpio.Domain/

RUN dotnet restore src/Presentation/Helpio.Dashboard/Helpio.Dashboard.csproj

COPY . .

RUN dotnet publish src/Presentation/Helpio.Dashboard/Helpio.Dashboard.csproj -c Release -o /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN useradd --create-home --home /home/app --shell /usr/sbin/nologin appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

EXPOSE 8080
USER appuser

ENTRYPOINT ["dotnet", "Helpio.Ir.Dashboard.dll"]
