# Compatibilidade dos contratos da API

## Regras de qualidade

O catálogo aceita os tipos `temperatura.limiar_estatico`,
`temperatura.gradiente_termico` e `temperatura.assimetria_escovas`.

Todos usam os parâmetros `aviso` e `critico`, com `aviso` menor que `critico`.
No limiar estático, os valores representam temperatura em °C e devem estar
entre `-55` e `200`. No gradiente, representam a taxa de subida em °C/minuto.
Na assimetria, representam a diferença entre escovas em °C. Gradiente e
assimetria aceitam somente valores não negativos.

O tipo é persistido em `RegrasQualidade.TipoRegra`; os parâmetros ficam em
`RegrasQualidade.ParametrosJson`. Essa combinação diferencia a implementação
aplicada ao registro sem exigir colunas específicas para cada tipo.

As respostas usam camelCase, severidade como string e datas ISO 8601 com offset
ou UTC explícito, tanto em HTTP quanto no protocolo JSON do SignalR.

No cadastro, omitir `tipoRegra` seleciona o limiar estático por compatibilidade.
Na edição (`PUT`), envie o objeto completo com `tipoRegra`, `parametros` e
`ativo`: omitir o tipo também seleciona o estático, não preserva implicitamente
o tipo anterior. Os nomes dos tipos e dos parâmetros estão no
[guia de monitoramento](monitoramento-preditivo.md).

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

Cada lote deve conter **todos os sensores cadastrados de um único motor**,
identificados por `espMacAddress` e `numeroEscova`. O ESP envia o conjunto
completo em uma requisição; não envie uma requisição por escova. O número da
escova deve ser positivo e único no lote. Temperaturas devem ser finitas,
entre -55 e 200 °C, inclusive. Horário e IP são atribuídos no recebimento HTTP.

`202 {"ok":true}` significa validação do payload e entrada na fila. A
verificação contra o cadastro acontece depois: sensores desconhecidos, mistura
de motores ou ausência de alguma escova descartam o lote inteiro, com registro
no log e contador de falhas. Não há resposta posterior nem confirmação de
persistência ao dispositivo. O ESP apenas coleta e transmite; o backend
avalia os alertas. Perda de amostras em falhas/reinícios é uma limitação aceita
da demonstração; não há fila durável ou idempotência persistente.

## Última leitura

`GET /api/leituras` retorna a última leitura persistida junto com `motorId` e
`resultadosQualityGates`. Os gates são reavaliados no momento da consulta com
as regras atualmente ativas e vinculadas ao motor; os resultados históricos
dos gates não são persistidos nesta versão.

O gradiente usa a última leitura **anterior** ao instante da amostra consultada;
a assimetria usa as leituras do mesmo motor com o **mesmo timestamp** dessa
amostra, compartilhado pelas leituras de cada lote. Não são misturadas as
últimas leituras independentes de escovas em instantes diferentes. Não há
nova coluna de lote nem migration para essa reconstrução.

`alertaDesgaste` na resposta é calculado a partir dos resultados reavaliados.
Editar regras pode mudar o diagnóstico da mesma temperatura; não altera a
temperatura, o timestamp ou a flag histórica gravada na ingestão.

Quando faltam amostras para calcular uma regra, seu resultado usa
`codigoRegra: "DADOS_INSUFICIENTES"`, mensagem explicativa e
`metadados.avaliacaoConcluida: false`. Por compatibilidade, `severidade` é
`Normal` e `disparouAlerta` é `false`: isso significa ausência de violação
calculada, não confirmação de condição saudável. O frontend deve mostrar
"Aguardando amostras" nesse caso. Lista vazia indica ausência de regras ativas
aplicáveis e deve ser exibida como "Sem regras".

O GET retorna 404 quando não há sensor ou leitura correspondente. Não existe
consulta de histórico por período nesta entrega.

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
