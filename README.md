# Hackaton Backend

Esqueleto C#/.NET 8 baseado em `C:\Projeto\portifolio-back\portfolio-back` e no
[guia DDD](GUIA_BACKEND_DDD_HACKATON.md). Mantém as responsabilidades da Application,
entidades base e serviços genéricos da referência. Não usa CQRS ou MediatR.

## Estado atual

As bases das cinco camadas estão implementadas. A API usa SQLite local por padrão, disponibiliza
Swagger em Development e oferece `GET /api/status`. Ainda não existem entidades de
negócio, CRUDs concretos, login, emissão de tokens, seed ou migrations do produto.

SQLite está habilitado por padrão; validação JWT é opcional e ativada por configuração.
Com autenticação desativada, ações protegidas retornam 401; nenhuma identidade fictícia
é criada. O status da API indica apenas que o processo atende HTTP, não saúde do SQLite.

## Requisitos e execução

Projetos com alvo `net8.0`, nullable e implicit usings habilitados. A solução `Hackaton.slnx`
foi preparada com SDK 10.0.401; use um SDK que suporte esse formato. Em ambientes que
precisem apenas do SDK 8, converta para `.sln` antes de executar comandos da solução.

Com SDK compatível e runtime ASP.NET Core 8 instalado no sistema:

```powershell
cd C:\Projeto\Hackaton-backend
dotnet restore Hackaton.slnx
dotnet build Hackaton.slnx --no-restore
dotnet run --project HackatonApi/HackatonApi.csproj --launch-profile http
```

Nesta máquina os runtimes .NET e ASP.NET Core **8.0.31** foram instalados separadamente
em `$env:USERPROFILE\.dotnet-hackaton8`, pelo [script oficial da Microsoft](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script). O SDK 10 e o
PATH global não foram substituídos. Para executar com esse runtime local:

```powershell
cd C:\Projeto\Hackaton-backend
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

### SQLite local — configuração padrão

O projeto usa Microsoft.EntityFrameworkCore.Sqlite **8.0.2**. Pomelo/MySQL foi removido.
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
$env:ConnectionStrings__DefaultConnection = 'Data Source=App_Data/outro-banco.db'
```

Para desabilitar persistência temporariamente: Persistence__Enabled=false.
appsettings.Local.example.json é apenas exemplo e não é carregado automaticamente.
Arquivos App_Data, *.db, *.db-wal, *.db-shm e *.db-journal estão no .gitignore.
Compartilhe código e migrations; não versione o banco local.

Não há tabelas de negócio ou migrations ainda. Ao definir entidades, criar maps EF,
DbSets e migrations. Não foi colocado EnsureCreated no startup, evitando misturar
criação direta de schema com o fluxo de migrations.

```powershell
dotnet tool restore
dotnet ef migrations add Nome --project Hackaton.Infrastructure --startup-project HackatonApi
dotnet ef database update --project Hackaton.Infrastructure --startup-project HackatonApi
```

Com o runtime apenas na pasta local, definir DOTNET_ROOT e DOTNET_ROOT_X64 conforme
a seção de testes antes de usar dotnet-ef. Não há criação de seed ou migração automática
no boot. Criar tabelas somente quando o modelo de negócio existir.

SQLite aceita um escritor de cada vez. Esta escolha simplifica o desenvolvimento
local; os limites de concorrência devem ser avaliados conforme o produto evoluir.

## HTTP, Swagger, erros e autenticação

BaseController usa `api/[controller]`: GET coleção/Guid públicos, POST/PUT/DELETE
restritos a Admin. POST e PUT retornam 200; DELETE retorna 204; item ausente retorna 404.
Endpoints não CRUD podem usar ControllerBase e AppService próprio, como na referência.
Não existem controllers concretos de negócio nesta etapa.

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
e Id em sub. Ainda não existem geração de token, usuários, BCrypt, refresh ou logout.
Essas funcionalidades dependem do domínio que será anunciado no hackathon.

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
a integração com o frontend ainda não foi implementada. A base está pronta para receber as regras do produto.

Verificações realizadas em 08/10/2026: restore e build aprovados (zero erros/avisos),
12 testes aprovados, dotnet-ef restaurado e execução pelo script local conferida.
GET /api/status e OpenAPI responderam 200; preflight CORS autorizou localhost:5173.
O servidor de verificação foi encerrado depois dos checks; use o script para iniciá-lo.

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
com persistência habilitada. Ainda não há tabelas de negócio no arquivo do produto.

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
cd C:\Projeto\Hackaton-backend
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
O arquivo nasce na primeira conexão; ainda não há tabelas de negócio ou auto-migrations.
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
