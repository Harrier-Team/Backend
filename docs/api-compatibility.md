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
