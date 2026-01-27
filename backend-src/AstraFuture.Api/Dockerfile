# Dockerfile para AstraFuture Backend (.NET 10)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todos os arquivos de projeto (paths relativos a backend-src/)
COPY ["AstraFuture.Api/AstraFuture.Api.csproj", "AstraFuture.Api/"]
COPY ["AstraFuture.Application/AstraFuture.Application.csproj", "AstraFuture.Application/"]
COPY ["AstraFuture.Domain/AstraFuture.Domain.csproj", "AstraFuture.Domain/"]
COPY ["AstraFuture.Infrastructure/AstraFuture.Infrastructure.csproj", "AstraFuture.Infrastructure/"]
COPY ["AstraFuture.Shared/AstraFuture.Shared.csproj", "AstraFuture.Shared/"]

# Restaurar dependências
RUN dotnet restore "AstraFuture.Api/AstraFuture.Api.csproj"

# Copiar todo o código fonte
COPY . .

# Build e publish
WORKDIR "/src/AstraFuture.Api"
RUN dotnet publish "AstraFuture.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "AstraFuture.Api.dll"]
