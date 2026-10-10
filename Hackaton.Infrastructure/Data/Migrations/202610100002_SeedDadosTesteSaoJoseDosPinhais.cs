using Hackaton.Infrastructure.Data;
using Hackaton.Infrastructure.Data.Map;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Hackaton.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202610100002_SeedDadosTesteSaoJoseDosPinhais")]
public sealed class SeedDadosTesteSaoJoseDosPinhais : Migration
{
    private static readonly Guid PrefeituraId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid SolicitanteId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid TecnicoUsuarioId = Guid.Parse("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid TecnicoId = Guid.Parse("dddddddd-0000-0000-0000-000000000001");
    private static readonly Guid UnidadeId = Guid.Parse("eeeeeeee-0000-0000-0000-000000000001");
    private static readonly Guid ChamadoAbertoId = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid ChamadoValidacaoId = Guid.Parse("22222222-0000-0000-0000-000000000002");
    private static readonly DateTime SeedTime = new(2026, 10, 10, 12, 0, 0, DateTimeKind.Utc);

    protected override void Up(MigrationBuilder m)
    {
        m.Sql($"UPDATE Prefeituras SET Nome = 'Prefeitura Municipal de São José dos Pinhais', Cidade = 'São José dos Pinhais', Uf = 'PR', Ativa = 1 WHERE Id = '{PrefeituraId}'");

        m.InsertData("Usuarios", new[] { "Id", "Nome", "Email", "Telefone", "Papel", "Ativo", "PrefeituraId", "CreationUserId", "CreationTime" }, new object[]
        { AdminId, "Administrador de Teste", "admin@sjp.example.test", "41999990001", "Admin", true, PrefeituraId, Guid.Empty, SeedTime });
        m.InsertData("Usuarios", new[] { "Id", "Nome", "Email", "Telefone", "Papel", "Ativo", "PrefeituraId", "CreationUserId", "CreationTime" }, new object[]
        { SolicitanteId, "Solicitante de Teste", "solicitante@sjp.example.test", "41999990002", "Usuario", true, PrefeituraId, Guid.Empty, SeedTime });
        m.InsertData("Usuarios", new[] { "Id", "Nome", "Email", "Telefone", "Papel", "Ativo", "PrefeituraId", "CreationUserId", "CreationTime" }, new object[]
        { TecnicoUsuarioId, "Técnico de Teste", "tecnico@sjp.example.test", "41999990003", "Tecnico", true, PrefeituraId, Guid.Empty, SeedTime });

        m.InsertData("UnidadesAtendimento", new[] { "Id", "Nome", "Endereco", "Latitude", "Longitude", "Ativa", "PrefeituraId", "CreationUserId", "CreationTime" }, new object[]
        { UnidadeId, "Unidade Central - Teste", "Rua Passos de Oliveira, Centro, São José dos Pinhais - PR", -25.535, -49.203, true, PrefeituraId, AdminId, SeedTime });

        m.InsertData("Tecnicos", new[] { "Id", "UsuarioId", "FotoUrl", "Disponibilidade", "DisponibilidadeAtualizadaEm", "CreationUserId", "CreationTime" }, new object[]
        { TecnicoId, TecnicoUsuarioId, "https://example.test/images/tecnico-teste.png", "Disponivel", SeedTime, AdminId, SeedTime });
        m.InsertData("TecnicoUnidadeAtendimento", new[] { "Id", "TecnicoId", "UnidadeAtendimentoId", "Ativo", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("33333333-0000-0000-0000-000000000003"), TecnicoId, UnidadeId, true, AdminId, SeedTime });

        InsertTicket(m, ChamadoAbertoId, "Chamado de teste vinculado à Unidade Central", "Vinculado", TecnicoId, null, SeedTime);
        InsertTicket(m, ChamadoValidacaoId, "Chamado de teste aguardando validação da entrega", "Aguardando validação", TecnicoId, SeedTime.AddHours(-2).AddMinutes(10), SeedTime.AddHours(-3));

        InsertEvent(m, "44444444-0000-0000-0000-000000000004", ChamadoAbertoId, null, "Abertura", "Chamado aberto no sistema.", SeedTime);
        InsertEvent(m, "44444444-0000-0000-0000-000000000005", ChamadoAbertoId, AdminId, "Vinculo", "Chamado vinculado ao técnico responsável pela unidade.", SeedTime.AddMinutes(1));
        InsertEvent(m, "44444444-0000-0000-0000-000000000006", ChamadoValidacaoId, null, "Abertura", "Chamado aberto no sistema.", SeedTime.AddHours(-3));
        InsertEvent(m, "44444444-0000-0000-0000-000000000007", ChamadoValidacaoId, AdminId, "Vinculo", "Chamado atribuído ao técnico de teste.", SeedTime.AddHours(-2).AddMinutes(5));
        InsertEvent(m, "44444444-0000-0000-0000-000000000008", ChamadoValidacaoId, TecnicoUsuarioId, "Inicio", "Técnico iniciou o atendimento.", SeedTime.AddHours(-2).AddMinutes(10));
        InsertEvent(m, "44444444-0000-0000-0000-000000000010", ChamadoValidacaoId, TecnicoUsuarioId, "Conclusao", "Equipamento revisado; chamado concluído e aguardando validação do solicitante.", SeedTime.AddHours(-1));

        m.InsertData("ComentariosChamado", new[] { "Id", "ChamadoId", "AutorUsuarioId", "Texto", "DataHora", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("55555555-0000-0000-0000-000000000005"), ChamadoValidacaoId, SolicitanteId, "Consigo acessar o sistema novamente, obrigado.", SeedTime.AddMinutes(-50), SolicitanteId, SeedTime.AddMinutes(-50) });
        m.InsertData("ComentariosChamado", new[] { "Id", "ChamadoId", "AutorUsuarioId", "Texto", "DataHora", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("55555555-0000-0000-0000-000000000006"), ChamadoAbertoId, TecnicoUsuarioId, "Estou verificando a conexão da unidade.", SeedTime.AddMinutes(3), TecnicoUsuarioId, SeedTime.AddMinutes(3) });

        m.InsertData("ParticipantesChamado", new[] { "Id", "ChamadoId", "TecnicoId", "Papel", "DataVinculo", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("66666666-0000-0000-0000-000000000006"), ChamadoAbertoId, TecnicoId, "Responsavel", SeedTime.AddMinutes(1), AdminId, SeedTime.AddMinutes(1) });
        m.InsertData("ParticipantesChamado", new[] { "Id", "ChamadoId", "TecnicoId", "Papel", "DataVinculo", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("66666666-0000-0000-0000-000000000007"), ChamadoValidacaoId, TecnicoId, "Responsavel", SeedTime.AddHours(-2).AddMinutes(5), AdminId, SeedTime.AddHours(-2).AddMinutes(5) });

        m.InsertData("PosicoesTecnico", new[] { "Id", "TecnicoId", "ChamadoId", "Latitude", "Longitude", "PrecisaoMetros", "DataHora" }, new object[]
        { Guid.Parse("77777777-0000-0000-0000-000000000007"), TecnicoId, ChamadoAbertoId, -25.5351, -49.2032, 18.0, SeedTime.AddMinutes(2) });

        m.InsertData("NotificacoesPush", new[] { "Id", "UsuarioDestinatarioId", "ChamadoId", "Titulo", "Mensagem", "Status", "Tentativas", "CriadaEm", "ChaveDeduplicacao" }, new object[]
        { Guid.Parse("88888888-0000-0000-0000-000000000008"), TecnicoUsuarioId, ChamadoAbertoId, "Novo chamado", "Chamado de teste da Unidade Central.", "Pendente", 0, SeedTime.AddMinutes(1), "seed:evento-abertura:tecnico-teste" });
        m.InsertData("NotificacoesPush", new[] { "Id", "UsuarioDestinatarioId", "ChamadoId", "Titulo", "Mensagem", "Status", "Tentativas", "CriadaEm", "EnviadaEm", "ChaveDeduplicacao" }, new object[]
        { Guid.Parse("88888888-0000-0000-0000-000000000009"), SolicitanteId, ChamadoValidacaoId, "Entrega aguardando validação", "O técnico concluiu o atendimento. Confira a foto de entrega.", "Enviada", 1, SeedTime.AddHours(-1), SeedTime.AddHours(-1).AddSeconds(5), "seed:evento-conclusao:solicitante-teste" });

        m.InsertData("DispositivosPush", new[] { "Id", "UsuarioId", "Endpoint", "ChavePublica", "ChaveAutenticacao", "Ativo", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("99999999-0000-0000-0000-000000000009"), TecnicoUsuarioId, "https://push.example.test/subscriptions/seed-device", "FAKE_TEST_PUBLIC_KEY", "FAKE_TEST_AUTH_KEY", true, AdminId, SeedTime });

        m.InsertData("AnexosChamado", new[] { "Id", "ChamadoId", "AutorUsuarioId", "NomeArquivo", "TipoConteudo", "TamanhoBytes", "Localizacao", "TipoAnexo", "DataHora", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse("aaaaaaaa-1111-0000-0000-000000000001"), ChamadoValidacaoId, TecnicoUsuarioId, "foto-entrega-teste.jpg", "image/jpeg", 123456L, "seed://anexos/foto-entrega-teste.jpg", "FotoEntrega", SeedTime.AddHours(-1), TecnicoUsuarioId, SeedTime.AddHours(-1) });
    }

    private static void InsertTicket(MigrationBuilder m, Guid id, string description, string status, Guid? tecnicoId, DateTime? startedAt, DateTime createdAt)
        => m.InsertData("Chamados", new[] { "Id", "Descricao", "SolicitanteId", "UnidadeAtendimentoId", "AreaAtendimento", "Natureza", "Modalidade", "Prioridade", "Status", "PessoasAfetadas", "ExisteAlternativa", "NecessitaMaterial", "TecnicoResponsavelId", "InicioAtendimento", "CreationUserId", "CreationTime" }, new object[]
        { id, description, SolicitanteId, UnidadeId, "TI / Sistemas", "Software", "Presencial", "Normal", status, 3, false, false, tecnicoId!, startedAt!, SolicitanteId, createdAt });

    private static void InsertEvent(MigrationBuilder m, string id, Guid chamadoId, Guid? authorId, string type, string description, DateTime when)
        => m.InsertData("EventosChamado", new[] { "Id", "ChamadoId", "AutorUsuarioId", "Tipo", "Descricao", "DataHora", "CreationUserId", "CreationTime" }, new object[]
        { Guid.Parse(id), chamadoId, authorId!, type, description, when, authorId ?? Guid.Empty, when });

    protected override void Down(MigrationBuilder m)
    {
        m.DeleteData("AnexosChamado", "Id", Guid.Parse("aaaaaaaa-1111-0000-0000-000000000001"));
        m.DeleteData("DispositivosPush", "Id", Guid.Parse("99999999-0000-0000-0000-000000000009"));
        m.DeleteData("NotificacoesPush", "Id", Guid.Parse("88888888-0000-0000-0000-000000000008"));
        m.DeleteData("NotificacoesPush", "Id", Guid.Parse("88888888-0000-0000-0000-000000000009"));
        m.DeleteData("PosicoesTecnico", "Id", Guid.Parse("77777777-0000-0000-0000-000000000007"));
        m.DeleteData("ParticipantesChamado", "Id", Guid.Parse("66666666-0000-0000-0000-000000000006"));
        m.DeleteData("ParticipantesChamado", "Id", Guid.Parse("66666666-0000-0000-0000-000000000007"));
        m.DeleteData("ComentariosChamado", "Id", Guid.Parse("55555555-0000-0000-0000-000000000005"));
        m.DeleteData("ComentariosChamado", "Id", Guid.Parse("55555555-0000-0000-0000-000000000006"));
        foreach (var id in new[] { "44444444-0000-0000-0000-000000000004", "44444444-0000-0000-0000-000000000005", "44444444-0000-0000-0000-000000000006", "44444444-0000-0000-0000-000000000007", "44444444-0000-0000-0000-000000000008", "44444444-0000-0000-0000-000000000010" })
            m.DeleteData("EventosChamado", "Id", Guid.Parse(id));
        m.DeleteData("Chamados", "Id", ChamadoAbertoId);
        m.DeleteData("Chamados", "Id", ChamadoValidacaoId);
        m.DeleteData("TecnicoUnidadeAtendimento", "Id", Guid.Parse("33333333-0000-0000-0000-000000000003"));
        m.DeleteData("Tecnicos", "Id", TecnicoId);
        m.DeleteData("UnidadesAtendimento", "Id", UnidadeId);
        m.DeleteData("Usuarios", "Id", TecnicoUsuarioId);
        m.DeleteData("Usuarios", "Id", SolicitanteId);
        m.DeleteData("Usuarios", "Id", AdminId);
        m.Sql($"UPDATE Prefeituras SET Nome = 'Prefeitura migrada - revisar cadastro', Cidade = 'Não informado', Uf = 'NA' WHERE Id = '{PrefeituraId}'");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.2");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly,
            type => type != typeof(LancamentoPontoTecnicoMap));
        modelBuilder.Entity<Hackaton.Domain.Entities.Usuario>().Ignore(x => x.PasswordHash);
    }
}
