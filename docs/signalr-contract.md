# Contrato SignalR de telemetria

## Conexão

- Hub único: `/api/telemetriaHub`
- Transporte: SignalR
- Eventos enviados pelo servidor:
  - `ReceberTelemetria`
  - `ReceberLoteTelemetria`

Os dois eventos usam o mesmo objeto `TelemetriaRealtimeDto`. O evento em lote
recebe uma coleção desses objetos; o evento individual recebe um único objeto.

## Payload de telemetria

Cada item contém:

| Campo | Tipo | Semântica |
|---|---|---|
| `sensorId` | string | Identificador canônico do sensor |
| `espMacAddress` | string | MAC do ESP que originou a leitura |
| `numeroEscova` | inteiro | Número da escova |
| `valorCelsius` | número | Temperatura sem arredondamento prévio |
| `dataHora` | ISO 8601 | Timestamp com offset ou UTC explícito |
| `alertaDesgaste` | booleano | Indica se algum gate disparou |
| `motorId` | inteiro nulo | Motor vinculado, quando existir |
| `resultadosQualityGates` | array | Todos os resultados aplicáveis; vazio quando nenhum gate se aplica |

`resultadosQualityGates` nunca deve ser omitido ou enviado como `null` em um
evento normal. Os campos `valorObservado`, `valorLimite` e `metadados` podem
ser `null`.

## Resultado de Quality Gate

Cada resultado contém `regraQualidadeId`, `nomeRegra`, `disparouAlerta`,
`severidade`, `tipoRegra`, `codigoRegra`, `mensagemDiagnostico`,
`valorObservado`, `valorLimite` e `metadados`.

Os valores públicos de `severidade` são:

- `Normal`
- `Atencao`
- `AlertaPreditivo`
- `Critico`

As avaliações são eventos transitórios. O backend não persiste histórico de
resultados nesta versão.

## Distribuição dos eventos

Nesta versão, a telemetria é publicada em broadcast global. Todos os clientes
conectados recebem os eventos e o frontend é responsável por filtrar os itens
do motor atualmente exibido usando `motorId`.

O hub mantém `AssinarMotor` e `DesassinarMotor` para uma evolução futura, mas
esses grupos não restringem a publicação enquanto a estratégia global estiver
em vigor.

## Compatibilidade

Clientes antigos que ignoram `resultadosQualityGates` continuam compatíveis.
Clientes novos devem tratar a propriedade como uma lista sempre presente. Não
há um segundo hub ou evento específico para Quality Gates.
