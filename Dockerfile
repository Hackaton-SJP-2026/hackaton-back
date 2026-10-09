# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Camada de restore reaproveitável quando apenas o código mudar.
COPY NuGet.config ./
COPY Hackaton.Domain/Hackaton.Domain.csproj Hackaton.Domain/
COPY Hackaton.Application/Hackaton.Application.csproj Hackaton.Application/
COPY Hackaton.Infrastructure/Hackaton.Infrastructure.csproj Hackaton.Infrastructure/
COPY HackatonApi/HackatonApi.csproj HackatonApi/
RUN dotnet restore HackatonApi/HackatonApi.csproj

COPY . .
RUN dotnet publish HackatonApi/HackatonApi.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# app é o usuário sem privilégios das imagens oficiais .NET 8.
# Volume nomeado novo herda as permissões do diretório /data.
RUN mkdir -p /data && chown app:app /data
ENV ASPNETCORE_ENVIRONMENT=Production     ASPNETCORE_HTTP_PORTS=8080     Persistence__Enabled=true     ConnectionStrings__DefaultConnection="Data Source=/data/hackaton.db"     Http__RedirectToHttps=false
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "HackatonApi.dll"]
