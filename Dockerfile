FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MotoRental.API/MotoRental.API.csproj", "MotoRental.API/"]
COPY ["src/MotoRental.Application/MotoRental.Application.csproj", "MotoRental.Application/"]
COPY ["src/MotoRental.Domain/MotoRental.Domain.csproj", "MotoRental.Domain/"]
COPY ["src/MotoRental.Infrastructure/MotoRental.Infrastructure.csproj", "MotoRental.Infrastructure/"]
RUN dotnet restore "MotoRental.API/MotoRental.API.csproj"
COPY src/ ./
WORKDIR "/src/MotoRental.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MotoRental.API.dll"]