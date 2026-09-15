# ADR 004 - Uso de HorizontalPodAutoscaler e Segregação de Ambientes

## Status

Aceito

## Contexto

A oficina expandiu para múltiplas unidades e a base de clientes cresce de forma contínua. A carga sobre a API deixou de ser uniforme: concentra-se no horário comercial, com picos na abertura da oficina e no fim da tarde, e cai a praticamente zero à noite e nos fins de semana.

Um número fixo de réplicas resolve isso mal nas duas pontas. Dimensionar para o pico significa pagar por capacidade ociosa a maior parte do tempo; dimensionar para a média significa degradar justamente quando o atendimento está mais movimentado — que é quando o atendente está com o cliente na frente dele.

O cluster EKS já tem o Metrics Server instalado como addon, e a aplicação já expõe `/health` e `/health/ready`. Falta decidir **como** escalar e **quantos ambientes** manter.

---

## Decisão

### 1. Escalar horizontalmente por CPU, com HPA

A aplicação escala de **1 a 6 réplicas**, com alvo de **50% de utilização de CPU**, via `HorizontalPodAutoscaler` (`autoscaling/v2`).

O `minReplicas: 1` é deliberado: fora do horário comercial não há razão para manter capacidade parada. O `maxReplicas: 6` é o teto que a capacidade do node group comporta sem entrar em pressão de recursos.

O alvo de 50% deixa folga para absorver um pico enquanto o pod novo sobe. A aplicação executa migrations e verificação de contextos EF antes de responder no `/health/ready`, o que faz o `startupProbe` tolerar até 300 segundos (`failureThreshold: 30`, `periodSeconds: 10`) — um alvo mais agressivo, como 80%, provocaria saturação durante essa janela de inicialização.

### 2. Comportamento assimétrico: sobe rápido, desce devagar

```yaml
behavior:
  scaleUp:
    stabilizationWindowSeconds: 0      # reage imediatamente
    policies: [{ type: Percent, value: 100, periodSeconds: 30 }]
  scaleDown:
    stabilizationWindowSeconds: 300    # espera 5 minutos
    policies: [{ type: Percent, value: 50, periodSeconds: 60 }]
```

Subir capacidade cedo demais custa alguns centavos; descer cedo demais custa disponibilidade. Como a carga é intermitente por natureza, uma janela de estabilização curta na descida provocaria oscilação — o *flapping* clássico, em que o HPA remove réplicas e precisa recriá-las em seguida, pagando o custo de inicialização a cada ciclo.

### 3. Segregação de ambientes por namespace, não por cluster

Homologação e produção convivem no **mesmo cluster**, em namespaces distintos (`homologacao` e `production`), com releases Helm separados (`wrench-hml` e `wrench`) e hostnames distintos.

O gatilho é a branch: `develop` implanta em homologação, `master` em produção.

---

## Alternativas Consideradas

**Réplicas fixas.** Simples e previsível, sem Metrics Server nem tuning. Descartada por não atender ao requisito de escalabilidade do desafio e por errar nas duas direções — ou paga ociosidade, ou degrada no pico.

**Escalar por métrica customizada (requisições por segundo ou latência).** Correlaciona melhor com a experiência do usuário do que CPU: um pod pode estar lento por espera de I/O sem consumir CPU. Descartada por agora porque exige Prometheus Adapter ou KEDA, e a operação da API é dominada por trabalho de CPU (serialização, validação, mapeamento) — CPU é um proxy razoável. Vale revisitar quando a frente de observabilidade tiver métricas de latência por endpoint em produção: aí existe base empírica para decidir.

**Vertical Pod Autoscaler.** Ajustaria requests e limits automaticamente, mas escala vertical não resolve disponibilidade — um pod maior continua sendo um ponto único de falha, e o VPA reinicia o pod para aplicar o novo tamanho. Complementar ao HPA, não substituto; fica de fora enquanto os requests atuais estiverem adequados.

**Cluster separado para homologação.** É o isolamento ideal: falha em homologação não alcança produção de forma alguma. Descartada por custo — duplicar VPC, EKS, node group e ALB mais que dobra a conta de infraestrutura de um projeto acadêmico, sem contrapartida proporcional. A separação por namespace entrega isolamento de rede, de recursos e de configuração, que é o que o ciclo de desenvolvimento precisa.

**Ambiente de homologação com RDS próprio.** Mesma lógica: uma segunda instância RDS pelo tempo do projeto não se paga. Homologação aponta para o mesmo banco, com base lógica distinta quando necessário. É uma limitação real e está registrada como tal.

---

## Consequências

**Positivas**

* Custo acompanha a demanda: fora do horário comercial a aplicação roda com uma réplica.
* Picos de atendimento são absorvidos sem intervenção manual.
* O comportamento assimétrico evita *flapping* e o custo repetido de inicialização.
* Ter homologação no mesmo cluster valida o caminho real de deploy — mesmo chart, mesmo pipeline, mesmo controller de ALB.

**Negativas**

* Com `minReplicas: 1`, a primeira requisição após um período ocioso pode pegar um único pod sob carga fria. Aceitável para o padrão de uso; se incomodar, o piso sobe para 2.
* CPU não captura degradação por I/O. Uma lentidão no RDS não dispara escala — e escalar não resolveria mesmo.
* O teto de 6 réplicas é limitado pela capacidade do node group. Estourar esse teto exige Cluster Autoscaler ou Karpenter, que não estão instalados.
* Homologação e produção compartilham cluster e banco. Um teste de carga em homologação **afeta** produção. É a contrapartida explícita da decisão de custo e precisa ser respeitada na prática: testes de carga rodam em janela combinada.
* O HPA depende do Metrics Server. Se o addon cair, a escala congela no número atual de réplicas — sem falhar visivelmente. Esse é um caso a cobrir com alerta na frente de observabilidade.

---

## Referências

* [Diagrama de componentes](../infraestrutura/arquitetura.md)
* [Pipeline HTTP e healthchecks](../infraestrutura/pipeline-http.md)
* Chart Helm: `wrench-api-k8s/templates/api/hpa.yaml`
