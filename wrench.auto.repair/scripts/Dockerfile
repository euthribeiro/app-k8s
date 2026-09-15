FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./src/wrench.web.api/wrench.web.api.csproj", "src/wrench.web.api/"]
COPY ["./src/wrench.auto.repair.autenticacao.application/wrench.auto.repair.autenticacao.application.csproj", "src/wrench.auto.repair.autenticacao.application/"]
COPY ["./src/wrench.auto.repair.autenticacao.domain/wrench.auto.repair.autenticacao.domain.csproj", "src/wrench.auto.repair.autenticacao.domain/"]
COPY ["./src/wrench.auto.repair.autenticacao.infra/wrench.auto.repair.autenticacao.infra.csproj", "src/wrench.auto.repair.autenticacao.infra/"]
COPY ["./src/wrench.auto.repair.cadastro.application/wrench.auto.repair.cadastro.application.csproj", "src/wrench.auto.repair.cadastro.application/"]
COPY ["./src/wrench.auto.repair.cadastro.domain/wrench.auto.repair.cadastro.domain.csproj", "src/wrench.auto.repair.cadastro.domain/"]
COPY ["./src/wrench.auto.repair.cadastro.infra/wrench.auto.repair.cadastro.infra.csproj", "src/wrench.auto.repair.cadastro.infra/"]
COPY ["./src/wrench.auto.repair.core/wrench.auto.repair.core.csproj", "src/wrench.auto.repair.core/"]
COPY ["./src/wrench.auto.repair.estoque.application/wrench.auto.repair.estoque.application.csproj", "src/wrench.auto.repair.estoque.application/"]
COPY ["./src/wrench.auto.repair.estoque.domain/wrench.auto.repair.estoque.domain.csproj", "src/wrench.auto.repair.estoque.domain/"]
COPY ["./src/wrench.auto.repair.estoque.infra/wrench.auto.repair.estoque.infra.csproj", "src/wrench.auto.repair.estoque.infra/"]
COPY ["./src/wrench.auto.repair.ordem.servico.application/wrench.auto.repair.ordem.servico.application.csproj", "src/wrench.auto.repair.ordem.servico.application/"]
COPY ["./src/wrench.auto.repair.ordem.servico.domain/wrench.auto.repair.ordem.servico.domain.csproj", "src/wrench.auto.repair.ordem.servico.domain/"]
COPY ["./src/wrench.auto.repair.ordem.servico.infra/wrench.auto.repair.ordem.servico.infra.csproj", "src/wrench.auto.repair.ordem.servico.infra/"]
COPY ["./src/wrench.auto.repair.infra/wrench.auto.repair.infra.csproj", "src/wrench.auto.repair.infra/"]

RUN dotnet restore "./src/wrench.web.api/wrench.web.api.csproj"
COPY . .
WORKDIR "/src/src/wrench.web.api"
RUN dotnet build "./wrench.web.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./wrench.web.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "wrench.web.api.dll"]
