# Hackatohm Backend

Backend da Hackatohm para monitoramento térmico de motores elétricos,
especialmente das escovas. O serviço recebe lotes de telemetria, avalia
Quality Gates, persiste as leituras e publica atualizações para o painel em
tempo real. Nesta etapa, o foco é entregar uma base funcional para manutenção
preditiva: além dos gates, o sistema registra eventos de alerta e estima por
tendência quando um limite será atingido. Modelos de machine learning ainda não
fazem parte do escopo implementado, e o resultado de cada gate por leitura não
é persistido.

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

As referências EF Core usadas pela API e infraestrutura estão alinhadas em
10.0.14, com provider Npgsql 10.0.3, para evitar conflitos de versões nos testes.

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
dotnet ef database update --project Hackatohm.Infrastructure/Hackatohm.Infrastructure.csproj --startup-project Hackatohm.Api/Hackatohm.Api.csproj
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

O contrato da demonstração é **um ESP enviando todos os sensores de um motor
no mesmo lote**. O dispositivo coleta e transmite; as decisões de alerta ficam
no backend. O objetivo da Hackatohm é demonstrar comunicação, amostragem no
painel e diagnóstico por regras térmicas. Consulte o guia de
[manutenção preditiva e amostragem](docs/monitoramento-preditivo.md) para as
fórmulas, a integração do frontend e os limites da demonstração.

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
| `GET` | `/api/motores/{id}/status` | Obtém o status agregado do motor |
| `GET` | `/api/alertas` | Lista os alertas de manutenção, com filtros e paginação |
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
A validação do payload ocorre antes do enfileiramento. O worker confere se
todos os sensores estão cadastrados, pertencem a um único motor e formam o
conjunto completo desse motor; depois avalia os Quality Gates, persiste e
publica via SignalR. Lotes incompatíveis com o cadastro são descartados com
registro no log. O ESP não precisa esperar confirmação de persistência.

O catálogo aceita `temperatura.limiar_estatico`,
`temperatura.gradiente_termico`, `temperatura.assimetria_escovas` e
`temperatura.previsao_tendencia`.
Telemetria e limites de regras estáticas aceitam **-55 a 200 °C**, inclusive.
Gradiente usa °C/minuto e assimetria usa diferença em °C; seus limites são
finitos e não negativos. Essas três exigem `aviso < critico`.

A previsão por tendência ajusta uma reta às últimas leituras de cada escova e
estima em quantos minutos ela atingirá `temperaturaLimite`. Nela, `aviso` e
`critico` são **minutos de antecedência**, com `aviso > critico`; `janela`
(5 a 60 leituras, padrão 12) é opcional. Exemplo de `parametros`:
`{"aviso": 30, "critico": 10, "temperaturaLimite": 95}`.

O GET da última leitura reconstrói o contexto com a amostra anterior do
sensor e as leituras do motor no mesmo instante do lote, reavaliando as regras
atualmente ativas. Assim, gradiente e assimetria também são calculados na
recarga do painel. O booleano `alertaDesgaste` da resposta acompanha essa
reavaliação; a flag armazenada no histórico permanece como foi na ingestão.

O GET de status do motor (`/api/motores/{id}/status`) consolida a última leitura
de cada sensor cadastrado: maior severidade, contagem por severidade, motivo
principal e situação (`Online`, `Degradado` ou `Offline`). O limite de silêncio
é configurado em `Telemetria:OfflineAfterSeconds` (padrão 120). Detalhes em
[docs/api-compatibility.md](docs/api-compatibility.md).

Cada lote processado também atualiza os alertas de manutenção: um alerta abre
quando um gate passa a disparar e é encerrado quando ele deixa de disparar. O
`GET /api/alertas` lista esses eventos, com filtro por motor, estado, categoria
(`Preditivo`, `Aceleracao` ou `Condicao`), tipo de regra e severidade. Detalhes em
[docs/api-compatibility.md](docs/api-compatibility.md).

## Atualizações em tempo real

O hub SignalR está disponível em `/api/telemetriaHub`. Os eventos enviados aos
clientes são `ReceberLoteTelemetria`, `ReceberTelemetria` e
`ReceberStatusMotor` (status agregado do motor, publicado após cada lote). Os DTOs incluem as
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
dotnet test Hackatohm.Api.UnitTests/Hackatohm.Api.UnitTests.csproj
```

`Hackatohm.Api.UnitTests` cobre regras, validadores, reconstrução do contexto
na consulta, lotes completos de um motor e o protocolo JSON registrado para o
SignalR. Os testes usam dados isolados em memória.

`Hackatohm.Api.IntegrationTests` é uma estrutura antiga sem testes
implementados. Testes de integração não fazem parte do escopo da Hackatohm.

O pipeline de CI executa restore, build e testes para cada push nas branches
principais e para cada pull request.

## Saúde e métricas

- `GET /health/live`: verifica se a aplicação está viva e se a fila está
  disponível.
- `GET /health/ready`: verifica a conexão com o PostgreSQL e o estado da fila.
- As métricas `hackatohm.telemetria.*` registram aceitas, rejeitadas,
  processadas e falhas através de `System.Diagnostics.Metrics`.
- `processadas` conta lotes salvos; descartes por cadastro/lote inválido e
  exceções entram em `falhas`. São contadores locais, sem confirmação ao ESP.

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
- Confirmação durável ao ESP, fila persistente, reenvio histórico, machine
  learning e um serviço separado de consolidação do motor ficam fora desta
  entrega. O painel pode resumir os resultados atuais pela maior severidade.
- Não há endpoint de histórico de leituras por período nesta versão: o frontend
  forma a janela do gráfico com os eventos recebidos durante a sessão. Só a
  lista de alertas (`/api/alertas`) é paginada.
