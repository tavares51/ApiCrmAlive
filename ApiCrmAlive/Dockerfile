# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Default container port for ASP.NET Core is 8080. Keep it explicit so reverse proxies / PaaS
# that rely on the image metadata don't end up forwarding to the wrong port.
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
# Apply EF Core migrations automatically on boot (implemented in Program.cs).
# Override at deploy-time if you don't want this behavior.
ENV APPLY_MIGRATIONS=true
ENV APPLY_MIGRATIONS_STRICT=true
EXPOSE 7879

# Run as non-root in the final image.
RUN mkdir -p /app && chown -R app:app /app
USER app


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiCrmAlive/ApiCrmAlive.csproj", "ApiCrmAlive/"]
RUN dotnet restore "./ApiCrmAlive/ApiCrmAlive.csproj"
COPY . .
WORKDIR "/src/ApiCrmAlive"
RUN dotnet build "./ApiCrmAlive.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiCrmAlive.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --chown=app:app --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiCrmAlive.dll"]
