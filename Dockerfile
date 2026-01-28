# Dockerfile para AstraFuture Backend (.NET 10) - Raiz do projeto
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de solução primeiro
COPY ["backend-src/AstraFuture.sln", "./"]

# Copiar todos os .csproj explicitamente
COPY ["backend-src/AstraFuture.Api/AstraFuture.Api.csproj", "AstraFuture.Api/"]
COPY ["backend-src/AstraFuture.Application/AstraFuture.Application.csproj", "AstraFuture.Application/"]
COPY ["backend-src/AstraFuture.Domain/AstraFuture.Domain.csproj", "AstraFuture.Domain/"]
COPY ["backend-src/AstraFuture.Infrastructure/AstraFuture.Infrastructure.csproj", "AstraFuture.Infrastructure/"]
COPY ["backend-src/AstraFuture.Shared/AstraFuture.Shared.csproj", "AstraFuture.Shared/"]

# Copiar código fonte de cada projeto
COPY ["backend-src/AstraFuture.Api/", "AstraFuture.Api/"]
COPY ["backend-src/AstraFuture.Application/", "AstraFuture.Application/"]
COPY ["backend-src/AstraFuture.Domain/", "AstraFuture.Domain/"]
COPY ["backend-src/AstraFuture.Infrastructure/", "AstraFuture.Infrastructure/"]
COPY ["backend-src/AstraFuture.Shared/", "AstraFuture.Shared/"]

# Restaurar e publicar
RUN dotnet restore "AstraFuture.Api/AstraFuture.Api.csproj"
RUN dotnet publish "AstraFuture.Api/AstraFuture.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "AstraFuture.Api.dll"]