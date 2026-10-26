# Hackatohm Backend

API responsável pelo cadastro de motores e sensores, ingestão de telemetria,
avaliação de regras de qualidade e atualização do painel em tempo real.

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
dotnet ef database update --project Hackatohm.Api/Hackatohm.Api.csproj
```

Execute a API:

```powershell
dotnet run --project Hackatohm.Api/Hackatohm.Api.csproj
```

O perfil HTTP de desenvolvimento disponibiliza a API em
`http://localhost:5281`. O documento OpenAPI está disponível em
`http://localhost:5281/openapi/v1.json` durante o desenvolvimento.

## Estrutura

```text
Hackatohm.Api/
├── Controllers/       Endpoints HTTP
├── Data/              AppDbContext, repositórios e unidade de trabalho
├── DTOs/              Contratos HTTP e SignalR
├── Entities/          Entidades persistidas
├── Errors/            Catálogo de erros e tratamento de exceções
├── Hubs/              Hub e contratos de telemetria em tempo real
├── Migrations/        Histórico do schema do PostgreSQL
├── QualityGates/      Avaliador e estratégias de regras
├── Queues/            Fila em memória para telemetria recebida
├── UseCases/          Casos de uso da aplicação
└── Workers/           Processamento em background
```

## Endpoints principais

### Motores e sensores

| Método | Caminho | Descrição |
|---|---|---|
| `POST` | `/api/motores` | Cadastra um motor e seus sensores iniciais |
| `GET` | `/api/motores` | Lista motores |
| `GET` | `/api/motores/{id}` | Obtém um motor e seus sensores |
| `DELETE` | `/api/motores/{id}` | Exclui um motor |
| `GET` | `/api/sensores/{id}` | Obtém um sensor |
| `POST` | `/api/sensores` | Cadastra um sensor |
| `POST` | `/api/motores/sensores/{sensorId}/ping` | Atualiza os dados de conexão do sensor |

### Telemetria

| Método | Caminho | Descrição |
|---|---|---|
| `POST` | `/api/leituras` | Valida e enfileira um lote de telemetria |
| `GET` | `/api/leituras` | Consulta a leitura mais recente |

O POST de telemetria retorna `202 Accepted` quando o lote entra na fila.
Validação, avaliação dos Quality Gates, persistência e publicação SignalR
ocorrem depois, no worker.

### Regras de qualidade

| Método | Caminho | Descrição |
|---|---|---|
| `POST` | `/api/regras-qualidade` | Cria uma regra de limiar térmico |
| `GET` | `/api/regras-qualidade` | Lista o catálogo de regras |
| `GET` | `/api/regras-qualidade/{id}` | Obtém uma regra |
| `PUT` | `/api/regras-qualidade/{id}` | Edita uma regra |
| `DELETE` | `/api/regras-qualidade/{id}` | Exclui a regra e seus vínculos |
| `GET` | `/api/motores/{motorId}/regras-qualidade` | Lista os vínculos do motor |
| `GET` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Obtém um vínculo |
| `POST` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Associa uma regra ao motor |
| `PUT` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Ativa ou desativa o vínculo |
| `DELETE` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | Remove somente o vínculo |

Atualmente, o catálogo aceita a estratégia `temperatura.limiar_estatico`.
Os resultados dos gates são incluídos nos eventos SignalR, mas não são
persistidos como histórico.

## Atualizações em tempo real

O hub SignalR está disponível em `/api/telemetriaHub`. Os eventos enviados aos
clientes são `ReceberLoteTelemetria` e `ReceberTelemetria`. Os DTOs incluem as
leituras e os resultados das regras avaliadas.

## Migrations

Para criar uma migration:

```powershell
dotnet ef migrations add NomeDaMigration --project Hackatohm.Api/Hackatohm.Api.csproj
```

Para aplicar migrations ao banco configurado:

```powershell
dotnet ef database update --project Hackatohm.Api/Hackatohm.Api.csproj
```

Revise o código da migration antes de aplicá-la a ambientes compartilhados ou
de produção.

## Build

Na raiz do repositório:

```powershell
dotnet build Hackatohm.Api/Hackatohm.Api.csproj
```

## Observações operacionais

- A fila de telemetria é mantida em memória. Uma reinicialização do processo
  pode perder mensagens aceitas que ainda não foram processadas.
- O backend não autentica nem autoriza chamadas neste momento. Não exponha a
  API publicamente sem definir controles de acesso.
- Ajuste CORS para as origens reais do painel antes de implantar o serviço.
- Configure credenciais reais por variáveis de ambiente, User Secrets ou um
  gerenciador de segredos. Nunca inclua credenciais em arquivos versionados.
