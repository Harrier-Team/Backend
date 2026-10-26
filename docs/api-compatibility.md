# Compatibilidade dos contratos da API

## Regras de qualidade

O catálogo aceita somente `temperatura.limiar_estatico` nesta versão. Os
parâmetros `aviso` e `critico` são números finitos entre `-55` e `125`, e
`aviso` deve ser menor que `critico`.

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
