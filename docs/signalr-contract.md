# Contrato SignalR de telemetria

## Conexão

- Hub único: `/api/telemetriaHub`
- Transporte: SignalR
- Eventos enviados pelo servidor:
  - `ReceberTelemetria`
  - `ReceberLoteTelemetria`

Os dois eventos usam o mesmo objeto `TelemetriaRealtimeDto`. O evento em lote
recebe uma coleção desses objetos; o evento individual recebe um único objeto.
O fluxo de ingestão atual publica `ReceberLoteTelemetria`, uma vez por lote
completo de um motor. `ReceberTelemetria` permanece no contrato por
compatibilidade; o painel não deve contar os dois como amostras distintas.

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

A severidade é serializada como **string** pelo protocolo JSON do hub, assim
como nas respostas HTTP. Clientes que já normalizam os números 0–3 podem manter
essa tolerância para versões anteriores do servidor.

`DADOS_INSUFICIENTES` em `codigoRegra`, acompanhado de
`metadados.avaliacaoConcluida: false`, significa que faltam amostras para a
regra. Nesse caso, `severidade: "Normal"` apenas preserva a enumeração existente,
sem atestar normalidade; apresente "Aguardando amostras". Consulte
[api-compatibility.md](api-compatibility.md).

As avaliações são eventos transitórios. O backend não persiste histórico de
resultados nesta versão.

## Distribuição dos eventos

Nesta versão, a telemetria é publicada em broadcast global. Todos os clientes
conectados recebem os eventos e o frontend é responsável por filtrar os itens
do motor atualmente exibido usando `motorId`.

O hub mantém `AssinarMotor` e `DesassinarMotor` para uma evolução futura, mas
esses grupos não restringem a publicação enquanto a estratégia global estiver
em vigor.

## Carregamento e amostragem no painel

1. Conecte ao hub e registre o callback de lote.
2. Consulte os motores/sensores e carregue a última leitura de cada sensor.
3. Mescle GET e eventos por `sensorId`/`dataHora`, evitando duplicar pontos e
   impedindo que uma resposta HTTP mais antiga substitua um evento recente.
4. Use os resultados do backend para os alertas; temperatura, gradiente e
   assimetria têm unidades diferentes. Não aplique um corte térmico fixo para
   reclassificar a severidade enviada pelo servidor.

Reconectar não reproduz eventos perdidos: recarregue as últimas leituras por
HTTP e continue a janela em memória. O horário é de recebimento do servidor,
não de medição do ESP. Mais orientações em
[monitoramento-preditivo.md](monitoramento-preditivo.md).

## Compatibilidade

Clientes antigos que ignoram `resultadosQualityGates` continuam compatíveis.
Clientes novos devem tratar a propriedade como uma lista sempre presente. Não
há um segundo hub ou evento específico para Quality Gates.
