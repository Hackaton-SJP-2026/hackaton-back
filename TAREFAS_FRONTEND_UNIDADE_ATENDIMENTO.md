# Tarefas de frontend — unidade de atendimento e atribuição imediata

## Objetivo
Implementar o cadastro de unidades de atendimento e a regra de atribuição automática por unidade, respeitando os fluxos de web e app já previstos.

## Regras a atender

- O administrador cadastra unidades de atendimento pela plataforma web.
- Cada técnico pode ser responsável por uma ou mais unidades.
- O chamado deve conter a unidade de atendimento no cadastro.
- Ao abrir um chamado para uma unidade, o sistema identifica os técnicos responsáveis por essa unidade.
- O chamado é vinculado imediatamente ao técnico responsável.
- Um push/aviso é enviado ao técnico responsável no momento da atribuição automática.
- O vínculo por unidade deve coexistir com as regras de secretaria, proximidade e cooperação.
- A unidade deve aparecer no filtro, no detalhe do chamado e na tela do técnico.

## Tarefas de frontend

### 1. Cadastro de unidades
- [ ] Criar tela de cadastro de unidades de atendimento na web.
- [ ] Campos mínimos: nome da unidade, secretaria, departamento, endereço/local, coordenadas, status e observações.
- [ ] Salvar unidade com identificador único e persistência local/mockada.
- [ ] Listar unidades cadastradas com filtro por secretaria e status.
- [ ] Permitir editar e desativar unidade.
- [ ] Validar campos obrigatórios antes do envio.

### 2. Vincular técnicos às unidades
- [ ] Criar formulário de associação técnico ↔ unidade.
- [ ] Permitir que um técnico pertença a uma ou mais unidades.
- [ ] Exibir na tela de perfil técnico as unidades responsáveis.
- [ ] Validar duplicidade de vínculo entre técnico e unidade.
- [ ] Exibir em dashboard/admin a lista de responsáveis por unidade.

### 3. Cadastro do chamado com unidade
- [ ] Incluir campo de unidade de atendimento no formulário de abertura de chamado.
- [ ] Popular a lista de unidades conforme a secretaria/departamento selecionado.
- [ ] Validar que a unidade selecionada seja válida para o cadastro.
- [ ] Incluir a unidade no detalhe do chamado e na lista de chamados.
- [ ] Exibir a unidade também em filtros e buscas.

### 4. Atribuição automática por unidade
- [ ] Ao abrir o chamado, identificar os técnicos vinculados à unidade.
- [ ] Aplicar regra de atribuição imediata conforme política configurada.
- [ ] Se houver mais de um técnico, definir prioridade de seleção: primeiro elegível, fila, ou regra administrativa.
- [ ] Registrar o vínculo imediatamente no histórico do chamado.
- [ ] Garantir que o responsável principal não seja duplicado quando a unidade tiver mais de um técnico elegível.

### 5. Avisos e push
- [ ] Emitir notificação de atribuição imediata para o técnico responsável.
- [ ] Exibir mensagem com nome da unidade, número do chamado e prioridade.
- [ ] Ao abrir a notificação, levar o usuário diretamente ao chamado vinculado.
- [ ] Tratar ausência de permissão de push e fallback em toast/local.

### 6. Visibilidade no app
- [ ] Exibir chamados atribuídos por unidade na lista do técnico.
- [ ] Mostrar a unidade em detalhes, histórico e painel de execução.
- [ ] Exibir um badge/indicador de vínculo por unidade no fluxo de atendimento.
- [ ] Garantir que a tela do técnico mostre chamados recebidos por responsabilidade da unidade.

### 7. Segurança e regras de acesso
- [ ] Restringir edição de unidades a perfis de administrador.
- [ ] Restringir a visualização de unidades a usuários autorizados.
- [ ] Garantir que o técnico veja apenas chamados da(s) unidade(s) sob sua responsabilidade.
- [ ] Bloquear atribuição manual conflitante com a regra de unidade quando a política exigir vinculação automática.

### 8. Validação de comportamento
- [ ] Cadastrar unidade e vincular técnico.
- [ ] Abrir chamado para unidade e verificar atribuição automática.
- [ ] Verificar push/aviso para o técnico responsável.
- [ ] Validar que o técnico recebe o chamado sem ação manual.
- [ ] Validar cenário com múltiplos técnicos por unidade.
- [ ] Validar cenário sem técnico vinculado à unidade.
- [ ] Validar recarga da tela sem perder vínculo e histórico.

## Critério de aceite
- Um chamado aberto para uma unidade deve gerar vínculo imediato ao técnico responsável.
- O técnico responsável recebe aviso/push ao abrir o chamado.
- A unidade aparece no cadastro do chamado e no fluxo do técnico.
- A lógica funciona tanto na web quanto no app de atendimento.
- A regra não quebra as regras de secretaria, proximidade e cooperação já existentes.
