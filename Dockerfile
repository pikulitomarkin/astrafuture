# Dockerfile para AstraFuture Backend (.NET 10) - Raiz do projeto - v2
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar tudo de uma vez
COPY . .

# Listar conteúdo para debug
RUN ls -la && ls -la backend-src/

# Restaurar e publicar
RUN dotnet restore "backend-src/AstraFuture.Api/AstraFuture.Api.csproj"
RUN dotnet publish "backend-src/AstraFuture.Api/AstraFuture.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copiar arquivos publicados
COPY --from=build /app/publish .

# Railway usa a variável PORT
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

ENTRYPOINT ["dotnet", "AstraFuture.Api.dll"]