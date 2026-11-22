# Build multi-stage do Frontend Blazor WebAssembly (.NET 9) — sem alterar código-fonte
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
HEALTHCHECK --interval=15s --timeout=3s --retries=3 CMD wget -qO- http://localhost/health || exit 1
