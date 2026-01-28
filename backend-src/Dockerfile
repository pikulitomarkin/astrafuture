# Dockerfile para AstraFuture Backend (.NET 10)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de solução primeiro
COPY ["AstraFuture.sln", "./"]

# Copiar todos os .csproj explicitamente
COPY ["AstraFuture.Api/AstraFuture.Api.csproj", "AstraFuture.Api/"]
COPY ["AstraFuture.Application/AstraFuture.Application.csproj", "AstraFuture.Application/"]
COPY ["AstraFuture.Domain/AstraFuture.Domain.csproj", "AstraFuture.Domain/"]
COPY ["AstraFuture.Infrastructure/AstraFuture.Infrastructure.csproj", "AstraFuture.Infrastructure/"]
COPY ["AstraFuture.Shared/AstraFuture.Shared.csproj", "AstraFuture.Shared/"]

# Copiar código fonte de cada projeto
COPY ["AstraFuture.Api/", "AstraFuture.Api/"]
COPY ["AstraFuture.Application/", "AstraFuture.Application/"]
COPY ["AstraFuture.Domain/", "AstraFuture.Domain/"]
COPY ["AstraFuture.Infrastructure/", "AstraFuture.Infrastructure/"]
COPY ["AstraFuture.Shared/", "AstraFuture.Shared/"]

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

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "AstraFuture.Api.dll"]
