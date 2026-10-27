# Monitoramento preditivo e amostragem na Hackatohm

## Objetivo da entrega

Demonstrar uma cadeia funcional: ESP coleta temperaturas de todas as escovas
de um motor, envia um lote, backend avalia regras e salva as amostras, frontend
acompanha curvas e diagnósticos em tempo real. A decisão fica no backend; o ESP
não comanda manutenção nem espera confirmação de persistência.

O protótipo fornece indícios térmicos para apoiar manutenção de condição.
Não estima vida útil restante nem demonstra causalidade entre temperatura e
desgaste. Não há modelo de machine learning implementado. Limites operacionais
precisam de calibração com o equipamento; valores de exemplo são sintéticos.

## Contrato do ESP

Um ESP deve enviar todas as escovas cadastradas do mesmo motor em cada POST.
Cada escova tem `numeroEscova` positivo e único; o identificador persistido é
`MAC_NORMALIZADO_EscovaN`. Cadastre todos os sensores antes de enviar.

```json
{
  "espMacAddress": "AA:BB:CC:DD:EE:FF",
  "sensores": [
    { "numeroEscova": 1, "temperatura": 42.5 },
    { "numeroEscova": 2, "temperatura": 44.1 }
  ]
}
```

O exemplo exige um motor com exatamente essas duas escovas no cadastro.
Adicionar uma terceira escova exige incluí-la nos próximos lotes. O fluxo
atual pressupõe um ESP cobrindo o conjunto completo do motor; dividir as escovas
desse motor entre ESPs não atende a esse contrato.

O backend valida números finitos de -55 a 200 °C. Essa faixa é um contrato de
software, não uma declaração de exatidão do sensor ou de operação segura do
motor. Não foi removida porque também evita amostras inválidas no diagnóstico.

`202 {"ok":true}` confirma entrada na fila. O worker verifica cadastro e
completude antes de avaliar/salvar. Falhas ficam nos logs e métricas; não há
segunda confirmação. Payload inválido retorna 400 e fila cheia retorna 503
com `Retry-After: 5`. O firmware atual pode seguir enviando a amostra mais
recente na próxima cadência, aceitando lacunas em caso de falha.

## Regras disponíveis

Todas usam `parametros: {"aviso": ..., "critico": ...}`, com números finitos
e, exceto na previsão por tendência, `aviso < critico`. Configuração e vínculo
devem estar ativos para avaliação.

| `tipoRegra` | Valor avaliado | Unidade dos limites | Condição para avaliar |
|---|---|---|---|
| `temperatura.limiar_estatico` | Temperatura atual | °C, de -55 a 200 | Uma leitura válida |
| `temperatura.gradiente_termico` | `(T_atual - T_anterior) / intervalo_em_minutos` | °C/min, não negativos | Duas leituras do sensor com intervalo positivo |
| `temperatura.assimetria_escovas` | `T_máxima - T_mínima` no lote | Diferença em °C, não negativos | Pelo menos duas escovas no lote |
| `temperatura.previsao_tendencia` | Minutos estimados até `temperaturaLimite`, pela reta das últimas leituras | Minutos (`aviso > critico`), mais `temperaturaLimite` em °C | Pelo menos 5 leituras do sensor |

O limite crítico é verificado primeiro. Valor `>= critico` gera `Critico`;
senão, `>= aviso` gera `AlertaPreditivo`; abaixo do aviso gera `Normal`.
No gradiente, queda/estabilidade (`<= 0`) não gera alerta de subida.

Limiar, assimetria e gradiente descrevem o estado presente ou a velocidade de
subida. Só a previsão por tendência responde *quando* o limite será atingido: o
tempo estimado até `temperaturaLimite` é o valor observado do resultado. Ela é
uma extrapolação linear das últimas leituras, não um modelo de machine learning,
e perde precisão se o comportamento térmico não for linear. Na previsão, menos
minutos até o limite significa mais urgência; por isso `aviso` é maior que
`critico`.

Exemplo de bancada: 40 para 45 °C em 30 s corresponde a 10 °C/min.
Um lote com 40, 44 e 58 °C tem assimetria de 18 °C. Os números não são
recomendações de limites para o equipamento real.

A assimetria representa o conjunto do motor: o mesmo resultado é associado
a cada escova avaliada. Ela não identifica sozinha qual escova está defeituosa;
os metadados informam as temperaturas mínima e máxima do lote.

Falta de histórico no primeiro envio ou motor com uma única escova gera
`DADOS_INSUFICIENTES`, sem disparar alerta, com mensagem explicativa e
`metadados.avaliacaoConcluida: false`. O campo de severidade permanece `Normal`
por compatibilidade e deve ser apresentado como "Aguardando amostras" nesse
contexto. A ausência de regras produz lista vazia, exibida como "Sem regras".

## Consistência entre consulta e tempo real

O lote recebe um timestamp UTC único do servidor. Todas as suas leituras são
salvas juntas. Na consulta da última leitura, o backend recupera a amostra
anterior daquele sensor e as leituras do motor nesse mesmo timestamp, usando
as regras atualmente ativas. Não compara escovas de instantes diferentes.

Mantidas as regras e os dados, a consulta reproduz os diagnósticos de gradiente
e assimetria do lote. O recebimento já normaliza o timestamp para a precisão de
microssegundos do PostgreSQL, usando a mesma base temporal na avaliação e na
consulta dos novos lotes. Não há identificador de lote separado.

Editar/desativar regras altera a reavaliação no próximo GET ou novo envio,
sem modificar as temperaturas salvas. O `alertaDesgaste` devolvido pelo GET
é recalculado para acompanhar os resultados. Não há push específico para
alteração de regras: após editar, o frontend deve recarregar as leituras.

Histórico completo das avaliações não é persistido. Registros antigos que já
tenham sido recebidos em lotes parciais só permitem reconstruir as leituras
efetivamente existentes naquele instante; o novo contrato não cria amostras
retroativas nem altera o banco por migration.

## Integração e amostragem no frontend

- Registre `ReceberLoteTelemetria`, filtre `motorId` e atualize as escovas do
  lote de uma vez. Use `sensorId` e `dataHora` para mesclar amostras de HTTP e
  SignalR sem duplicação.
- Ao abrir/reconectar, consulte a última leitura por sensor. Uma resposta GET
  antiga não deve substituir uma leitura mais recente recebida pelo hub.
- O backend avalia todos os lotes aceitos. Redução de pontos para desenhar o
  gráfico deve ocorrer apenas na apresentação, preservando picos relevantes
  e os eventos de alerta recebidos.
- Mantenha uma janela limitada de pontos por sensor em memória. Esta API não
  expõe histórico por período; recarregar a página não repõe toda a curva.
- Mostre temperatura em °C, gradiente em °C/min e assimetria como diferença em
  °C. Não acrescente °C indiscriminadamente a todo `valorObservado`.
- Use a maior severidade dos gates válidos para resumir o motor; `alertaDesgaste`
  sozinho não distingue aviso de crítico. Exiba diagnósticos, regra e escova.
- Mostre timestamp/idade da última amostra. Conexão SignalR ativa ou ping do
  ESP não garantem temperatura recente. Silêncio não representa retorno à
  normalidade; mantenha o último diagnóstico com indicação de desatualização.

Para a bancada atual, manter uma cadência fixa de aproximadamente 10 segundos
é uma referência inicial de demonstração, coerente com os envios observados.
Ao ajustar a frequência, considere tempo de resposta do sensor, velocidade do
fenômeno e rede. Não há garantia de simultaneidade física entre sensores:
o timestamp representa recebimento do lote, e o gradiente usa esse intervalo.

## Verificação prática da demonstração

1. Cadastre um motor e todas as escovas com o MAC real; confirme que o firmware
   envia a mesma quantidade e numeração em cada lote.
2. Verifique a chegada das leituras no painel e a cadência dos timestamps.
3. Use regras de bancada apropriadas para demonstrar aviso/crítico e observe
   mensagem, valor e unidade. Não reutilize limites sintéticos na operação real.
4. Recarregue o painel: as últimas amostras e seus diagnósticos devem reaparecer.
5. Edite uma regra, recarregue as leituras e confirme que o diagnóstico acompanha
   a nova configuração. Desligue o envio para demonstrar idade dos dados.

## Escopo assumido

A entrega mantém fila em memória, broadcast global e testes unitários focados.
Não exige testes de integração, confirmação durável ao ESP, fila distribuída,
reenvio offline, arquitetura adicional de estados ou machine learning.
Há um projeto antigo de testes de integração vazio, sem compromisso de cobertura.
Essas escolhas concentram o esforço na comunicação, nos dados do painel e nas
regras preditivas demonstráveis.
