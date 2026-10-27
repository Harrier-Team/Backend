# Rotas da API

Este catálogo reflete as rotas expostas pelos controllers do backend. Os
contratos dos corpos e das respostas podem ser consultados no documento OpenAPI
durante a execução da API em desenvolvimento:
`http://localhost:5281/openapi/v1.json`.

## Motores

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `POST` | `/api/motores` | `201` | Cadastra um motor e seus sensores iniciais. |
| `GET` | `/api/motores` | `200` | Lista os motores cadastrados. |
| `GET` | `/api/motores/{id}` | `200` | Obtém um motor pelo identificador inteiro. |
| `GET` | `/api/motores/{id}/status` | `200` | Obtém o status agregado das escovas e Quality Gates do motor. |
| `DELETE` | `/api/motores/{id}` | `204` | Exclui um motor pelo identificador inteiro. |

Os endpoints de consulta e exclusão retornam `404` quando o motor não existe.

## Sensores

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `POST` | `/api/sensores` | `201` | Cadastra um sensor associado a um motor. |
| `GET` | `/api/sensores/{sensorId}` | `200` | Obtém um sensor pelo identificador textual. |
| `POST` | `/api/motores/sensores/{sensorId}/ping` | `200` | Atualiza a última conexão e o endereço IP observado. |

As rotas retornam `404` quando o sensor ou o motor referenciado não existe.

## Telemetria e leituras

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `POST` | `/api/leituras` | `202` | Valida e enfileira um lote enviado pelo dispositivo. |
| `GET` | `/api/leituras` | `200` | Consulta a leitura mais recente por sensor e reavalia seus Quality Gates. |

O POST retorna `400` para payload inválido e `503` quando a fila está cheia.
Nesse último caso, a resposta inclui `Retry-After: 5`. A avaliação dos gates, a
persistência e a publicação SignalR acontecem de forma assíncrona.

A resposta do `GET /api/leituras` inclui `motorId` e
`resultadosQualityGates`. Os gates são reavaliados usando a última leitura
persistida e as regras atualmente ativas do motor. A lista sempre é retornada,
inclusive vazia quando não existem regras aplicáveis.

Envie `espMacAddress` e `numeroEscova` na query do GET. Exemplo:
`GET /api/leituras?espMacAddress=AA%3ABB%3ACC%3ADD%3AEE%3AFF&numeroEscova=1`.
Ausência de sensor/leitura retorna 404.

O POST recebe `espMacAddress` e `sensores: [{numeroEscova, temperatura}]`.
O ESP deve enviar todos os sensores cadastrados de um único motor no mesmo
lote. Sensor desconhecido, lote incompleto ou mistura de motores são
descartados pelo worker após o 202, com log. O ESP não espera confirmação da
gravação. Consulte os exemplos completos em
[`Hackatohm.Api.http`](../Hackatohm.Api/Hackatohm.Api.http).

A reavaliação no GET reconstrói o gradiente com a leitura anterior e a
assimetria com o lote persistido do mesmo motor/timestamp. O booleano
`alertaDesgaste` da resposta é recalculado junto dos gates. Detalhes e casos
sem amostras suficientes estão em [api-compatibility.md](api-compatibility.md).

## Catálogo de Quality Gates

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `POST` | `/api/regras-qualidade` | `201` | Cria uma regra de qualidade. |
| `GET` | `/api/regras-qualidade` | `200` | Lista o catálogo de regras. |
| `GET` | `/api/regras-qualidade/{id}` | `200` | Obtém uma regra pelo UUID. |
| `PUT` | `/api/regras-qualidade/{id}` | `200` | Edita uma regra pelo UUID. |
| `DELETE` | `/api/regras-qualidade/{id}` | `204` | Exclui uma regra e seus vínculos. |

O campo `tipoRegra` aceita `temperatura.limiar_estatico`,
`temperatura.gradiente_termico`, `temperatura.assimetria_escovas` e
`temperatura.previsao_tendencia`.
Os campos `parametros.aviso` e `parametros.critico` devem ser finitos e ter
`aviso` menor que `critico`. Eles representam, respectivamente, temperatura em
°C, taxa de subida em °C/minuto ou diferença entre escovas em °C, conforme o
tipo escolhido. Na previsão por tendência a relação se inverte e as unidades
mudam: ambos são minutos de antecedência, com `aviso` maior que `critico`, e o
corpo exige ainda `parametros.temperaturaLimite` (°C, de -55 a 200), com
`parametros.janela` opcional (5 a 60 leituras, padrão 12). Esses dois campos
extras só são aceitos nesse tipo.

No tipo estático, ambos os limites ficam entre -55 e 200 °C. Nos demais,
ambos são não negativos. Comparações usam `>=`: igualdade ao limite já aciona
o respectivo nível. O aviso gera `AlertaPreditivo` e o crítico gera `Critico`.

## Alertas de manutenção

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `GET` | `/api/alertas` | `200` | Lista os alertas de manutenção, do mais recente ao mais antigo. |

Filtros opcionais: `motorId`, `ativo`, `categoria` (`Preditivo`, `Aceleracao` ou
`Condicao`), `tipoRegra`, `severidade`, `pagina` (a partir de 1) e `tamanho` (1 a
200, padrão 50). A resposta é `{ itens, pagina, tamanho, total }`; parâmetros
inválidos retornam `400`. Campos e semântica em
[api-compatibility.md](api-compatibility.md).

## Vínculos entre motores e regras

| Método | Rota | Sucesso | Descrição |
|---|---|---:|---|
| `GET` | `/api/motores/{motorId}/regras-qualidade` | `200` | Lista as regras vinculadas ao motor. |
| `GET` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | `200` | Obtém um vínculo pelo motor e regra. |
| `POST` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | `201` | Cria um vínculo. |
| `PUT` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | `200` | Atualiza o estado ativo do vínculo. |
| `DELETE` | `/api/motores/{motorId}/regras-qualidade/{regraId}` | `204` | Remove somente o vínculo. |

Uma associação duplicada retorna `409 Conflict` com o código
`quality_rule_already_linked`.

## Infraestrutura

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/health/live` | Verifica se a aplicação e a fila estão disponíveis. |
| `GET` | `/health/ready` | Verifica PostgreSQL e fila para aceitar tráfego. |
| SignalR | `/api/telemetriaHub` | Endpoint de conexão do hub SignalR. |

O hub publica `ReceberLoteTelemetria` e `ReceberTelemetria`. O contrato desses
eventos, além de `ReceberStatusMotor` e `ReceberAlertaManutencao`, está em
[signalr-contract.md](signalr-contract.md).

## Erros

As rotas HTTP usam Problem Details, enquanto a ingestão de dispositivos usa o
formato compacto documentado em [api-compatibility.md](api-compatibility.md).
