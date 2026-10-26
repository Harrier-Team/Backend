# Compatibilidade dos contratos da API

## Regras de qualidade

O catálogo aceita os tipos `temperatura.limiar_estatico`,
`temperatura.gradiente_termico` e `temperatura.assimetria_escovas`.

Todos usam os parâmetros `aviso` e `critico`, com `aviso` menor que `critico`.
No limiar estático, os valores representam temperatura em °C e devem estar
entre `-55` e `125`. No gradiente, representam a taxa de subida em °C/minuto.
Na assimetria, representam a diferença entre escovas em °C. Gradiente e
assimetria aceitam somente valores não negativos.

O tipo é persistido em `RegrasQualidade.TipoRegra`; os parâmetros ficam em
`RegrasQualidade.ParametrosJson`. Essa combinação diferencia a implementação
aplicada ao registro sem exigir colunas específicas para cada tipo.

As respostas usam camelCase, severidade como string e datas ISO 8601 com offset
ou UTC explícito.

## Vínculos

O vínculo contém o objeto `regra` completo. O estado `ativo` do vínculo é
independente do estado global de `regra.ativo`. Uma associação duplicada retorna
`409 Conflict` com o código `quality_rule_already_linked`.

Excluir uma regra remove seus vínculos. Remover um vínculo não remove a regra do
catálogo.

## Erros

Respostas de erro para clientes HTTP usam Problem Details com:

- `status`;
- `title`;
- `detail` seguro;
- `instance`;
- `code`;
- `traceId`;
- `errors`, quando houver falhas de campo.

Clientes de dispositivos continuam recebendo o formato compacto de sucesso ou
erro definido para ingestão.

## Ingestão e capacidade

O tamanho máximo do lote e a capacidade da fila são configurações explícitas
do serviço. Lotes acima de `Telemetria:MaxBatchSize` são rejeitados como payload
inválido. Quando a fila está cheia, a API responde `503` com `Retry-After`.

Ainda não há idempotência persistente: o protocolo atual não fornece uma chave
de mensagem do dispositivo. Antes de ativá-la, o dispositivo e a API devem
concordar sobre uma chave estável, sua retenção e o comportamento para reenvios.

## Última leitura

`GET /api/leituras` retorna a última leitura persistida junto com `motorId` e
`resultadosQualityGates`. Os gates são reavaliados no momento da consulta com
as regras atualmente ativas e vinculadas ao motor; os resultados históricos
dos gates não são persistidos nesta versão.

## Status agregado do motor

`GET /api/motores/{motorId}/status` retorna a situação consolidada do motor,
incluindo a maior severidade entre as escovas, a contagem por severidade, o
principal motivo e a última leitura de cada escova.

`situacao` é calculada pelo frescor das leituras:

- `Online`: todas as escovas possuem leitura dentro de
  `Telemetria:OfflineAfterSeconds`;
- `Degradado`: apenas parte das escovas possui leitura recente;
- `Offline`: o motor não possui leitura recente ou ainda não possui leituras.

O campo `desde` não faz parte deste primeiro contrato. Os enums
`severidadeGeral`, `escovas[].severidade` e `situacao` são serializados como
strings.

Cada lote de telemetria contém todas as escovas do motor. O status agregado
publicado após a ingestão representa o conjunto completo recebido naquele lote.
