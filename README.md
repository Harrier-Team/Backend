# Hackatohm Backend

Backend da Hackatohm para monitoramento térmico de motores elétricos,
especialmente das escovas. O serviço recebe lotes de telemetria, avalia
Quality Gates, persiste as leituras e publica atualizações para o painel em
tempo real. Nesta etapa, o foco é entregar uma base funcional para manutenção
preditiva; modelos de machine learning e histórico persistido dos resultados
dos gates ainda não fazem parte do escopo implementado.

## Tecnologias

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core e PostgreSQL
- SignalR
- FluentValidation
- Docker Compose para o PostgreSQL local

## Requisitos

- .NET SDK 10
- Docker Desktop com Docker Compose
- `dotnet-ef` 10 para criar ou aplicar migrations

## Configuração local

Na raiz deste repositório, crie o arquivo `.env` a partir do exemplo:

```powershell
Copy-Item .env.example .env
```

Altere `POSTGRES_PASSWORD` para uma senha local. O arquivo `.env` é ignorado
pelo Git e não deve ser versionado.

Inicie o banco:

```powershell
docker compose up -d postgres
```

Instale o Entity Framework Core CLI, caso ainda não esteja instalado:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

Se já houver outra versão global instalada, atualize-a:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.14
```

Crie ou atualize o schema do banco:

```powershell
dotnet ef database update `
  --project Hackatohm.Infrastructure/Hackatohm.Infrastructure.csproj `
  --startup-project Hackatohm.Api/Hackatohm.Api.csproj
```

Execute a API:

```powershell
dotnet run --project Hackatohm.Api/Hackatohm.Api.csproj
```

O perfil HTTP de desenvolvimento disponibiliza a API em
`http://localhost:5281`. O documento OpenAPI está disponível em
`http://localhost:5281/openapi/v1.json` durante o desenvolvimento.

## Escopo atual

- Cadastro de motores e sensores.
- Ingestão assíncrona de lotes enviados pelos dispositivos.
- Persistência das leituras no PostgreSQL.
- Avaliação de regras de qualidade térmica configuráveis por motor.
- Consulta da leitura mais recente de um sensor.
- Publicação de telemetria e resultados dos gates via SignalR.
- Health checks, métricas básicas e documentação OpenAPI em desenvolvimento.

O envio SignalR permanece em broadcast global por simplicidade e prazo da
Hackatohm. O frontend deve filtrar os eventos recebidos pelo identificador do
motor. Assinatura de grupos existe no hub, mas não restringe o processamento
atual.

## Arquitetura e estrutura

```text
Hackatohm.Domain/
├── Entities/          Entidades e regras centrais
└── QualityGates/      Contratos, contexto e estratégias puras
Hackatohm.Application/
├── Abstractions/      Interfaces de persistência
└── QualityGates/      Avaliação e composição dos Quality Gates
Hackatohm.Infrastructure/
└── Persistence/       EF Core, repositórios e migrations
Hackatohm.Api/
├── Controllers/       Endpoints HTTP
├── DTOs/              Contratos HTTP e SignalR
├── Errors/            Catálogo de erros e tratamento de exceções
├── Hubs/              Hub e contratos de tempo real
├── Queues/            Fila em memória de telemetria
├── UseCases/          Orquestração dos fluxos HTTP e ingestão
└── Workers/           Processamento em background
```

A composição segue o sentido `Api -> Application -> Domain` e
`Api -> Infrastructure -> Application/Domain`. O domínio não conhece ASP.NET
Core, EF Core, SignalR ou PostgreSQL. Como o projeto ainda prioriza a entrega
da Hackatohm, os adaptadores HTTP, SignalR e o processamento assíncrono
continuam concentrados na API.

## Rotas da API

As rotas estão catalogadas em
[docs/api-routes.md](docs/api-routes.md). O documento OpenAPI gerado durante o
desenvolvimento também pode ser consultado em
`http://localhost:5281/openapi/v1.json`.

### Resumo

| Método | Caminho | Descrição |
|---|---|---|
| `POST` | `/api/motores` | Cadastra um motor e seus sensores iniciais |
| `GET` | `/api/motores` | Lista motores |
| `GET` | `/api/motores/{id}` | Obtém um motor |
| `DELETE` | `/api/motores/{id}` | Exclui um motor |
| `POST` | `/api/sensores` | Cadastra um sensor |
| `GET` | `/api/sensores/{sensorId}` | Obtém um sensor |
| `POST` | `/api/motores/sensores/{sensorId}/ping` | Registra o ping de um sensor |
| `POST` | `/api/leituras` | Valida e enfileira um lote de telemetria |
| `GET` | `/api/leituras` | Consulta a leitura mais recente e seus Quality Gates |
| `POST` | `/api/regras-qualidade` | Cria uma regra de qualidade |
| `GET` | `/api/regras-qualidade` | Lista as regras |
| `GET` | `/api/regras-qualidade/{id}` | Obtém uma regra |
| `PUT` | `/api/regras-qualidade/{id}` | Edita uma regra |
| `DELETE` | `/api/regras-qualidade/{id}` | Exclui uma regra e seus vínculos |
| `GET` | `/api/motores/{motorId}/regras-qualidade` | Lista vínculos |
| `GET` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Obtém um vínculo |
| `POST` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Cria um vínculo |
| `PUT` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Atualiza o vínculo |
| `DELETE` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Remove um vínculo |

O POST de telemetria retorna `202 Accepted` quando o lote entra na fila.
Validação, avaliação dos Quality Gates, persistência e publicação SignalR
ocorrem depois, no worker. O catálogo aceita atualmente a estratégia
`temperatura.limiar_estatico`.

## Atualizações em tempo real

O hub SignalR está disponível em `/api/telemetriaHub`. Os eventos enviados aos
clientes são `ReceberLoteTelemetria` e `ReceberTelemetria`. Os DTOs incluem as
leituras e os resultados das regras avaliadas. O contrato completo está em
[docs/signalr-contract.md](docs/signalr-contract.md).

## Migrations

Para criar uma migration:

```powershell
dotnet ef migrations add NomeDaMigration `
  --project Hackatohm.Infrastructure/Hackatohm.Infrastructure.csproj `
  --startup-project Hackatohm.Api/Hackatohm.Api.csproj
```

Para aplicar migrations ao banco configurado:

```powershell
dotnet ef database update `
  --project Hackatohm.Infrastructure/Hackatohm.Infrastructure.csproj `
  --startup-project Hackatohm.Api/Hackatohm.Api.csproj
```

Revise o código da migration antes de aplicá-la a ambientes compartilhados ou
de produção.

## Build

Na raiz do repositório:

```powershell
dotnet build Hackatohm.slnx
dotnet test Hackatohm.slnx
```

Os projetos de teste são organizados em:

- `Hackatohm.Api.UnitTests`: regras, validadores e componentes isolados;
- `Hackatohm.Api.IntegrationTests`: persistência, endpoints e integrações.

O pipeline de CI executa restore, build e testes para cada push nas branches
principais e para cada pull request.

## Saúde e métricas

- `GET /health/live`: verifica se a aplicação está viva e se a fila está
  disponível.
- `GET /health/ready`: verifica a conexão com o PostgreSQL e o estado da fila.
- As métricas `hackatohm.telemetria.*` registram aceitas, rejeitadas,
  processadas e falhas através de `System.Diagnostics.Metrics`.

## Contratos versionados

O contrato dos eventos SignalR está documentado em
[docs/signalr-contract.md](docs/signalr-contract.md). Regras de compatibilidade
HTTP e códigos de erro estão em
[docs/api-compatibility.md](docs/api-compatibility.md).

## Observações operacionais

- A fila de telemetria é mantida em memória. Uma reinicialização do processo
  pode perder mensagens aceitas que ainda não foram processadas.
- O backend não autentica nem autoriza chamadas neste momento. Não exponha a
  API publicamente sem definir controles de acesso.
- Ajuste CORS para as origens reais do painel antes de implantar o serviço.
- Configure credenciais reais por variáveis de ambiente, User Secrets ou um
  gerenciador de segredos. Nunca inclua credenciais em arquivos versionados.
- A capacidade da fila e o tamanho máximo do lote são configuráveis na seção
  `Telemetria` (`QueueCapacity` e `MaxBatchSize`).
