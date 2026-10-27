# Contrato SignalR de telemetria

## Conexão

- Hub único: `/api/telemetriaHub`
- Transporte: SignalR
- Eventos enviados pelo servidor:
  - `ReceberTelemetria`
  - `ReceberLoteTelemetria`
  - `ReceberStatusMotor`
  - `ReceberAlertaManutencao`

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

## Exemplo de telemetria

Cada lote publicado em `ReceberLoteTelemetria` é um array com uma entrada por
escova do motor. O exemplo abaixo mostra duas escovas, uma delas com alerta:

```json
[
  {
    "sensorId": "AA:BB:CC:DD:EE:FF-1",
    "espMacAddress": "AA:BB:CC:DD:EE:FF",
    "numeroEscova": 1,
    "valorCelsius": 72.4,
    "dataHora": "2026-10-26T14:30:00+00:00",
    "alertaDesgaste": false,
    "motorId": 3,
    "resultadosQualityGates": [
      {
        "regraQualidadeId": "5f0c2b1e-8d4a-4a53-9a58-1f6f0a7c9e11",
        "nomeRegra": "Limiar estático",
        "disparouAlerta": false,
        "severidade": "Normal",
        "tipoRegra": "temperatura.limiar_estatico",
        "codigoRegra": "OK",,
        "valorObservado": null,
        "valorLimite": null,
        "metadados": null
      }
    ]
  },
  {
    "sensorId": "AA:BB:CC:DD:EE:FF-2",
    "espMacAddress": "AA:BB:CC:DD:EE:FF",
    "numeroEscova": 2,
    "valorCelsius": 96.1,
    "dataHora": "2026-10-26T14:30:00+00:00",
    "alertaDesgaste": true,
    "motorId": 3,
    "resultadosQualityGates": [
      {
        "regraQualidadeId": "5f0c2b1e-8d4a-4a53-9a58-1f6f0a7c9e11",
        "nomeRegra": "Limiar estático",
        "disparouAlerta": true,
        "severidade": "Critico",
        "tipoRegra": "temperatura.limiar_estatico",
        "codigoRegra": "TEMP_CRITICA",
        "mensagemDiagnostico": "Escova 2 atingiu temperatura crítica de 96.1 °C.",
        "valorObservado": 96.1,
        "valorLimite": 95,
        "metadados": null
      }
    ]
  }
]
```

Os valores de `codigoRegra`, `mensagemDiagnostico` e `metadados` dependem da
regra; o exemplo é ilustrativo.

Logo em seguida, o backend publica `ReceberStatusMotor` para o motor:

```json
{
  "motorId": 3,
  "severidadeGeral": "Critico",
  "situacao": "Online",
  "contagem": { "normal": 1, "atencao": 0, "alertaPreditivo": 0, "critico": 1 },
  "motivo": {
    "numeroEscova": 2,
    "nomeRegra": "Limiar estático",
    "tipoRegra": "temperatura.limiar_estatico",
    "codigoRegra": "TEMP_CRITICA",
    "valorObservado": 96.1,
    "valorLimite": 95,
    "mensagemDiagnostico": "Escova 2 atingiu temperatura crítica de 96.1 °C."
  },
  "escovas": [
    { "sensorId": "AA:BB:CC:DD:EE:FF-1", "numeroEscova": 1, "valorCelsius": 72.4, "severidade": "Normal", "dataHora": "2026-10-26T14:30:00+00:00", "alertaDesgaste": false, "resultadosQualityGates": [] },
    { "sensorId": "AA:BB:CC:DD:EE:FF-2", "numeroEscova": 2, "valorCelsius": 96.1, "severidade": "Critico", "dataHora": "2026-10-26T14:30:00+00:00", "alertaDesgaste": true, "resultadosQualityGates": [] }
  ]
}
```

Em `escovas[].resultadosQualityGates` vão os mesmos objetos mostrados acima; foram
omitidos aqui para abreviar.

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

As avaliações são eventos transitórios: o backend não persiste o resultado de
cada gate por leitura. Os alertas de manutenção (gate que passou a disparar) são
gravados e consultados por `GET /api/alertas`; veja
[api-compatibility.md](api-compatibility.md).

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

## Status agregado do motor

Ao chamar `AssinarMotor(motorId)`, o cliente entra no grupo do motor e recebe
imediatamente `ReceberStatusMotor` com o snapshot atual. Após uma nova
telemetria, o backend publica o snapshot atualizado em broadcast global.

O worker só aceita lotes com o conjunto completo de sensores do motor; portanto
o status publicado após cada lote representa o motor inteiro. O campo `motorId`
permite que cada cliente filtre os snapshots do motor exibido.

O payload contém:

| Campo | Tipo | Semântica |
|---|---|---|
| `motorId` | inteiro | Motor monitorado |
| `severidadeGeral` | string | Maior severidade entre as escovas |
| `situacao` | string | `Online`, `Degradado` ou `Offline` |
| `contagem` | objeto | Quantidade de escovas por severidade final |
| `motivo` | objeto nulo | Gate disparado de maior severidade |
| `escovas` | array | Última leitura e gates de cada escova |

Os enums `severidadeGeral`, `escovas[].severidade` e `situacao` são sempre
serializados como strings. O campo `desde` não faz parte deste primeiro
contrato; será adicionado quando o histórico de transições for implementado.

O backend não emite evento quando um ESP fica em silêncio, portanto `Offline` não
é enviado por push. Se o cliente deixar de receber eventos do motor por um tempo
(por exemplo, 30 s), deve consultar `GET /api/motores/{motorId}/status`.

## Alerta de manutenção

Quando um lote abre um alerta de manutenção ou faz a severidade de um alerta
aberto **subir**, o backend publica `ReceberAlertaManutencao`, um evento por
alerta, do mais grave para o menos grave, logo depois de `ReceberStatusMotor`.
O payload é o mesmo item devolvido por `GET /api/alertas` (veja os campos em
[api-compatibility.md](api-compatibility.md)), já com o `id` gravado:

```json
{
  "id": 41,
  "motorId": 3,
  "sensorId": "AA:BB:CC:DD:EE:FF_Escova2",
  "numeroEscova": 2,
  "regraQualidadeId": "5f0c2b1e-8d4a-4a53-9a58-1f6f0a7c9e12",
  "nomeRegra": "Previsão por tendência",
  "tipoRegra": "temperatura.previsao_tendencia",
  "categoria": "Preditivo",
  "codigoRegra": "PREVISAO_TENDENCIA_ALERTA",
  "severidade": "AlertaPreditivo",
  "valorObservado": 8.4,
  "valorLimite": 10,
  "mensagem": "A escova 2 tende a atingir 95.0 °C em ~8 min (+2.10 °C/min).",
  "iniciadoEm": "2026-10-27T14:30:00+00:00",
  "atualizadoEm": "2026-10-27T14:30:00+00:00",
  "resolvidoEm": null,
  "ativo": true
}
```

Não há evento para os demais casos: um alerta que continua aberto sem piorar,
que perde gravidade ou que é encerrado não é reenviado, para o painel não
receber um aviso a cada lote. O painel mantém a lista atual com
`GET /api/alertas?ativo=true` e usa este evento para mostrar o aviso e inserir
ou atualizar a linha pelo `id`. Para saber que um alerta foi encerrado, consulte
a lista de novo. Como os demais eventos, é enviado em broadcast global: filtre
por `motorId`. O evento só chega a quem está conectado; ao reconectar, recarregue
a lista pela API.
