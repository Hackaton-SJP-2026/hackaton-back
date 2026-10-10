# Análise do frontend e passo a passo de implementação — Missão SJP

Data da análise inicial: 09/10/2026. Revisão: regras de negócio informadas pelo usuário incorporadas nesta versão.

## 1. Escopo e diagnóstico

Análise do código em `Hackaton_frontend`, comparado ao `Manual_Hackathon_SJP_A5 (1).pdf` (36 páginas, versão para revisão) e à foto da ordem de serviço nesta pasta. O backend não foi auditado: as dependências de backend citadas são necessidades para integrar o frontend, não conclusões sobre o que falta naquele projeto.

O frontend é um protótipo de consulta com identidade visual, navegação e interações locais. Ainda falta demonstrar a jornada operacional: registrar → integrar quando aplicável → notificar técnicos por secretaria e/ou proximidade → assumir/atribuir → executar → pausar/retomar → concluir → validar → sincronizar → reconhecer contribuições. Não se estima percentual de conclusão, pois quantidade de telas não mede atendimento aos requisitos.

## 2. O que foi implementado

| Área | O que existe | Evidência | Limitação |
|---|---|---|---|
| Base | React 18, TypeScript, Vite, Tailwind, Router, Axios e componentes Radix | `package.json`, `src/components/ui` | Sem suíte/script de testes |
| Login | Campos obrigatórios, mostrar senha, acesso ao dashboard | `src/pages/login/Index.tsx` | Qualquer credencial com formato válido entra; sem autenticação |
| Layout | Marca Missão SJP, menu desktop/mobile e avisos de demonstração | `src/layout/Dashboard.tsx` | Usuário/perfil fixos |
| Rotas | Login, dashboard, chamados, exemplos e página não encontrada | `src/routes.tsx` | Rotas operacionais sem proteção |
| Dashboard | Contadores, período 7/30 dias, gráfico, filtros e participação das equipes | `src/pages/dashboard/Index.tsx` | Dez registros locais, sem API |
| Chamados | Kanban, tabela, busca sem acentos, filtro por equipe/prioridade e ordenação por número | `src/pages/tickets/Index.tsx` | Doze registros locais; somente consulta; sem mudança por arrastar |
| Detalhes | Descrição, solicitante, contato, secretaria, departamento, unidade, equipe, executor e equipamento | `src/components/tickets/TicketDetails.tsx` | Somente leitura |
| Histórico | Autor, horário, execução e impedimentos | `src/data/tickets.ts`, `TicketDetails.tsx` | Eventos fictícios gerados na inicialização |
| Estados | Aberto, Em andamento, Bloqueado, Aguardando validação e Resolvido | `src/data/tickets.ts` | Sem ações de conclusão/validação |
| Mapa | Destino, trajeto, posição, distância, enquadramento e aviso de falha dos tiles | `TicketMap.tsx`, `src/data/ticket-tracking.ts` | Simulação por temporizador; sem GPS ou persistência |
| HTTP | Instância Axios e variável de URL | `src/lib/api-client.ts`, `.env.example` | Não consumida pelas páginas |
| Acessibilidade inicial | Labels, nomes acessíveis, estados textuais, link para conteúdo e modal Radix | Páginas, layout e componentes UI | Validação prática ainda necessária |

O primeiro chamado fictício usa o número 14142 e o assunto do cabo HDMI da foto, com pessoas e datas de demonstração. Não é importação do legado nem leitura automática da imagem.

## 3. Atendimento aos requisitos obrigatórios

Referência: manual, páginas 21–23.

| Requisito | Situação no frontend | Pendência |
|---|---|---|
| 1. Aplicação genérica e cadastro autorizado | Representação parcial de setores/equipes | Cadastro por atendente, configuração de setores/equipes/tipos/fluxos e exemplo funcional fora da TI |
| 2. Integração bidirecional | Não demonstrada | Entrada/saída com legado simulado, vínculo de origem, prevenção de duplicação e recuperação de falha |
| 3. Web e mobile com push | Layout responsivo inicial | Jornada do executor e notificação push real |
| 4. Rastreabilidade, validação e acesso | Apresentação parcial | Sessão, permissões, eventos persistidos, anexos, avaliação, confirmação e retorno de pendências |
| 5. Gamificação configurável | Não implementada | Pontos, regras, rankings individual/equipe e reinício periódico preservando histórico |
| 6. IA demonstrável | Não implementada | Aplicação útil, revisão humana e execução compatível com infraestrutura municipal |
| 7. Hospedagem municipal e código-fonte | Código e configuração de build disponíveis | Documentação integrada, implantação municipal, dependências, licenças e custos |

## 4. Limitações concretas

1. Login apenas executa navegação para `/dashboard`; sair apenas navega ao login. Não há sessão nem invalidação.
2. Dashboard e chamados usam bases diferentes: dez números 1039–1048 com idade relativa versus doze números 14142–14153 com datas fixas. Os indicadores não correspondem à área de trabalho.
3. Não existem cadastro, atribuição, edição, registro de observação, impedimento, evidência, conclusão ou avaliação.
4. Assistente de IA, Equipes, Conquistas e Relatórios são itens desabilitados, sem rotas funcionais.
5. O contrato `Ticket` usa nomes em texto e não contém IDs de usuários/equipes, sistema de origem, ID externo, eventos tipados, anexos, avaliação ou sincronização.
6. Rastreamento usa pontos fictícios e temporizador. Chegada simulada não comprova execução. Encerramento histórico deriva dos pontos, sem evento real de conclusão.
7. Tiles do mapa usam `tile.openstreetmap.org` por padrão. É necessário documentar essa dependência e definir seu funcionamento no ambiente municipal. Esta observação compara a arquitetura ao manual; não constitui parecer jurídico.
8. O Kanban tem largura mínima de 1400 px e exige rolagem horizontal; a execução pelo celular precisa de apresentação própria.
9. README ainda descreve esqueleto e rotas antigas e aponta `C:\Projeto\Hackaton_frontend`, diferente da pasta atual.
10. `.env.example` menciona `VITE_MAP_ATTRIBUTION` apenas em comentário e não define a URL da API.

## 4A. Regras de negócio atualizadas

Esta seção registra as novas instruções do usuário e prevalece sobre trechos históricos deste plano quando houver divergência. Não afirma que as funcionalidades foram implementadas.

### Escopo: área técnica não é secretaria

O escopo inicial adotado é atendimento de TI aos órgãos municipais. As áreas informadas são **Suporte, Telecom e TI**, configuráveis. Como “TI” também é o domínio geral, usar provisoriamente “TI / Sistemas” como terceira área, pendente de confirmação da nomenclatura. Não são afirmações sobre o organograma oficial.

- **Secretaria:** órgão atendido e responsabilidade do técnico, como Saúde ou Educação. Outras secretarias serão cadastradas; não há lista oficial definida nesta conversa.
- **Departamento e unidade:** subdivisão e local físico, por exemplo departamento da Saúde e hospital/unidade de saúde.
- **Área de atendimento:** Suporte, Telecom ou TI/Sistemas.
- **Natureza:** Hardware ou Software; não confundir com área ou secretaria. Regras para demanda mista ainda precisam ser definidas.
- **Modalidade:** remota ou presencial. Para remota, proximidade física não é critério útil por padrão; notificação por secretaria continua aplicável.

Suporte pode abranger estações, periféricos e instalação; Telecom, conectividade/telefonia; TI/Sistemas, aplicações e acessos. Essa distribuição é proposta inicial, ajustável pelo administrador.

### Papéis e canais

| Papel | Canal principal | Permissões e responsabilidades |
|---|---|---|
| Usuário da prefeitura/secretaria | Web | Login/perfil; cadastrar; consultar seus chamados abertos, em andamento e fechados; ver técnico, início e eventos; confirmar/avaliar entrega e indicar pendência |
| Técnico | Aplicativo | Login/perfil com foto; disponíveis, vinculados, em andamento, pausados e concluídos; aceitar chamado autorizado; ordem de serviço; eventos; início/pausa/retomada/conclusão; localização; push; ranking |
| Administrador | Web | Login/perfil; cadastrar; gerenciar técnicos/secretarias/áreas; atribuir/reencaminhar; dashboard e entregas; filtros; mapa agregado com modal; regras de pontos e notificações |

As permissões devem ser aplicadas no backend. Pertencer a outra secretaria não libera todo o histórico ou dados de usuários: um técnico elegível para cooperação recebe apenas os dados necessários à oportunidade e ao atendimento autorizado.

**Diferença em relação ao manual:** o usuário agora solicita abertura direta pelo usuário da prefeitura. O manual reserva abertura ao atendente e distingue demandante. O produto deve seguir a instrução nova, documentando essa diferença; se a entrega precisar cumprir literalmente o manual, manter modo configurável de abertura por atendente/autorizado e apresentar o conflito à organização. Os três papéis do produto não apagam a separação entre quem registra, quem executa e quem confirma.

### Cadastro do chamado

Campos: descrição; impacto no serviço (pessoas/unidades afetadas e existência de alternativa); urgência informada; contato disponível; necessidade de material/equipamento e local de retirada quando conhecidos; solicitante (usuário); secretaria; departamento; unidade/local e coordenadas para atendimento presencial; área; Hardware/Software; atendimento remoto/presencial; equipamento quando pertinente; prioridade; pontos previstos; identificadores de origem quando integrado. Usuário autenticado é solicitante por padrão; administrador pode abrir para outra pessoa, registrando autor e solicitante separadamente.

Prioridade e pontos devem seguir regra configurada, sem permitir que o solicitante manipule livremente pontuação ou prioridade alta. Um chamado de software pode não ter equipamento físico. Local deve apontar para unidade cadastrada, evitando distância calculada a partir de nome em texto.

### Unidade de atendimento e atribuição imediata

Regra adicionada: o chamado deve conter o campo `unidade_antecedimento` (ou `unidade_atendimento`, conforme convenção final da equipe) e esse valor deve ser preenchido a partir de um cadastro de unidades no portal web. A unidade representa o local/entidade de atendimento e deve ter identidade própria dentro do sistema.

- [ ] O administrador cadastra unidades de atendimento via web.
- [x] Cada técnico pode ser vinculado a uma ou mais unidades; cada unidade tem no máximo um responsável principal ativo.
- [x] Quando um chamado é aberto para uma unidade, o sistema localiza o responsável principal ativo e disponível.
- [x] Havendo responsável disponível, o chamado é vinculado imediatamente e gera evento automático de atribuição.
- [x] Sem responsável principal disponível, o chamado permanece aberto e sem técnico responsável.
- [ ] O vínculo imediato gera aviso/push para o técnico responsável, com identificação da unidade, do chamado e da prioridade.
- [x] Outros técnicos podem permanecer vinculados à unidade como equipe de atendimento; a designação principal é explícita pelo gestor e única por unidade.
- [ ] A unidade deve constar no cadastro do chamado e no painel de chamados/atendimento para filtrar, consultar e rastrear demandas por local.
- [ ] O responsável pela unidade recebe todos os chamados abertos para essa unidade e não apenas os chamados de outras áreas/secretarias.
- [ ] O vínculo por unidade deve coexistir com as regras de cooperação, secretaria e proximidade já previstas; a atribuição por unidade não elimina a necessidade de validar elegibilidade e disponibilidade.

Essa regra é priorizada para a jornada operacional: a unidade deixa de ser apenas um campo descritivo e passa a ser um critério de elegibilidade e de atribuição automática.

### Vínculo, execução, pausa e tempo

Fluxo proposto: **Aberto → Vinculado → Em andamento ↔ Pausado → Aguardando validação → Resolvido**. Bloqueado representa impedimento e suspende atendimento/tempo ativo conforme regra proposta; manter motivo e histórico. Rejeição da entrega retorna para análise e continuidade.

- Vincular/aceitar não inicia automaticamente o cronômetro nem a captura; ação “Iniciar” abre sessão de execução.
- Tela de disponíveis permite assumir chamado da responsabilidade do técnico ou oportunidade de cooperação autorizada.
- Assunção é atômica no servidor: dois técnicos não podem virar responsáveis principais simultaneamente pelo mesmo chamado.
- Regra adotada: um atendimento em execução por técnico, inclusive participação ativa em cooperação; outros chamados podem ficar vinculados na fila. Participantes registram seus próprios intervalos de trabalho, papéis e contribuição, sem compartilhar um cronômetro indistinto.
- Eventos incluem descrição, tipo, autor e horário, preservados após pausa, transferência, conclusão e reabertura.
- Tempo ativo é soma dos intervalos iniciar/retomar até pausar/concluir; tempo pausado e tempo total são indicadores separados. Relógio visual deriva dos timestamps do servidor e não zera após fechar/reabrir o app.
- Pausa exige motivo categorizado e descrição quando necessária: material/terceiros, urgência autorizada, intervalo ou fim de expediente. O motivo determina o tratamento de disponibilidade e indicadores; nenhuma pausa torna o técnico automaticamente disponível. Pausa interrompe contagem ativa e coleta/envio de novas posições. Retomada cria novo intervalo. Dados já capturados permanecem no histórico; envio tardio deve manter timestamp original e ser identificado como histórico.
- Conclusão encerra cronômetro/coleta e concede os pontos conforme política; resolução confirmada permanece etapa distinta.

### Monitoramento, localização e mapa

Adota-se provisoriamente a regra operacional mais específica fornecida: **monitorar durante chamados em andamento**, com posição enviada a cada **10 minutos**, interrompendo ao pausar ou concluir. A expressão “monitorar o tempo todo” foi interpretada como acompanhar todo o atendimento ativo, não coletar durante pausa ou fora dele. A abrangência fora do atendimento permanece decisão pendente.

Mapa do administrador mostra última posição conhecida, nunca garante posição instantânea. Modal deve conter técnico/foto, secretarias, chamado, unidade, estado, início, tempo ativo/pausado e horário/precisão da posição. Sinalizar dados antigos; sem posição, exibir “localização indisponível”. Raio de proximidade, validade máxima da posição e precisão aceitável devem ser parâmetros configuráveis, ainda sem valores acordados.

Coleta de 10 minutos pode não detectar rapidamente a chegada ao hospital. É possível enviar posição na abertura/retomada e oferecer atualização contextual ao técnico, sem prometer localização contínua. Push por secretaria independe desse dado e resolve o caso principal mesmo quando não há GPS válido.

Captura em segundo plano é requisito a verificar em aparelho real. Não se pressupõe que a atual aplicação React responsiva consiga manter GPS com tela bloqueada; escolher solução mobile após prova funcional e registrar limitações.

### Nova regra: notificar por secretaria OU por proximidade

**Receber aviso de abertura não significa receber atribuição obrigatória.** São eventos distintos: novo chamado da responsabilidade, oportunidade de ajuda e vínculo confirmado.

| Mesma secretaria do técnico? | Próximo com posição válida? | Resultado |
|---|---|---|
| Sim | Sim ou não, inclusive GPS indisponível | Notificar abertura independentemente da distância; informar que pertence à sua secretaria |
| Não | Sim, chamado presencial | Notificar oportunidade de cooperação; aceitar é voluntário e não há penalidade por recusa/ausência de resposta |
| Não | Não ou posição indisponível/desatualizada | Não notificar por proximidade; pode haver atribuição explícita pelo administrador, que gera seu próprio aviso |
| Sim e próximo | Sim | Uma notificação de abertura, com ambos os motivos; não duplicar aviso |

Cada técnico pode pertencer a uma ou mais secretarias cadastradas. Para abertura, considerar destinatários habilitados para receber avisos: **vínculo com a secretaria OU proximidade válida ao local**, respeitando disponibilidade de turno e política de acesso a configurar. O técnico ocupado continua recebendo aviso da sua secretaria; estar ocupado afeta início/atribuição, não elimina a informação. Políticas de turno e avisos fora do expediente permanecem pendentes; não bloquear silenciosamente o caso do hospital.

Exemplo obrigatório: técnico da Saúde está atendendo no hospital; abre outro chamado no mesmo hospital. Ele recebe push de abertura por pertencer à Saúde, mesmo sem estar vinculado ao novo chamado ou sem GPS disponível. Se estiver distante, também recebe. Um técnico da Educação próximo recebe convite de ajuda voluntária; só passa a responsável/participante depois de aceitar e o servidor confirmar. Se já estiver ocupado, poderá aceitar para fila ou participar conforme política, sem iniciar dois atendimentos ativos simultaneamente.

Implementação necessária:

- [ ] Backend identifica secretarias responsáveis e candidatos próximos em cada nova abertura presencial.
- [ ] Deduplicar destinatários da união dos dois grupos por chamado/evento/técnico, inclusive em retransmissões.
- [ ] Push informa número, unidade, prioridade e motivo do aviso sem expor dados desnecessários na tela bloqueada.
- [ ] App abre detalhes com indicação “Sua secretaria” ou “Ajuda voluntária”; botão de aceite só quando elegível e disponível para assunção.
- [ ] Aceite entre secretarias mantém secretaria original e registra responsável colaborador, motivo e horário.
- [ ] Proximidade usa distância entre posição válida do técnico e unidade, não rota fictícia nem endereço textual.
- [ ] App mostra aviso persistido/lista de novidades ao reconectar; push é transporte de aviso, não fonte única do chamado.
- [ ] Se chamado já foi assumido, atualizar tela e substituir assunção por “Oferecer ajuda”, quando autorizado. Responsável ou administrador aceita a colaboração; não trocar automaticamente o responsável e não gerar vínculo principal duplicado.
- [ ] Emitir push separado para vínculo/atribuição confirmada, inclusive quando realizada pelo administrador.

### Prioridade alta e disponibilidade

Quando surge prioridade alta, iniciar triagem de atendimento: localizar técnicos responsáveis disponíveis, avaliar competência/área, fila/atividade e posição válida quando atendimento presencial. Proximidade pode ajudar a escolher atendimento ou retirada de material; não substitui responsabilidade, competência ou disponibilidade.

Proposta: notificar responsáveis, sugerir ao administrador candidatos disponíveis e oferecer ajuda aos próximos autorizados; não interromper automaticamente atendimento em curso nem impor cooperação entre secretarias. Prazo de resposta, escalonamento e possibilidade de atribuição automática precisam de definição. Se não houver candidato, manter chamado visível como alta prioridade sem atribuição e alertar administrador.

Se houver retirada de equipamento/material, registrar necessidade e local de retirada; mostrar isso ao responsável. Critério de deslocamento considera retirada e destino somente com dados válidos; algoritmo de otimização de rota não foi solicitado nem está implementado.

### Pontuação e ranking

Cada chamado tem pontos previstos segundo regras configuradas; técnico recebe crédito ao concluir. Para conciliar com qualidade e confirmação exigidas pelo manual, adota-se crédito provisório na conclusão e definitivo na confirmação, com ajuste auditável em reabertura. Essa política passa a ser adotada nesta revisão; pesos, divisão de crédito e tratamento da ausência de resposta ainda devem ser configurados. Repetir conclusão, retransmitir evento ou transferir chamado não duplica pontos. Cooperação deve registrar contribuição e regra de divisão/bonificação explícita; não penalizar recusa voluntária nem pausa por dependência externa. Ranking individual e por equipe, períodos e histórico permanecem requisitos do manual.

### Decisões pendentes sem bloquear a documentação

- Nomenclatura exata de Suporte/Telecom/TI e catálogo de competências; hardware/software misto.
- Raio de proximidade, idade/precisão máxima do GPS e política de turno/fora do expediente.
- Monitoramento fora do atendimento, pois conflita com pausa interrompendo captura.
- Limite de chamados vinculados em fila e procedimento de aprovação da colaboração; uma execução ativa por técnico e oferta de ajuda sem troca de responsável já são regras adotadas.
- Prazos/escalonamento de prioridade alta e atribuição automática versus decisão do administrador.
- Matriz de impacto/urgência, pesos de pontuação, divisão/bonificação de colaboração e tratamento de ausência de confirmação; confirmação definitiva e ajustes auditáveis na reabertura já são regras adotadas.
- Formato do app após validar GPS em segundo plano; ponto/local de retirada de equipamento.

## 4B. O que já existe e o que falta frente às novas regras

| Nova necessidade | Já existe | Ainda falta |
|---|---|---|
| Escopo TI, áreas e secretarias | Strings de secretaria, departamento, equipe e categoria | Cadastros com IDs; Suporte/Telecom/TI separados de Hardware/Software; vínculos técnicos-secretarias |
| Usuário web | Login visual e detalhes de chamado | Sessão, perfil, cadastro, dados próprios e acompanhamento autorizado |
| Técnico app | Layout responsivo e leitura de ordem | Perfil/foto, disponíveis, aceite, fila, eventos gravados, pausa, cronômetro e execução completa |
| Administrador web | Dashboard e filtros locais; mapa de um chamado | Permissões, filtros novos, dados reais, entregas e mapa agregado/modal de técnicos |
| Aviso por secretaria | Não implementado | Seleção de destinatários independentemente da distância e push de abertura |
| Ajuda por proximidade | Trajetos fictícios, sem elegibilidade | GPS válido, raio configurável, convite voluntário e aceite entre secretarias |
| Aviso de vínculo | Não implementado | Push ao assumir/ser atribuído e abertura da tarefa correta |
| Prioridade alta | Badge e filtro local | Disponibilidade, competência, triagem, retirada de equipamento e escalonamento |
| Tempo e GPS a cada 10 minutos | Simulação visual de mapa a cada 5 segundos | Sessões persistidas, cronômetro confiável, GPS real/background, pausa e sinalização de dado antigo |
| Pontos por conclusão | Menu Conquistas desabilitado | Pontuação por chamado, crédito único, ranking e política de confirmação/cooperação |

A atualização é documental. Nenhuma dessas pendências passa a concluída só por estar descrita neste arquivo.

## 4C. Refinamentos adotados após revisão das regras

As melhorias desta seção foram autorizadas pelo usuário. São regras do plano a implementar, não funcionalidades existentes. Parâmetros numéricos não fornecidos permanecem explicitamente pendentes.

### Aviso, responsabilidade e execução são eventos separados

1. **Aviso:** informa abertura ou oportunidade; não cria vínculo nem obrigação de cooperação.
2. **Vínculo:** aceite confirmado pelo servidor ou atribuição autorizada define responsável principal e fila; gera seu próprio aviso.
3. **Início:** abre intervalo de execução, cronômetro e coleta de localização.

Registrar cada evento com chamado, autor/origem, destinatário ou responsável e horário. Entrega do push não comprova leitura, aceite ou atendimento. Não usar quantidade de notificações como indicador de trabalho executado.

### Disponibilidade independente do estado do chamado

Estados do técnico: **Disponível**, **Ocupado**, **Fora do expediente** e **Indisponível**, com motivo e última atualização. Início de atendimento torna o técnico ocupado. Pausar não libera automaticamente; avaliar motivo e demais atividades. Intervalo/fim do expediente e impedimento do chamado são fatos diferentes.

Receber avisos da secretaria durante expediente independe de estar ocupado ou distante. A política fora do expediente e eventual plantão deve ser configurada, sem inferir disponibilidade a partir de GPS ou aplicativo conectado. Aplicativo desconectado também não prova indisponibilidade profissional; mostrar separadamente conectividade e disponibilidade.

Uma execução ativa por técnico; múltiplos vínculos em fila são permitidos. Convite voluntário não tem penalidade por recusa ou ausência de resposta.

### Notificações úteis e cooperação

Convites por proximidade exigem atendimento presencial, posição recente/com precisão aceitável e competência cadastrada compatível. Sem esses dados, não presumir elegibilidade por proximidade. Aviso da secretaria continua independente do GPS; secretaria pode conter equipes de áreas distintas, devendo o aviso indicar a área necessária.

Deduplicar por evento/chamado/técnico, agrupar avisos comuns quando configurado e evitar repetir convites sem mudança relevante. Alta prioridade deve seguir sua política própria de destaque/escalonamento. Persistir novidades no sistema para consulta quando o dispositivo reconectar.

Para chamado já assumido, “Oferecer ajuda” cria solicitação de colaboração. Responsável ou administrador aceita; o responsável principal permanece até transferência explícita e auditada. Registrar colaboradores e atividades realizadas. Recusar colaboração não reduz pontuação. Notificações pendentes devem abrir o estado atual, inclusive se chamado já foi resolvido ou transferido.

### Prioridade por impacto e urgência

Solicitante informa impacto e urgência; regra configurada sugere prioridade e administrador confirma/altera com justificativa. Considerar interrupção de serviço essencial, quantidade de usuários afetados e alternativa operacional. Não atribuir alta prioridade apenas porque o usuário selecionou “urgente”.

Configurar prazo de resposta/assunção, alerta por ausência de candidato e escalonamento ao administrador/plantão. Distinguir prazo até primeiro atendimento de duração de execução. Atraso do push não muda o timestamp de abertura.

Não interromper automaticamente outra tarefa. Administrador autoriza eventual troca por urgência, registra justificativa e mantém responsável/plano de continuidade do chamado interrompido. Cooperação entre secretarias permanece voluntária. Atribuição automática é possibilidade futura configurável, não regra adotada neste plano.

### Pausa e indicadores de tempo

Motivos mínimos: **aguardando material/terceiros**, **interrupção por urgência autorizada**, **intervalo** e **fim de expediente**. Impedimento exige registro do que falta e de quem pode atuar; gestor acompanha pendências.

Medir separadamente tempo ativo, pausado, espera até início, espera por material, espera por confirmação e tempo total desde abertura. Pausa nunca apaga tempo já consumido nem faz o chamado sumir do dashboard. Exibir andamento e impedimentos sem atribuir ao técnico atraso causado exclusivamente por terceiros.

Timestamps e intervalos pertencem ao servidor; app deriva cronômetro desses registros. Eventos repetidos não abrem/fecham duas vezes o mesmo intervalo. Colaboradores têm tempo próprio, e tempo total do chamado não é simplesmente a soma dos tempos de todas as pessoas.

### Localização e proteção contra decisões com dados antigos

Mapa usa rótulo “Última posição conhecida”, horário e precisão; posição desatualizada não entra no cálculo de proximidade. Sem GPS válido, manter elegibilidade/notificação por secretaria. Atendimento remoto não usa proximidade por padrão.

Manter envio a cada 10 minutos durante execução, com interrupção na pausa e conclusão. Não deduzir que técnico não trabalhou porque o aparelho não enviou posição. Falha de GPS, conectividade e horário do último evento devem ficar visíveis ao administrador separadamente do estado do serviço.

### Conclusão, confirmação e pontuação

Conclusão do técnico encerra execução/localização e move para “Aguardando validação”. Usuário confirma ou aponta pendência. Configurar lembretes e alertar administrador quando não houver resposta; sem regra expressa, **não fechar automaticamente nem inferir satisfação**. Eventual encerramento administrativo deve ter permissão, justificativa e distinguir-se de confirmação pelo usuário.

Conceder pontos provisórios únicos na conclusão e definitivos na confirmação; regra para ausência de resposta deve ser definida antes de utilizar ranking oficial. Reabertura aplica ajuste auditável, sem apagar entrega ou deduzir fraude automaticamente. Distribuição entre principal e colaboradores considera complexidade, qualidade e contribuição e deve aparecer de forma explicável. Separar saldo provisório e confirmado no ranking. Impedimentos externos e recusa voluntária não geram penalidade automática.

### Parâmetros obrigatórios antes de operar

- [ ] Política de expediente/plantão, destinatários e tratamento de avisos fora do horário.
- [ ] Raio de proximidade e limites de idade/precisão da posição.
- [ ] Matriz de impacto/urgência, prazos de resposta e destinatários de escalonamento.
- [ ] Aprovação de interrupção por urgência e continuidade da tarefa pausada.
- [ ] Pesos, divisão de crédito entre colaboradores e tratamento de validação sem resposta.
- [ ] Prazo/frequência de lembretes e possibilidade de encerramento administrativo com motivo.

### Pendências de implementação adicionais

| Melhoria adotada | Situação atual | Implementar |
|---|---|---|
| Eventos separados de aviso/vínculo/início | Sem persistência operacional | Contratos/eventos auditados e transições autorizadas |
| Disponibilidade independente | Não implementada | Estado do técnico, expediente, conectividade e sincronização |
| Cooperação em chamado assumido | Não implementada | Oferta, aprovação, participantes e transferência explícita |
| Prioridade por impacto/urgência | Apenas prioridade fictícia | Campos, matriz configurável, justificativa e escalonamento |
| Pausa categorizada | Não implementada | Motivos, intervalos e indicadores de espera/execução |
| Confirmação sem resposta | Não implementada | Lembretes, alerta administrativo e estado pendente |
| Crédito provisório/confirmado | Não implementado | Livro de lançamentos idempotente, ajustes e ranking separado |

## 5. Passo a passo das pendências

P0: jornada e infraestrutura de atendimento. P1: demais requisitos obrigatórios e entrega. P2: diferenciais. P1 não significa opcional. Novos caminhos abaixo são sugestões, não arquivos já existentes.

### Passo 1 — Definir contratos e fluxo [P0]

- [ ] Conferir o que o backend já oferece antes de implementar serviços; incorporar eventos separados de aviso/vínculo/início, disponibilidade do técnico e regras da seção 4C.
- [ ] Definir IDs de usuário, solicitante, setor, unidade, equipe e executor.
- [ ] Acrescentar origem/ID externo, eventos tipados, anexos, impedimentos e validação ao contrato.
- [ ] Definir transições autorizadas e quem pode realizá-las: aberto → atribuído/iniciado → em andamento → bloqueado/retomado → aguardando validação → resolvido.
- [ ] Definir atribuição e reabertura como estados ou eventos, preservando responsáveis e horários.
- [ ] Estabelecer que pendência do demandante retorna ao atendimento para análise e continuidade.

**Arquivos:** extrair domínio de `src/data/tickets.ts`, por exemplo para `src/interface/ticket.interface.ts`.

**Aceite:** contratos e exemplos de entrada/saída documentados e acordados com backend; conclusão e confirmação são operações distintas.

### Passo 2 — Implementar autenticação e permissões [P0]

- [ ] Integrar login, identidade e sessão ao backend.
- [ ] Implementar os três papéis do produto: usuário da prefeitura, técnico e administrador. Mapear responsabilidades de atendente/demandante para o usuário autorizado e de gestor para o administrador, conforme seção 4A; preservar auditoria e permitir acumulação explicitamente autorizada.
- [ ] Proteger rotas e apresentar menus/ações segundo permissões.
- [ ] Implementar logout, sessão expirada, credencial inválida e acesso negado.
- [ ] Separar explicitamente modo demonstração e autenticação operacional.

**Arquivos:** login, `src/routes.tsx`, layout, cliente HTTP e novos hooks/serviços de sessão.

**Dependência:** backend deve aplicar autorização. Esconder botões não protege os dados.

**Aceite:** acesso direto sem sessão é tratado; usuário não realiza ação fora de seu papel; sair invalida sessão conforme contrato.

### Passo 3 — Consumir API e compartilhar a fonte dos dados [P0]

- [ ] Configurar `VITE_API_URL` e criar serviços em `src/services` e hooks em `src/hooks`.
- [ ] Substituir arrays locais por fonte compartilhada entre dashboard, chamados e detalhes.
- [ ] Tratar carregamento, vazio, erro, nova tentativa e indisponibilidade.
- [ ] Definir filtros, busca, ordenação e paginação conforme API.
- [ ] Atualizar lista e indicadores após mutações; manter fixtures apenas no modo demo.

**Arquivos:** `src/lib/api-client.ts`, páginas de dashboard/chamados e detalhes.

**Aceite:** chamado mantém dados iguais nas telas e após recarregar; erro de API não aparece como lista vazia.

### Passo 4 — Cadastrar e atribuir demandas; configurar equipes [P0]

- [ ] Criar formulário para usuário da prefeitura e administrador autorizados: descrição, solicitante, secretaria, departamento, unidade/local, área de atendimento, hardware/software, modalidade remota/presencial, equipamento quando aplicável e prioridade conforme permissão.
- [ ] Validar campos e impedir envio duplicado; incluir impacto, urgência, contato, necessidade de material e retirada; aplicar matriz de prioridade com alteração administrativa justificada.
- [ ] Criar configuração mínima de setores, equipes, membros, tipos de tarefa e fluxos.
- [ ] Permitir atribuição a pessoa ou equipe e registrar mudanças de responsável.
- [ ] Demonstrar inicialmente chamados de TI nas áreas configuráveis de Suporte, Telecom e TI/sistemas, em secretarias diferentes. Preservar configuração genérica para a exigência do manual; a demonstração fora da TI permanece pendência de compatibilidade, não escopo confirmado do produto.

**Arquivos:** chamados, novo formulário e página de equipes/configurações; habilitar menu ao concluir funcionalidade.

**Aceite:** usuário cadastra e acompanha apenas seus chamados; administrador pode cadastrar para outro solicitante com auditoria; técnico assume chamado permitido ou recebe atribuição. A abertura pelo usuário é alteração explícita em relação ao manual, registrada na seção 4A.

### Passo 5 — Construir a jornada do executor no celular [P0]

- [ ] Criar login e perfil do técnico com foto; listas de chamados disponíveis, vinculados/em andamento, pausados e concluídos; permitir assumir chamado aberto autorizado, sem atribuição duplicada.
- [ ] Implementar iniciar, registrar evento descritivo, pausar com motivo, registrar impedimento, solicitar apoio, retomar e concluir com descrição; exibir cronômetro de tempo ativo e interromper contagem/coleta durante pausa.
- [ ] Anexar e consultar evidências, com limites acordados com backend.
- [ ] Exibir envio em andamento, sucesso/erro e histórico atualizado; implementar disponibilidade independente, pausa categorizada e oferta/aprovação de ajuda em chamado já assumido.
- [ ] Tratar alterações concorrentes sem sobrescrever ações de terceiros silenciosamente.
- [ ] Conclusão deve levar a “Aguardando validação”, sem resolver automaticamente.

**Arquivos:** nova página de missões e componentes de ações; reutilizar detalhes e serviços.

**Aceite:** executor realiza atendimento no celular; gestor vê eventos persistidos com autor e horário.

### Passo 6 — Demonstrar push verdadeiro [P0]

- [ ] Escolher app ou web mobile, formatos permitidos pelo manual.
- [ ] Para web push: implementar service worker, inscrição do dispositivo, pedido contextual de permissão e registro da inscrição no backend.
- [ ] Integrar três notificações distintas: novo chamado da secretaria independentemente da distância, oportunidade voluntária por proximidade e vínculo/atribuição confirmada. Ao tocar, abrir o chamado correto e suas ações autorizadas; ver regras da seção 4A.
- [ ] Tratar permissão negada, dispositivo incompatível, inscrição inválida e logout; aplicar expediente/plantão, competência para cooperação, deduplicação, atualização de convite antigo e escalonamento da prioridade alta.
- [ ] Documentar transporte de push, dependências e compatibilidade com implantação municipal.

**Dependência:** backend de envio, ambiente seguro e dispositivo/navegador compatível.

**Aceite:** abertura na secretaria notifica técnico mesmo distante; abertura próxima em outra secretaria oferece aceite voluntário; atribuição notifica o vinculado. Testar técnico da Saúde atendendo no hospital quando surge outro chamado naquela unidade. Tocar abre tarefa respeitando acesso. Toast, badge e temporizador não comprovam push.

### Passo 7 — Implementar confirmação e avaliação do demandante [P0]

- [ ] Criar perfil do usuário e “Meus atendimentos”, restrito às demandas autorizadas, com abertos/em andamento/fechados, responsável, início e eventos, incluindo indicação de pausa e conclusão aguardando confirmação.
- [ ] Mostrar entrega e evidências permitidas.
- [ ] Permitir confirmar resolução e avaliar o atendimento.
- [ ] Permitir informar problema persistente com descrição da pendência.
- [ ] Encaminhar pendência para análise/continuidade preservando a conclusão anterior; implementar lembretes e alerta por ausência de confirmação, sem fechamento automático não configurado.

**Aceite:** demonstrar confirmação em uma tarefa e rejeição em outra; só confirmação gera resolução validada.

### Passo 8 — Demonstrar integração bidirecional e recuperação [P0]

- [ ] Preparar com backend um legado simulado para enviar demandas e receber eventos.
- [ ] Exibir origem e ID original; mostrar sincronização/falhas aos perfis autorizados.
- [ ] Retornar andamento, impedimentos, conclusão e validação conforme recursos do contrato.
- [ ] Garantir identificação única por origem + ID externo e idempotência de eventos no backend.
- [ ] Preservar eventos em armazenamento durável e reenviar após falha de comunicação.

**Dependência:** conector, idempotência e fila são principalmente responsabilidades do backend; frontend acompanha resultados.

**Aceite:** entrada repetida não duplica chamado; legado indisponível recebe eventos pendentes quando volta, sem perda/duplicação.

### Passo 9 — Implementar gamificação configurável [P1, obrigatório]

- [ ] Criar pontos/conquistas e rankings individual e por equipe, com setor e período.
- [ ] Configurar pesos de qualidade, complexidade e colaboração por perfil autorizado.
- [ ] Definir pontos por chamado e exibir justificativa. Conclusão do técnico concede pontuação provisória única; confirmação torna definitiva e rejeição aplica ajuste auditável. Essa política foi adotada na revisão; pesos, divisão entre colaboradores e ausência de resposta permanecem parâmetros a definir. Cooperação entre secretarias deve ter crédito explícito sem duplicar integralmente pontos.
- [ ] Implementar crédito provisório/confirmado separado, divisão explicável entre colaboradores e ajustes idempotentes; definir tratamento justo de impedimentos externos e evitar premiar apenas volume/velocidade.
- [ ] Implementar períodos, inclusive mensal, com novo placar sem apagar histórico.

**Dependência:** cálculo e histórico persistidos no backend.

**Aceite:** regras ajustáveis alteram resultado demonstrável; novo período reinicia placar e mantém consulta ao anterior.

### Passo 10 — Integrar aplicação útil de IA [P1, obrigatório]

- [ ] Escolher função concreta, como sugerir categoria/equipe e apontar dados faltantes no cadastro.
- [ ] Exibir sugestão, justificativa e opções aceitar/editar/recusar, com erro/carregamento.
- [ ] Exigir conferência humana para alterações de prioridade, encaminhamento ou avaliação.
- [ ] Chamar pelo backend; não expor credenciais em variáveis `VITE_*`.
- [ ] Documentar dados utilizados, componente e execução compatível com infraestrutura municipal.
- [ ] Identificar componentes simulados e plano de implementação definitiva.

**Aceite:** descrição gera auxílio útil e revisável; apresentação distingue funcional de simulado. Menu/texto promocional não comprova IA.

### Passo 11 — Corrigir e ampliar indicadores [P1]

- [ ] Usar dados compartilhados, datas/eventos reais, períodos e fuso definidos.
- [ ] Calcular estados, tempo até início, execução, confirmação, motivos de bloqueio, reaberturas e satisfação conforme dados disponíveis.
- [ ] Explicar cada cálculo, distinguindo recebidos no período e resolvidos no período.
- [ ] Tratar zero registros, divisão por zero e ausência de avaliações.
- [ ] Permitir abrir atendimento a partir dos pontos de atenção do dashboard; filtrar por secretaria, área, técnico, prioridade, estado e período; mostrar entregas dos técnicos e tempo ativo versus pausado.

**Aceite:** conclusão, confirmação e reabertura alteram indicadores coerentemente, sem apagar histórico.

### Passo 12 — Validar interface e comportamento [P1]

- [ ] Verificar fluxo completo em desktop e celular real.
- [ ] Validar teclado, foco/modal, labels, mensagens, contraste, zoom e leitor de tela.
- [ ] Garantir descrição textual do mapa e identificação dos estados além das cores.
- [ ] Oferecer lista/cartões para execução mobile sem depender do Kanban largo.
- [ ] Adicionar testes de comportamento para permissões, conclusão versus validação, reabertura e falha de API; testar aceite concorrente, cooperação sem troca de principal, uma execução ativa, pausa sem disponibilidade automática, aviso sem GPS e pontuação sem duplicação.
- [ ] Executar lint, TypeScript, build e roteiro integrado com backend/push.

**Aceite:** jornada e cenários de falha funcionam no dispositivo da demonstração, sem perda de informações ou ações indevidas.

### Passo 13 — Documentar e preparar implantação [P1, obrigatório]

- [ ] Atualizar README com pasta correta, rotas, papéis, variáveis e limites reais.
- [ ] Documentar instalação, configuração, build e publicação SPA com fallback para `index.html`.
- [ ] Documentar HTTPS, push/service worker e atualização de versão/caches quando aplicável.
- [ ] Relacionar dependências, licenças, custos e serviços externos; definir tiles/configuração municipal do mapa.
- [ ] Entregar código e instruções integradas de sistema/banco com backend.
- [ ] Identificar componentes funcionais, simulados e futuros.
- [ ] Preparar dados fictícios e roteiro; não usar nomes/contatos reais da foto.

**Aceite:** outra pessoa instala e executa seguindo documentação e entende como hospedar no datacenter municipal.

### Passo 14 — Implementar monitoramento e mapa administrativo [P0 nas novas regras]

- [ ] Implementar captura real de localização durante atendimento, com envio a cada 10 minutos; obter posição ao iniciar/retomar e registrar horário e precisão. Coleta ao iniciar/retomar é proposta para evitar uma janela inicial sem posição.
- [ ] Tratar permissão negada, aparelho sem sinal, app em segundo plano e eventos offline; suspender coleta na pausa e conclusão, sem apagar histórico. Alertar quando posição estiver desatualizada e não exibi-la como posição atual.
- [ ] Criar mapa agregado do administrador, com última posição conhecida dos técnicos autorizados; clique abre modal com foto/nome, secretarias, chamado atual, estado, tempo ativo/pausado, horário e precisão da posição. Técnicos sem posição válida aparecem em lista, sem coordenada inventada.
- [ ] Encerrar rastreamento por evento real de pausa/conclusão, não pelo fim do trajeto calculado; administrar sessão ativa no servidor.
- [ ] Validar em dispositivo real a captura em segundo plano e a cadência de 10 minutos; não considerar temporizador JavaScript de página suficiente. Escolha do formato do app depende dessa validação.
- [ ] Deixar offline completo, QR Code e voz como diferenciais P2.

**Aceite:** durante execução, o app envia localização a cada 10 minutos nas condições suportadas e medidas no aparelho; pausa interrompe captura/envio e cronômetro; retomada inicia novo intervalo; conclusão encerra sessão. Mapa indica última atualização e abre informações corretas do técnico. Localização era negociável no manual, mas passa a obrigatória no produto pelas novas regras; offline completo permanece diferencial.

## 6. Ordem recomendada de entregas

1. Contratos, sessão e dados comuns: passos 1–3.
2. Cadastro, atribuição/aceite, execução, cronômetro, localização, mapa, push e validação: passos 4–7 e 14.
3. Integração de entrada/saída e recuperação: passo 8.
4. Gamificação, IA e indicadores: passos 9–11.
5. Validação e implantação documentada: passos 12–13.
6. Diferenciais posteriores: offline completo, QR Code e voz. O monitoramento do passo 14 é P0 nas novas regras.

Todos os requisitos obrigatórios precisam ser demonstrados no conjunto final. Evitar refinar apenas aparência enquanto faltam operações da jornada.

## 7. Checklist da apresentação

- [ ] Atendente registra uma demanda fictícia no legado simulado.
- [ ] Plataforma recebe e mantém origem/ID.
- [ ] Administrador atribui e técnico recebe push de vínculo no celular.
- [ ] Técnico da Saúde recebe abertura no hospital e também quando distante, sem depender de proximidade.
- [ ] Técnico da Educação próximo recebe convite voluntário, aceita sem alterar secretaria original; recusa não penaliza.
- [ ] Dois técnicos disputando aceite não geram responsáveis duplicados.
- [ ] Pausa interrompe tempo ativo e localização; retomada continua histórico; mapa administrativo mostra horário da última posição.
- [ ] Executor inicia, registra falta de material e gestor acompanha.
- [ ] Executor retoma, conclui e anexa evidência.
- [ ] Demandante confirma/avalia; outro exemplo rejeita e retorna ao atendimento.
- [ ] Legado recebe atualizações; repetição/falha temporária comprovam recuperação.
- [ ] Pontos e indicadores atualizam; novo período preserva histórico anterior.
- [ ] IA auxilia uma decisão com revisão humana.
- [ ] Chamados de Saúde e Educação demonstram vínculo por secretaria; configuração para outros setores preserva arquitetura genérica. Demonstração fora da TI continua pendência específica do manual.
- [ ] Equipe explica implantação, dependências e limitações.

- [ ] Prioridade deriva de impacto/urgência; ausência de candidato e prazo excedido alertam administrador.
- [ ] Chamado já assumido permite oferta de ajuda sem substituir responsável.
- [ ] Falta de confirmação gera lembrete/alerta, sem resolução automática.
- [ ] Ranking distingue pontos provisórios e confirmados; eventos repetidos não duplicam crédito.

## 8. Verificações e limites desta análise

- `npm run lint`: aprovado, sem erros reportados.
- `node node_modules/typescript/bin/tsc --noEmit -p tsconfig.app.json`: aprovado.
- `node node_modules/typescript/bin/tsc --noEmit -p tsconfig.node.json`: aprovado.
- Código das páginas, rotas, layout, dados, detalhes, mapa, HTTP e configurações inspecionado.
- Build de produção e testes de navegador/dispositivo não executados. Pasta `dist` existente não comprova correspondência com código atual.
- Backend, integrações externas e segurança em execução não auditados.
- Nenhum arquivo de implementação foi alterado nesta análise ou na revisão de regras. Os resultados de lint/TypeScript acima pertencem à análise inicial; não foram repetidos nesta alteração apenas documental.

Verificações estáticas aprovadas não comprovam os fluxos de negócio exigidos pelo manual. O documento é um plano técnico baseado nesta versão do frontend; atualizar o checklist conforme implementação.



## 9. Atualização do produto e próximas entregas — 09/10/2026

Esta seção registra o estado mais recente e a sequência acordada para o frontend. As tabelas e checklists da análise inicial acima são históricos; itens já entregues abaixo não devem ser tratados como pendências da fase mockada. Integração operacional com backend permanece futura.

### Estado atual e validação antes da próxima entrega

A jornada cadastro → aceite/atribuição → início → eventos → pausa/bloqueio/retomada → conclusão → confirmação/avaliação ou pendência → análise/devolução administrativa já funciona com mocks e persistência local. Também foram entregues sessão demonstrativa por papel, gestão de técnicos/disponibilidade, central de avisos por conta e mapa administrativo de últimas posições fictícias.

O mapa oferece filtro por secretaria, lista textual e modal com técnico, atendimento, disponibilidade, tempos e horário/precisão da posição. Capturas simuladas são persistidas durante execução com a aplicação aberta, respeitando intervalo de 10 minutos; pausa, bloqueio e conclusão interrompem novas capturas. Histórico permanece disponível, e posições antigas/imprecisas são sinalizadas. Sem posição, não inventar coordenadas do técnico. Foto ainda pendente.

Chamado presencial livre pode gerar convite voluntário por proximidade a técnico de outra secretaria, com tipo compatível, disponibilidade e posição válida. O aceite revalida elegibilidade, preserva a secretaria original e vincula sem iniciar atendimento. Não há colaboração aprovada em chamado já assumido nesta entrega. Parâmetros de demonstração: 2 km, idade máxima de 15 minutos e precisão até 100 m; não representam regras municipais definitivas. Atendimento remoto não usa proximidade. Avisos da própria secretaria independem de distância e ocupação.

- [x] Lint, build e verificações de comportamento do mapa/convites e ciclo integrado aprovados na entrega.
- [ ] Validar visualmente mapa, modal, posições antigas/indisponíveis e convites em navegador desktop e celular antes de avançar.
- [ ] Conferir pausa/retomada/conclusão e acesso por papel no roteiro visual.

GPS real, execução em segundo plano, push para outro dispositivo e backend não foram entregues. Persistência é local ao navegador; guardas e credenciais são demonstrativas. O build mantém aviso de bundle acima de 500 kB.

### Próxima etapa: cooperação em chamado já assumido

Implementar primeiro com serviços mockados e persistência local, mantendo a futura substituição pela API. Cooperação não deve substituir automaticamente o responsável principal nem alterar a secretaria de origem.

- [ ] Permitir que técnico elegível ofereça ajuda em chamado já assumido, respeitando competência/tipo e acesso autorizado.
- [ ] Permitir aprovação da oferta pelo responsável principal ou administrador; oferta pendente não cria vínculo nem inicia execução.
- [ ] Ao aprovar, registrar o técnico como colaborador, com papel explícito; manter um único responsável principal.
- [ ] Cada participante registra eventos com autoria e intervalos próprios de trabalho; não compartilhar um cronômetro indistinto.
- [ ] Aplicar uma execução ativa por técnico, inclusive como colaborador; vínculo em fila não inicia tempo.
- [ ] Aplicar pausa/retomada e interrupção de captura ao intervalo do participante, preservando histórico e disponibilidade independente.
- [ ] Persistir oferta, decisão, participantes, eventos e tempos; impedir duplicação de vínculo e avisos em ações repetidas.
- [ ] Recusa voluntária ou ausência de resposta ao convite não gera penalidade automática.
- [ ] Restringir consulta e ações aos participantes/perfis autorizados e emitir avisos mockados separados para oferta e decisão.
- [ ] Preservar conclusão e validação como operações distintas; colaboração não autoriza o técnico a confirmar a resolução em nome do solicitante.

Critério de aceite: demonstrar oferta → aprovação → vínculo como colaborador → execução com eventos e tempo próprios, preservando responsável/secretaria/histórico. Mostrar oferta pendente sem execução, tentativa de segunda execução ativa bloqueada e recarga sem perda ou duplicação. Validar em desktop/celular, executar lint/build e verificações de comportamento.

Antes de implementar transições, explicitar o efeito da pausa/conclusão do responsável sobre colaboradores ativos e quem pode encerrar a entrega global. A política precisa preservar intervalos, autorização e uma execução ativa por técnico; não presumir encerramento silencioso de sessões de terceiros.

### Ordem das entregas seguintes

1. Cooperação aprovada em chamado já assumido, conforme critérios acima.
2. Perfil editável e foto do técnico: persistência mockada, validação do arquivo e exibição no perfil/mapa; upload operacional futuro.
3. Evidências do atendimento: anexar e consultar com autoria, horário, limites e acesso por perfil; preservar vínculo ao atendimento/histórico.
4. Ranking e pontos: crédito provisório na conclusão e confirmado na validação; contribuição dos colaboradores e ajustes de reabertura explicáveis, auditáveis e sem duplicação; rankings individual/equipe por período, preservando histórico.

Pesos, divisão de crédito, tratamento de ausência de confirmação e parâmetros operacionais continuam pendentes de definição. Estas etapas são planejadas, não funcionalidades concluídas. A sequência não remove requisitos futuros de backend, GPS/push reais, integração com legado e implantação, nem a diferença já documentada entre o escopo atual de IA e o manual.

## 10. Regras de negócio confirmadas para o backend — 10/10/2026

Esta atualização substitui a necessidade de uma tabela `Secretarias` para o escopo contratado. O produto será vendido a prefeituras municipais e cada prefeitura representa um tenant isolado.

As referências anteriores a atribuição/notificação por secretaria ficam substituídas por responsabilidade da unidade, vínculo técnico-unidade e proximidade válida. Não persistir secretaria como entidade nem usar secretaria como critério de tenancy. Regras antigas que descrevem Saúde/Educação continuam apenas como exemplos históricos do protótipo e não como requisito de schema.

### Prefeitura, usuários, técnicos e unidades

- `Prefeitura` é o cliente/tenant. Usuários, técnicos, unidades e chamados devem ficar vinculados e isolados por prefeitura.
- Não criar tabela `Secretarias` nesta etapa. As unidades são pontos de atendimento de uma prefeitura.
- `UnidadeAtendimento` possui nome, endereço e coordenadas obrigatórias de latitude/longitude para permitir localização e proximidade. A migration mantém unidades legadas sem coordenadas como nulas, mas novos cadastros devem informar ambas.
- `Tecnico` é um perfil próprio ligado a um usuário. Um técnico pode atender várias unidades e uma unidade pode ter vários técnicos; a relação é `TecnicoUnidadeAtendimento`, com indicação de responsável principal.
- A unidade escolhida no chamado determina a prefeitura e os técnicos elegíveis. Na abertura, somente o responsável principal ativo e disponível recebe atribuição automática; se não existir, o chamado fica `Aberto`, sem responsável. A unicidade do responsável é protegida pelo banco.

### Histórico, comentários e participantes

- `EventoChamado` mantém tipo, descrição, autor opcional para eventos de sistema e data/hora do evento.
- Abertura do chamado, atribuição/vínculo de responsável e conclusão devem gerar eventos automáticos. O técnico responsável pode registrar eventos descritivos de progresso. Eventos são append-only e não substituem o estado atual do chamado.
- `ComentarioChamado` guarda comentários do solicitante e dos técnicos, com autor e data/hora. Comentários e eventos são registros diferentes: comentário é conversa; evento é trilha operacional.
- `ParticipantesChamado` registra técnicos colaboradores mantendo apenas um responsável principal. Vínculo de participante não inicia atendimento automaticamente.

### Localização, push e anexos

- `PosicoesTecnico` armazena latitude, longitude, precisão, data/hora e chamado associado quando houver. Segue a regra anterior de captura durante execução, a cada 10 minutos, interrompida em pausa ou conclusão.
- `NotificacoesPush` persiste notificações pendentes, enviadas e com falha, incluindo tentativas, horário e chave de deduplicação. `DispositivosPush` guarda endpoints/credenciais de inscrição por usuário para permitir o envio ao dispositivo.
- `AnexosChamado` guarda metadados, autor, tipo e conteúdo da imagem de entrega em Base64 no banco, conforme decisão do produto. A API aceita JPEG, PNG ou WEBP até 8 MB e valida formato, tamanho e assinatura do arquivo.
- Para concluir tecnicamente o atendimento, o responsável envia uma imagem `FotoEntrega`. A API grava anexo, evento `ConclusaoTecnica` e etapa `Aguardando validação` na mesma transação. O status permanece `Em Análise` até confirmação do solicitante, que então muda para `Resolvido`.
- O nível de complexidade do chamado define a pontuação no backend: Baixo = 5, Médio = 10, Alto = 15; prioridade `Alta` acrescenta 5 pontos. Chamado aberto pelo usuário inicia em Baixo; gestor pode definir a complexidade. Pontos não são aceitos do cliente. Na conclusão técnica, o lançamento idempotente soma pontos ao saldo do técnico e recalcula sua patente. O crédito fica provisório conforme política vigente; o progresso da patente considera o saldo acumulado.
- As patentes são Soldado raso (0), Soldado (100), Cabo (250), Sargento (500), Subtenente (800), Tenente (1200), Capitão (1800), Major (2600), Coronel (3600) e General (5000). O ranking retorna patente atual/próxima, símbolo, pontos faltantes, percentual da faixa e classes Tailwind.

### Tabelas presentes no modelo atual

`Prefeituras`, `Usuarios`, `UnidadesAtendimento`, `Chamados`, `Tecnicos`, `TecnicoUnidadeAtendimento`, `EventosChamado`, `ComentariosChamado`, `ParticipantesChamado`, `PosicoesTecnico`, `NotificacoesPush`, `DispositivosPush` e `AnexosChamado`.

Essa lista descreve o modelo/schema preparado. Foto de entrega é salva em Base64; push real e confirmação do solicitante continuam fora deste escopo. Migrations posteriores acrescentam responsável de unidade, conteúdo Base64, pontos no técnico/chamado e patente. A migration cria uma prefeitura inicial de São José dos Pinhais e dados fictícios de demonstração. A inscrição de push semeada é apenas um valor de teste.


## Status do chamado — 10/10/2026

Catálogo vigente: **Aberto, Em Análise e Resolvido**. Esta atualização substitui grafias anteriores. Vínculo, execução, pausa, bloqueio e espera de validação/análise são etapas internas. Conclusão pelo técnico mantém Em Análise; confirmação do solicitante marca Resolvido.

Backend: enum StatusChamado, resposta JSON textual e filtro com validação do catálogo. EtapaAtendimento preserva os estados operacionais anteriores. Migration 20261010130004_SepararStatusEtapaChamado converte dados sem apagar histórico e recusa status desconhecidos. Migration ainda não aplicada ao banco operacional nesta entrega.

Escopo atual: somente backend. As alterações de status feitas inicialmente no frontend foram desfeitas a pedido do usuário; sua adaptação fica para a integração futura. Priorização por IA permanece para entrega futura.
