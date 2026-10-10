# Hackaton Backend

Esqueleto C#/.NET 8 baseado em `C:\Projeto\portifolio-back\portfolio-back` e no
[guia DDD](GUIA_BACKEND_DDD_HACKATON.md). Mantém as responsabilidades da Application,
entidades base e serviços genéricos da referência. Não usa CQRS ou MediatR.

## MariaDB Hostinger — configuração vigente em 10/10/2026

- [x] Servidor srv1437.hstgr.io:3306 (IP alternativo 193.203.175.85), banco e usuário u204118466_hackaton. A conexão e a senha estão no appsettings.json por autorização do responsável pelo projeto.
- [x] Versão consultada no servidor: 11.8.9-MariaDB-log; Persistence:ServerVersion=11.8.9 evita detecção remota durante a inicialização.
- [x] Pomelo.EntityFrameworkCore.MySql 8.0.3 e EF Core/Design/SQLite/dotnet-ef 8.0.13. [Documentação do provedor](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql).
- [x] MariaDbAppDbContext compartilha entidades e mapeamentos com AppDbContext. Repositórios, UnitOfWork e controllers continuam consumindo AppDbContext; ambos resolvem a mesma instância por escopo no MariaDB.
- [x] Migration inicial própria 20261010131335_InicialMariaDb aplicada ao banco antes vazio: 14 tabelas de negócio e __EFMigrationsHistory; zero migrations pendentes. As migrations SQLite não são executadas neste contexto.
- [x] UTF-8 utf8mb4, GUIDs char(36), datas datetime(6), enum persistido em texto e EtapaAtendimento presentes desde a criação. Nenhuma mudança nas regras de negócio ou nos três status públicos.
- [x] Endpoint de DispositivoPush usa ASCII e collation ascii_bin para manter índice único completo de URL de até 2.000 caracteres dentro do limite do InnoDB. URLs de push devem estar codificadas como URLs ASCII; demais textos usam utf8mb4. Nenhum endpoint de negócio foi criado nesta etapa.
- [x] Dockerfile, Compose e perfil Docker do Visual Studio deixaram de sobrescrever a conexão com SQLite; usam a configuração MariaDB do appsettings.
- [x] Conexão remota, migration e escrita/leitura EF conferidas: GUIDs, vínculos, enum/etapa, consulta usada no login e Unicode (incluindo emoji). Transação de teste revertida; nenhum dado demonstrativo foi inserido permanentemente.
- [x] Suíte de 27 testes aprovada. Testes automatizados usam SQLite temporário/em memória ou geração offline de SQL MariaDB, sem depender da Hostinger.
- [ ] Cadastrar prefeitura e usuários com PasswordHash válido no banco novo. Os seeds históricos do SQLite não foram importados; o banco MariaDB contém somente a estrutura.
- [ ] Habilitar/configurar JWT para usar login real; Authentication:Enabled continua false. Definir VITE_PREFEITURA_ID no frontend conforme a prefeitura cadastrada.
- [ ] Validar Docker em host com Docker disponível e os fluxos futuros de negócio conforme implementados.

### Comandos para a equipe

Executar a partir da raiz do backend:

```powershell
dotnet restore HackatonApi/HackatonApi.csproj
dotnet tool restore
dotnet ef database update --context MariaDbAppDbContext --project Hackaton.Infrastructure --startup-project HackatonApi
dotnet run --project scripts/MariaDbCheck -- HackatonApi/appsettings.json
dotnet run --project HackatonApi
```

MariaDbCheck consulta versão e migrations e verifica escrita/leitura em transação revertida. Para migrations futuras, usar sempre --context MariaDbAppDbContext e --output-dir Data/Migrations/MariaDb:

```powershell
dotnet ef migrations add NomeDaAlteracao --context MariaDbAppDbContext --project Hackaton.Infrastructure --startup-project HackatonApi --output-dir Data/Migrations/MariaDb
```

Não usar EnsureCreated no banco operacional: as alterações são aplicadas por migrations, em etapa separada do boot. Para teste local legado, configurar Persistence:Provider=Sqlite junto de uma conexão SQLite e usar --context AppDbContext. A pendência dos seeds históricos SQLite permanece restrita ao legado; não impede o banco MariaDB, cuja estrutura já foi aplicada. As orientações SQLite mais abaixo são registros históricos.

## Estado anterior à troca de banco — 10/10/2026

Este README é o registro de andamento do backend: atualizar implementação, validação e
pendências após cada entrega. O trabalho atual está restrito ao backend; o frontend será
adaptado durante a integração futura. Referências técnicas antigas nas seções seguintes
devem ser lidas junto deste estado consolidado.

### O que está implementado

- [x] Cinco projetos .NET 8, bases de entidades, serviços/repositórios genéricos,
  UnitOfWork, auditoria, cache, Mapster, validação e injeção de dependências.
- [x] SQLite configurado, modelos e migrations para prefeitura, usuários, unidades,
  chamados, técnicos e vínculos, eventos, comentários, participantes, posições,
  dispositivos/notificações push, anexos e lançamentos de pontos.
- [x] Swagger em Development, CORS, tratamento de erros e GET /api/status.
- [x] Login web e app com hash de senha e emissão de JWT; definição administrativa de senha.
- [x] CRUD administrativo de usuários, unidades e chamados, com verificações por prefeitura.
- [x] Portal do usuário: abertura, listagem, detalhes, eventos e comentários dos próprios chamados.
- [x] App técnico: consultas por categoria, ranking, registro de posição, eventos e comentários.
- [x] Gestor: consulta de chamados com filtros/paginação e posições atuais/histórico dos técnicos.
- [x] Enum StatusChamado com os únicos status públicos: **Aberto, Em Análise e Resolvido**.
- [x] JSON e Swagger apresentam os três nomes textuais; filtros recusam status inválidos.
- [x] EtapaAtendimento separada do status público; consultas de fila, execução e GPS usam a etapa.
- [x] Migration 20261010130004_SepararStatusEtapaChamado preserva as etapas antigas e o histórico,
  converte os status e interrompe a conversão quando encontra um valor desconhecido.

Entidade/tabela preparada não significa fluxo entregue. Ainda não existem comandos
operacionais para atribuir, assumir, iniciar, pausar, retomar, concluir ou validar chamados.
As regras de conclusão e confirmação estão definidas, mas essas operações ainda precisam
ser implementadas. O ranking consulta lançamentos; sua geração automática não existe.

Autenticação está desabilitada por padrão: ativar Authentication:Enabled e configurar
Issuer, Audience e SigningKey para usar login/JWT. Com autenticação desabilitada, login
retorna 503 e ações protegidas retornam 401. GET /api/status verifica somente o processo HTTP.

### O que foi validado na etapa de status

- Build do backend aprovado.
- Suíte completa: **24 testes aprovados, zero falhas**.
- Testes novos verificam os três valores JSON, rejeição de valores inválidos, conversão dos
  oito estados antigos, preservação de eventos, consultas após conversão, aplicação repetida
  e recusa de status desconhecido sem sobrescrever os registros.
- Swagger verificado com catálogo textual de status. Expectativa antiga de leitura pública
  no controller de teste corrigida para 401, preservando a autorização já existente na base.
- Nova migration validada sobre schema legado preparado em SQLite temporário.

Essas verificações não comprovam login operacional, isolamento completo entre prefeituras,
push, GPS em aparelho, Docker ou jornada integrada. Os testes atuais incluem infraestrutura
e a nova mudança de status; os fluxos de negócio futuros precisam de seus próprios testes.

### Próxima etapa: corrigir migrations e preparar o banco

- [ ] Corrigir os seeds históricos: a execução desde um banco vazio falhou porque operações
  InsertData não encontram metadados de coluna no TargetModel (erro em Prefeituras.Ativa).
- [ ] Validar a cadeia completa de migrations em banco SQLite novo e conferir os dados demonstrativos.
- [ ] Validar a atualização de banco existente com preservação de registros e histórico.
- [ ] Aplicar a migration de status ao banco operacional/local e registrar a verificação.

**A migration de status ainda não foi aplicada ao banco operacional.** Os testes aprovados
da nova migration usam schema legado preparado e não certificam a cadeia histórica de seeds.

### Pendências seguintes, na ordem recomendada

1. **Técnicos e unidades:** APIs de gestão de técnicos, disponibilidade e vínculos técnico-unidade;
   consulta de unidades autorizadas para o solicitante; competências/tipos compatíveis.
2. **Contratos:** alinhar tipos de atendimento, campos do chamado, identificação/número público
   e papéis; prefeitura é o tenant e unidade define responsabilidade. Sem tabela Secretarias.
3. **Atribuição:** atribuir automaticamente por unidade, registrar evento/aviso e tratar ausência
   ou múltiplos candidatos; permitir atribuição/transferência administrativa com justificativa.
4. **Execução:** assumir, iniciar, pausar, bloquear, retomar e concluir; intervalos persistidos,
   timestamps do servidor, uma execução ativa por técnico, concorrência e idempotência.
5. **Entrega e validação:** upload/listagem/download de anexos; exigir foto FotoEntrega realmente
   enviada na conclusão; confirmação/nota do solicitante, pendência e devolução administrativa.
6. **Comunicação/localização:** central de avisos e leitura, inscrição de dispositivos, geração
   e envio real de push com tentativas; proximidade e dados suficientes para mapa administrativo.
7. **Cooperação:** oferta/aprovação, participantes com tempos próprios e política de encerramento global.
8. **Gamificação:** créditos provisórios/confirmados/revisão gerados nas transições, ajustes sem duplicação,
   rankings técnico/administrativo, período America/Sao_Paulo, empates e dez patentes; equipes depois.
9. **Perfil e acompanhamento:** edição/foto próprias, indicadores administrativos e auditoria adequada.
10. **Qualidade e operação:** testes de autorização/isolamento por prefeitura e jornada completa;
    adequar controllers ao fluxo AppService/DomainService/Repository/UoW; validar implantação,
    migrations, persistência e Docker. Integração com legado e frontend permanece futura.

### Priorização por IA — entrega futura

O usuário definiu que a prioridade será determinada por um agente de IA a integrar depois.
Não implementar matriz manual ou agente nesta fase. O campo Prioridade já existe; abertura
pelo portal usa Normal como fallback atual.

A interface para o agente ainda é uma proposta: entrada com dados/impacto do chamado,
saída com prioridade e justificativa, origem/versão, histórico e reprocessamento idempotente.
Sua implementação deve ficar no backend, sem bloquear abertura/atendimento quando o agente
estiver indisponível. Essa preparação não foi implementada na entrega de status.

## Requisitos e execução

Projetos com alvo `net8.0`, nullable e implicit usings habilitados. A solução `Hackaton.slnx`
foi preparada com SDK 10.0.401; use um SDK que suporte esse formato. Em ambientes que
precisem apenas do SDK 8, converta para `.sln` antes de executar comandos da solução.

Com SDK compatível e runtime ASP.NET Core 8 instalado no sistema:

```powershell
cd C:\Projeto\Hackaton\Hackaton-backend
dotnet restore Hackaton.slnx
dotnet build Hackaton.slnx --no-restore
dotnet run --project HackatonApi/HackatonApi.csproj --launch-profile http
```

Nesta máquina os runtimes .NET e ASP.NET Core **8.0.31** foram instalados separadamente
em `$env:USERPROFILE\.dotnet-hackaton8`, pelo [script oficial da Microsoft](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script). O SDK 10 e o
PATH global não foram substituídos. Para executar com esse runtime local:

```powershell
cd C:\Projeto\Hackaton\Hackaton-backend
.\scripts\run-local.ps1
```

O script compila a API usando o SDK do sistema e executa o DLL com o host .NET 8 local,
definindo o contentRoot como HackatonApi para carregar corretamente appsettings.json.
Você pode informar `-RuntimeDirectory C:\outra\pasta`. Não há roll-forward de 8 para 10.
O perfil é HTTP local; em Production o redirecionamento HTTPS é padrão, exceto quando Http:RedirectToHttps=false (como no container atrás do HTTPS da hospedagem).

- Status: `http://localhost:5286/api/status`.
- Swagger: `http://localhost:5286/swagger` em Development.
- OpenAPI: `http://localhost:5286/swagger/v1/swagger.json` em Development.
- A raiz `/` não possui endpoint e retorna 404.

## Arquitetura e estrutura

```text
Hackaton.Domain/
  Entities/                 Entity e EntityAudited
  Interface/Markers/        IMustAudited
  Interface/                IRepository, IDomainService, IUnitOfWork, IUserResolver
  Services/                 DomainService<T>
  DomainContainer.cs        AddDomain
Hackaton.Application/
  DTOs/, Validators/, Mappings/     Reservados para o domínio do produto
  Interface/                IAppService<TEntity,TDto>
  Services/                 AppService<TEntity,TDto>
  Exceptions/               Validação, entidade ausente e credenciais inválidas
  DependencyInjection.cs    AddApplication, Mapster e validators por assembly
Hackaton.Infrastructure/
  Data/                     AppDbContext e CacheKeys
  Data/Map/, Data/Migrations/       Reservados para o domínio do produto
  Repositories/             GenericRepository<T>
  UnitOfWork/               Persistência, auditoria e transações
  DependencyInjection.cs    AddInfrastructure
HackatonApi/
  Controllers/              BaseController<TEntity,TDto>
  Auth/                     HttpContextUserResolver e autenticação desativada
  Middlewares/              GlobalExceptionHandler
  Swagger/                  Filtros de ReadOnly e autorização
  Program.cs                Composição e pipeline HTTP
Hackaton.Tests/             Application, persistência e integração HTTP
scripts/run-local.ps1       Execução usando runtime local
.config/dotnet-tools.json   dotnet-ef 8.0.2 local
```

Dependências: Application -> Domain; Infrastructure -> Domain; API -> as três camadas.
Tests referencia as quatro camadas para testar comportamento e HTTP. O projeto de
testes está incluído na solução. Domain não conhece EF Core, ASP.NET ou Application.

Fluxo: Controller -> AppService -> DomainService -> Repository; a UnitOfWork confirma
alterações. Controller não acessa DbContext ou Repository. Não fazer AppService chamar
outro AppService. A DI do Domain admite apenas o pacote de abstrações usado na origem.

## Entidades e contratos base

- `Entity` é abstrata: Id Guid com get/set público e Guid.NewGuid() como valor inicial.
- `EntityAudited` é abstrata e herda Entity/IMustAudited: CreationUserId/CreationTime
  e ModificationUserId/ModificationTime, os últimos dois anuláveis.
- `IMustAudited` declara os quatro campos, não é um marcador vazio.
- `DomainService`, `AppService` e `GenericRepository` são classes concretas com métodos
  virtuais; `BaseController` é abstrato e `UnitOfWork` é sealed.
- IRepository/IDomainService: GetAllAsync, GetByIdAsync, Add, Update e Remove.
- IAppService: GetAllAsync, GetAsync, CreateAsync, UpdateAsync e DeleteAsync.
- IUserResolver retorna o Guid do usuário atual; a implementação HTTP lê sub/NameIdentifier.
  Requisições anônimas ou Id inválido retornam Guid.Empty.

## Application: validação, mapping e casos de uso

AppService recebe IDomainService, IUnitOfWork, Mapster IMapper e validators do DTO.
Create valida e mapeia, gera novo Guid ignorando o Id do body, chama Begin/Add/Confirm.
Update valida, busca a entidade, mapeia sobre ela e restaura o Id da URL antes de salvar.
Delete busca, marca remoção e confirma. Leituras mapeiam entidades para DTOs.
CancellationToken é propagado nas operações assíncronas.

Sem validator registrado não há validação automática. Para cada DTO futuro, criar
`AbstractValidator<TDto>` e um mapping Mapster `IRegister`. DTOs seguem `sealed record`
de propriedades get/init, strings iniciadas e Id com `[ReadOnly(true)]`. Manter um DTO
por entidade CRUD como na origem e ignorar Id na direção DTO -> Entity.

AddApplication escaneia validators e mappings, registra configuração Mapster singleton
e IMapper/ServiceMapper scoped. AppServices específicos devem ser registrados scoped
quando forem criados; não há registro aberto genérico de IAppService.

## Persistência, auditoria e cache

GenericRepository recebe contexto e IMemoryCache. GetAll usa AsNoTracking e cache de
lista com expiração deslizante de 30 minutos; GetById usa FindAsync com rastreamento.
Add/Update/Remove só alteram o DbSet, sem SaveChanges. Não há paginação ou ordenação genérica.

UnitOfWork.Confirm coleta tipos alterados, aplica auditoria, salva e invalida cache.
Auditoria usa UTC e IUserResolver: na criação preenche campos de criação e limpa modificação;
na atualização preenche modificação e protege os campos de criação no banco.
Não há soft delete ou auditoria de exclusão. Não expor campos de auditoria como entrada
quando criar DTOs; ignorá-los no mapping de entrada se o DTO incluir esses campos.

Begin tem corpo vazio e não abre transação SQL. BeginTransaction abre transação explícita;
ConfirmTransaction salva/commita/invalida cache; RejectTransaction faz rollback.
Dispose/DisposeAsync descartam transações restantes. A pilha não garante transações
aninhadas; rollback não restaura automaticamente os valores dos objetos rastreados.

### SQLite local — configuração legada/opcional

A configuração vigente usa MariaDB/Pomelo conforme a seção inicial. SQLite 8.0.13 permanece disponível para testes e uso local explícito.
Não precisa instalar servidor de banco, configurar usuário ou senha.

Em HackatonApi/appsettings.json:

```json
{
  "Persistence": { "Enabled": true },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/hackaton.db"
  }
}
```

Program passa ContentRootPath da API para AddInfrastructure. Caminhos relativos
são resolvidos a partir de HackatonApi, e a pasta é criada quando o módulo é registrado.
O arquivo padrão é **HackatonApi/App_Data/hackaton.db**, tanto com dotnet run quanto
com scripts/run-local.ps1. Não depende do diretório atual do terminal.
O arquivo SQLite é criado ao abrir a primeira conexão, não apenas ao iniciar a API.

AddInfrastructure registra DbContext SQLite, cache, IRepository<> e IUnitOfWork;
IUserResolver já está registrado na API. Para mudar o arquivo nesta sessão:

```powershell
$env:Persistence__Provider = 'Sqlite'
$env:ConnectionStrings__DefaultConnection = 'Data Source=App_Data/outro-banco.db'
```

Para desabilitar persistência temporariamente: Persistence__Enabled=false.
appsettings.Local.example.json é apenas exemplo e não é carregado automaticamente.
Arquivos App_Data, *.db, *.db-wal, *.db-shm e *.db-journal estão no .gitignore.
Compartilhe código e migrations; não versione o banco local.

As migrations de negócio estão em `Hackaton.Infrastructure/Data/Migrations`. Não foi
colocado EnsureCreated no startup, evitando misturar criação direta de schema com
o fluxo de migrations. As migrations criam o schema e inserem dados demonstrativos
de São José dos Pinhais; revise esses registros antes de usar em produção.

```powershell
dotnet tool restore
dotnet ef database update --context AppDbContext --project Hackaton.Infrastructure --startup-project HackatonApi
```

Com o runtime apenas na pasta local, definir DOTNET_ROOT e DOTNET_ROOT_X64 conforme
a seção de testes antes de usar dotnet-ef. As migrations não são aplicadas automaticamente
no boot. O seed é demonstrativo; uma migration posterior inclui hashes de senhas de teste.
A execução completa dos seeds ainda requer a correção registrada no estado atual.

SQLite aceita um escritor de cada vez. Esta escolha simplifica o desenvolvimento
local; os limites de concorrência devem ser avaliados conforme o produto evoluir.

## HTTP, Swagger, erros e autenticação

BaseController usa `api/[controller]`: GET/POST/PUT/DELETE restritos a Gestor/Admin. POST e PUT retornam 200; DELETE retorna 204; item ausente retorna 404.
Endpoints não CRUD podem usar ControllerBase e AppService próprio, como na referência.
Controllers concretos de usuários, unidades, chamados, autenticação e portais já existem.
A base atual exige Gestor/Admin também para leituras CRUD; as controllers aplicam verificações adicionais por prefeitura.

Filtros Swagger marcam propriedades ReadOnly e criam schema de entrada sem elas.
Isso documenta os campos, sem substituir a defesa do AppService contra Id do cliente.
O filtro de autorização mostra requisito Bearer só nas operações protegidas.
Swagger fica disponível apenas em Development.

GlobalExceptionHandler produz ProblemDetails: validação 400, entidade ausente 404,
credenciais inválidas 401 e inesperada 500. Respostas incluem traceId. Em Development
erros inesperados incluem detalhes; em Production retornam mensagem genérica e ficam
no log. Essa proteção de detalhes é uma melhoria sobre o handler do portfólio.

Por padrão `Authentication:Enabled=false`, sem token aceito ou login fictício. Rotas
protegidas recebem 401. Para ativar validação de tokens emitidos por um sistema compatível:

```powershell
$env:Authentication__Enabled = 'true'
$env:JwtSettings__Issuer = 'seu-emissor'
$env:JwtSettings__Audience = 'seu-publico'
$env:JwtSettings__SigningKey = 'SEU_SEGREDO_ALEATORIO_COM_PELO_MENOS_32_BYTES'
```

Não usar o placeholder como chave real. Quando ativado, JWT exige issuer, audience e chave,
valida assinatura/validade com ClockSkew zero. Tokens de Admin devem conter role=Admin
e Id em sub. Login, usuários e emissão de token já existem, conforme a seção de APIs abaixo.
Senhas usam PasswordHasher do ASP.NET Core Identity; não há refresh token ou revogação/logout de JWT.

CORS permite `http://localhost:5173` por padrão. Ajustar AllowedOrigins em configuração
ou `AllowedOrigins__0` por ambiente. O pipeline aplica CORS antes de autenticação/autorização.
Não há AllowCredentials porque cookies não estão sendo usados.

## Pacotes e decisões da base

| Camada | Pacotes diretos |
|---|---|
| Domain | DI.Abstractions 8.0.2 |
| Application | FluentValidation/DI extensions 11.9.0; Mapster 7.4.0; Mapster.DependencyInjection 1.0.1; DI.Abstractions 8.0.2 |
| Infrastructure | EF Core/Design/SQLite 8.0.2; MemoryCache 8.0.1; Configuration.Abstractions 8.0.0; DI.Abstractions 8.0.2 |
| API | JwtBearer 8.0.2; Swashbuckle 6.6.2; EF Core Design 8.0.2 |
| Tests | xUnit/runner 2.5.3; Moq 4.20.72; FluentAssertions 6.12.1; Test.Sdk 17.8.0; coverlet 6.0.0; SQLite/Mvc.Testing 8.0.2 |

A geração EF Core 8 segue a referência. O provedor do produto foi alterado para SQLite
por decisão do projeto; os testes também usam SQLite. Não foram copiados conteúdo pessoal, migrations, seed ou segredos.
Não foram instaladas integrações de IA ou adicionadas funcionalidades de domínio.

## Verificação

```powershell
dotnet build Hackaton.slnx --no-restore
dotnet test Hackaton.Tests/Hackaton.Tests.csproj --no-build
```

Com runtime apenas na pasta local, antes dos testes nesta sessão:

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet-hackaton8"
$env:DOTNET_ROOT_X64 = $env:DOTNET_ROOT
dotnet test Hackaton.Tests/Hackaton.Tests.csproj --no-build
```

A suíte usa SQLite em memória e WebApplicationFactory. Modelos/controllers de teste
existem somente no assembly de testes; não são expostos pela API distribuída.
Verifica Id do servidor/URL, validação sem gravação, entidade ausente, auditoria,
preservação da criação, invalidação de cache, rollback, status/Swagger sem banco,
autorização de escrita, tradução de erros e ausência de detalhes em Production.

Os testes usam SQLite em memória e arquivo temporário. Não há emissão de token ou banco externo validado;
a integração com o frontend ainda não foi implementada. O estado consolidado acima registra as APIs e regras já entregues.

Verificações realizadas em 08/10/2026: restore e build aprovados (zero erros/avisos),
12 testes aprovados, dotnet-ef restaurado e execução pelo script local conferida.
GET /api/status e OpenAPI responderam 200; preflight CORS autorizou localhost:5173.
O servidor de verificação foi encerrado depois dos checks; use o script para iniciá-lo.

## APIs de autenticação e atendimento

Configure `Authentication:Enabled=true`, `JwtSettings:Issuer`, `JwtSettings:Audience` e
`JwtSettings:SigningKey` (pelo menos 32 bytes) antes de iniciar a API. Sem JWT habilitado,
os logins retornam 503 e rotas protegidas recusam acesso. Exemplo local:

```powershell
$env:Authentication__Enabled = 'true'
$env:JwtSettings__Issuer = 'hackaton-local'
$env:JwtSettings__Audience = 'hackaton-clients'
$env:JwtSettings__SigningKey = 'LOCAL_ONLY_CHANGE_THIS_KEY_32_BYTES_MINIMUM'
```

Login web: `POST /api/auth/web/login`; aceita `Usuario`, `Gestor` ou `Admin`. Login do app:
`POST /api/auth/app/login`; aceita somente perfil `Tecnico` com registro técnico ativo na
prefeitura informada. Ambos recebem `Email`, `Senha` e `PrefeituraId` e retornam JWT.
Gestores/administradores definem senha com `PUT /api/auth/usuarios/{id}/senha`.
Não há cadastro público, refresh token ou revogação de JWT nesta entrega.

Contas demonstrativas criadas pelas migrations usam a senha local `Teste@12345!`:
`admin@sjp.example.test`, `solicitante@sjp.example.test` e `tecnico@sjp.example.test`.
Informe o ID da prefeitura `00000000-0000-0000-0000-000000000001`. Essas contas e senha
são apenas para desenvolvimento; altere/remova antes de qualquer uso real.

APIs principais:

- Técnico: `GET /api/app/chamados/abertos|pendentes|em-andamento`, `GET /api/app/ranking`,
  `POST /api/app/posicoes`, e eventos/comentários em `/api/app/chamados/{id}/...`.
- Usuário: `GET /api/usuario/chamados` (filtros `status`, `prioridade`, `unidadeId`,
  `desde`, `ate`), `POST /api/usuario/chamados`, eventos e comentários por chamado.
- Gestor: `GET /api/gestor/chamados` (filtros `status`, `prioridade`, `area`, `unidadeId`,
  `tecnicoId`, `solicitanteId`, datas e paginação), `GET /api/gestor/tecnicos/posicoes/atuais`
  e `GET /api/gestor/tecnicos/{tecnicoId}/posicoes?limite=100`; também pode consultar anexos
  por `GET /api/gestor/chamados/{chamadoId}/anexos`.
- Gestão de técnicos (gestor/admin): `GET/POST /api/gestor/tecnicos`, `GET
  /api/gestor/tecnicos/{tecnicoId}` e `PUT /api/gestor/tecnicos/{tecnicoId}/unidades`.
  A criação cadastra o usuário com papel `Tecnico`, senha inicial e vínculos opcionais.
  Cada vínculo informa `Responsavel`; uma unidade pode ter no máximo um responsável principal.

Ao abrir um chamado, a API atribui automaticamente o responsável principal ativo e disponível
da unidade e registra evento de vinculação. Se a unidade não tiver responsável elegível,
o chamado permanece `Aberto` e sem técnico responsável. A migration
`20261010150005_TecnicoResponsavelUnidade` adiciona essa regra ao banco.

O técnico envia suas coordenadas com `POST /api/app/posicoes`, durante um chamado em andamento.
Payload: `{"latitude":-25.5,"longitude":-49.2,"precisaoMetros":12,"chamadoId":"<guid>"}`.
O horário é definido pelo servidor e as posições ficam disponíveis nas consultas do gestor.

Para registrar a conclusão técnica com comprovante, use
`POST /api/app/chamados/{chamadoId}/finalizar` com `FotoBase64`, `TipoConteudo` (`image/jpeg`,
`image/png` ou `image/webp`) e, opcionalmente, `Descricao`. A imagem pode ser Base64 puro ou
data URI, tem limite de 8 MB e é armazenada em Base64 na tabela `AnexosChamado`. A operação grava
o anexo e o evento na mesma transação e move a etapa para `Aguardando validação`; o status só
passa a `Resolvido` após confirmação do solicitante. Técnico responsável e solicitante podem
consultar anexos em `GET /api/app/chamados/{id}/anexos` e `GET /api/usuario/chamados/{id}/anexos`.
Gestores também podem consultar pelo endpoint acima. A migration `20261010170006_ConteudoBase64Anexo`
adiciona a coluna do conteúdo.

Chamados e demais dados retornados pelo gestor são limitados à prefeitura do JWT. O técnico
vê chamados abertos somente em unidades às quais está vinculado; pendentes e em andamento
precisam estar atribuídos a ele ou ter sua participação registrada. O ranking informa pontuação
do período e acumulada, patente, símbolo, próxima patente, pontos restantes, percentual e classes
Tailwind. A pontuação por chamado é calculada no backend pela complexidade (Baixo=5, Médio=10,
Alto=15); prioridade `Alta` acrescenta 5 pontos. Chamado aberto pelo usuário começa em `Baixo`,
o gestor pode definir a complexidade e `Pontos` não é aceito do cliente. Na conclusão técnica,
o valor calculado é creditado uma única vez como provisório, somado ao campo `Pontos` do técnico
e sua patente é atualizada. A migration `20261010190007_PontosEPatenteTecnico`
adiciona os campos de saldo/patente; `20261010200008_PontosChamado` adiciona pontos ao chamado.

## Acrescentar uma funcionalidade no hackathon

1. Definir entidade e necessidade de auditoria; herdar Entity ou EntityAudited.
2. Criar interfaces/serviços/repositórios específicos sobre as bases genéricas.
3. Criar DTO, validator, mapping e AppService; registrar os serviços específicos.
4. Criar Map EF e DbSet; gerar e revisar migration própria.
5. Criar controller sobre BaseController ou endpoint específico para operação não CRUD.
6. Validar autorização e estados de erro, implementar testes pertinentes e atualizar este README.

Manter regras no domínio e a coordenação na Application. Revisar acesso público/Admin
conforme os requisitos reais, sem remover proteção de escrita silenciosamente.

## Mudança de banco

SQLite substituiu MySQL por solicitação do projeto. A arquitetura DDD, interfaces,
serviços, auditoria e cache foram preservados. O guia DDD registra a origem MySQL;
para configuração atual, seguir este README e o código.

Verificação da troca para SQLite: restore/build aprovados, 12 testes aprovados,
abertura de arquivo SQLite temporário validada pela DI e API/status/Swagger conferidos
com persistência habilitada. As migrations de negócio adicionadas depois ainda precisam
ser aplicadas e verificadas no ambiente local.

## Docker e hospedagem

A imagem Linux usa build em múltiplas etapas: SDK .NET 8 compila/publica a API,
e ASP.NET Core Runtime 8 executa apenas os arquivos publicados. Não é necessário
instalar .NET no host com Docker. As imagens oficiais .NET 8 possuem o usuário app
sem privilégios; a aplicação roda com esse usuário e HTTP na porta 8080.
[Referência das imagens .NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8/containers).

Arquivos: Dockerfile, .dockerignore, compose.yaml e .env.example.
O contexto de build exclui bin/obj, testes, banco local e configurações locais/segredos.
A imagem não contém o banco de dados existente no computador.

### Rodar com Compose

Com Docker Engine ou Docker Desktop em modo Linux e Compose v2 instalado:

```powershell
cd C:\Projeto\Hackaton\Hackaton-backend
docker compose up --build -d
docker compose logs -f api
```

A API responde em http://localhost:5286/api/status. Compose usa Production por padrão;
o Swagger não está exposto nesse ambiente. Para desenvolvimento local com Swagger:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
```

O exemplo define Development, porta 5286 e origem do frontend localhost:5173.
Não use Development na hospedagem pública. Ajuste API_PORT e FRONTEND_ORIGIN no .env.
Essas variáveis são usadas pelo Compose; o .env não é carregado pelo ASP.NET nem copiado
para a imagem. Se a porta mudar, ajuste a URL usada para testar e configurar o frontend.

```powershell
docker compose config
docker compose down
```

down encerra/remove containers e rede, preservando o volume nomeado.
down -v também apaga o volume e o banco: não usar para reiniciar normalmente.

### Rodar apenas a imagem

```powershell
docker build -t hackaton-backend:local .
docker volume create hackaton-sqlite
docker run --name hackaton-api -p 5286:8080 --mount type=volume,source=hackaton-sqlite,target=/data hackaton-backend:local
```

A imagem usa Production por padrão. Acrescente -e ASPNETCORE_ENVIRONMENT=Development
apenas para testar Swagger localmente. Para parar: docker stop hackaton-api.

### Persistência SQLite e permissões

No container a connection string é Data Source=/data/hackaton.db.
O Compose monta sqlite-data em /data, preservando os dados entre recriações do container.
O arquivo nasce na primeira conexão; migrations não são aplicadas automaticamente no boot.
Não usar apenas o filesystem temporário do container para guardar dados importantes.

Um volume nomeado novo recebe as permissões do diretório /data preparado para app.
Se usar diretório do host ou disco persistente da hospedagem, garantir permissão de escrita
para UID 1654 (app), incluindo criação dos arquivos auxiliares de journal/WAL.
O volume local e o arquivo HackatonApi/App_Data/hackaton.db são bancos independentes.
Planejar backup do arquivo com a API parada ou usar ferramenta de backup SQLite consistente.

### Ajustes na hospedagem

- Usar Dockerfile na raiz e porta interna 8080, ou PORT fornecida pelo serviço.
- A API aceita PORT entre 1 e 65535 e escuta em 0.0.0.0 nessa porta.
- Montar disco persistente em /data ou ajustar ConnectionStrings__DefaultConnection.
- Configurar ASPNETCORE_ENVIRONMENT=Production e AllowedOrigins__0 com a URL do frontend.
- TLS/HTTPS deve ser terminado pelo proxy da hospedagem: a imagem define
  Http__RedirectToHttps=false para servir HTTP internamente sem redirecionamentos.
- Usar /api/status para checagem do processo; ele não verifica o arquivo SQLite ou schema.
- Se ativar JWT, fornecer Authentication__Enabled e JwtSettings__* por segredos do serviço,
  sem inserir chaves no Dockerfile, repositório ou argumentos de build.
- Usar uma única instância com acesso ao arquivo SQLite. Réplicas independentes com discos
  próprios não compartilham o mesmo banco; escolher estratégia de persistência antes de escalar.

O Docker não elimina a necessidade de volume persistente nem garante suporte em qualquer
hospedagem: o serviço deve aceitar containers Linux e oferecer armazenamento adequado.
Migrations futuras continuam sendo um passo separado; a imagem não executa dotnet-ef no boot.

### Verificação do Docker

Nesta máquina o comando Docker não está disponível. Os arquivos foram preparados,
mas o build e a execução da imagem precisam ser conferidos num host com Docker.
Validar docker compose config, docker compose up --build e GET /api/status nesse host.
Também conferir persistência após recriar o container quando houver dados de negócio.

Verificação desta configuração: publicação Release aprovada, 12 testes aprovados,
Compose validado como YAML e API publicada conferida em Production com PORT=5297
(status 200 e Swagger 404). Não foi executado docker build por ausência de Docker no host.

## Docker pelo botão do Visual Studio

HackatonApi possui perfil Docker em Properties/launchSettings.json e a dependência
Microsoft.VisualStudio.Azure.Containers.Tools.Targets 1.23.0, como na referência.
O projeto aponta para o Dockerfile da raiz e seu contexto de build. Usa modo Regular
para executar o Dockerfile completo, incluindo a preparação das permissões de /data.
[Configuração oficial das Container Tools](https://learn.microsoft.com/en-us/visualstudio/containers/container-msbuild-properties?view=vs-2022).

1. Abrir Docker Desktop e aguardar o engine ativo em modo Linux.
2. No Visual Studio, abrir/recarregar Hackaton.slnx e definir HackatonApi como projeto de inicialização.
3. Selecionar Docker no menu ao lado da seta onde aparece http.
4. Executar com F5 ou Ctrl+F5; a primeira execução baixa imagens e compila o container.
5. Abrir http://localhost:5287/swagger ou http://localhost:5287/api/status.

O perfil usa Development apenas para depuração local, HTTP e volume hackaton-vs-sqlite.
Esse volume é diferente do usado pelo Compose: os dois modos não compartilham dados.
A porta 5287 evita conflito com a execução local/Compose em 5286.
O perfil http permanece disponível para executar sem Docker.

Se Docker não aparecer, recarregar a solução e conferir as Container Tools no Visual Studio Installer
e o suporte a desenvolvimento ASP.NET/web. Se ocorrer erro, consultar a janela Saída
na categoria Container Tools/Docker e copiar a mensagem para diagnóstico.
A execução pela interface do Visual Studio ainda precisa ser validada no ambiente com Docker ativo.


## Status do chamado — 10/10/2026

Implementação, validações e pendência de aplicação da migration estão consolidadas em
Estado atual no início deste README. O escopo permanece somente backend. As alterações
de status feitas inicialmente no frontend foram desfeitas a pedido do usuário; a adaptação
do frontend será realizada durante a integração.

Enum interno: Aberto = 0, EmAnalise = 1, Resolvido = 2. A API apresenta os nomes
Aberto, Em Análise e Resolvido; a coluna SQLite usa os nomes internos do enum.
EtapaAtendimento preserva vínculo, execução, pausa, bloqueio e espera de validação/análise.
Concluir deve manter Em Análise e confirmar deve marcar Resolvido quando esses comandos
forem implementados. A simplificação de status não implementa essas transições por si só.

## Integração do login do usuário no frontend — 10/10/2026

- [x] Frontend consome POST /api/auth/web/login com Email, Senha e PrefeituraId; esta etapa aceita somente Usuario.
- [x] Sessão por aba, Bearer no Axios, logout, expiração/401, rotas protegidas e perfil com identidade real.
- [x] Erros de login/indisponibilidade tratados; contas demonstrativas disponíveis somente no modo demo.
- [x] Teste HTTP UserLoginTests valida emissão e uso efetivo de JWT, isolamento por prefeitura, restrição administrativa, senha incorreta, prefeitura incorreta, conta inativa, perfil técnico no login web e token inválido. Usa SQLite temporário/EnsureCreated, sem alterar banco operacional.
- [ ] Habilitar Authentication__Enabled e configurar JwtSettings__Issuer, Audience e SigningKey no ambiente de execução; JWT permanece desabilitado por padrão.
- [ ] Resolver/aplicar a cadeia de migrations no banco operacional e provisionar conta Usuario ativa com PasswordHash válido. Não foi criado banco operacional nesta etapa.
- [ ] Integrar listagem, abertura e detalhes dos chamados do usuário. A conta real vê aviso dessa próxima etapa; a abertura está desabilitada e os dados mockados não são apresentados.
- [ ] Integrar login e funções reais de técnico, Gestor e Admin em etapas posteriores.
- [ ] Validar interface em navegador e celular.

O frontend usa VITE_AUTH_MODE=api, VITE_API_URL=http://localhost:5286 e VITE_PREFEITURA_ID por instalação. O ID demonstrativo do seed é 00000000-0000-0000-0000-000000000001. Não expor SigningKey no frontend. Documentação de configuração e comportamento atualizada no README e no plano do frontend. O escopo somente backend descrito na entrega de status permanece histórico; esta etapa foi autorizada para integrar o login no frontend.
