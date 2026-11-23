# Etapa 1 compila o Blazor com o SDK do .NET 9; etapa 2 serve os arquivos estáticos com Nginx
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY SalaoAdmin/SalaoAdmin.csproj SalaoAdmin/
RUN dotnet restore SalaoAdmin/SalaoAdmin.csproj
COPY SalaoAdmin/ SalaoAdmin/
RUN dotnet publish SalaoAdmin/SalaoAdmin.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -qO- http://127.0.0.1/health || exit 1
