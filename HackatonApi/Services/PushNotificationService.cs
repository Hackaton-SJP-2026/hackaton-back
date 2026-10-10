using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Hackaton.Application.Interface;
using Hackaton.Domain.Entities;
using Hackaton.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace HackatonApi.Services;

public sealed class PushNotificationOptions
{
    public bool Enabled { get; set; }
    public string Subject { get; set; } = "mailto:suporte@local.test";
    public string PublicKey { get; set; } = string.Empty;
    public string PrivateKey { get; set; } = string.Empty;
}

public sealed class PushNotificationService : IPushNotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly PushNotificationOptions _options;

    public PushNotificationService(
        AppDbContext db,
        IOptions<PushNotificationOptions> options,
        ILogger<PushNotificationService> logger)
    {
        _db = db;
        _logger = logger;
        _options = options.Value;
    }

    public async Task RegisterDeviceAsync(Guid usuarioId, string endpoint, string chavePublica, string chaveAutenticacao, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário obrigatório.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint do dispositivo obrigatório.", nameof(endpoint));

        if (string.IsNullOrWhiteSpace(chavePublica) || string.IsNullOrWhiteSpace(chaveAutenticacao))
            throw new ArgumentException("Credenciais do push são obrigatórias.", nameof(chavePublica));

        var usuarioExiste = await _db.Usuarios.AnyAsync(x => x.Id == usuarioId && x.Ativo, cancellationToken);
        if (!usuarioExiste)
            throw new InvalidOperationException("Usuário não encontrado ou inativo.");

        var dispositivo = await _db.DispositivosPush
            .FirstOrDefaultAsync(x => x.Endpoint == endpoint, cancellationToken);

        if (dispositivo is null)
        {
            _db.DispositivosPush.Add(new DispositivoPush
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Endpoint = endpoint,
                ChavePublica = chavePublica,
                ChaveAutenticacao = chaveAutenticacao,
                Ativo = true,
                CreationUserId = usuarioId,
                CreationTime = DateTime.UtcNow
            });
        }
        else
        {
            dispositivo.UsuarioId = usuarioId;
            dispositivo.ChavePublica = chavePublica;
            dispositivo.ChaveAutenticacao = chaveAutenticacao;
            dispositivo.Ativo = true;
            dispositivo.UltimoUsoEm = DateTime.UtcNow;
            dispositivo.ModificationUserId = usuarioId;
            dispositivo.ModificationTime = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnqueueNotificationAsync(Guid usuarioId, Guid? chamadoId, string titulo, string mensagem, CancellationToken cancellationToken = default)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Usuário obrigatório.", nameof(usuarioId));

        if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(mensagem))
            throw new ArgumentException("Título e mensagem são obrigatórios.", nameof(mensagem));

        var deduplicacao = Convert.ToHexString(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{usuarioId}:{chamadoId}:{titulo}:{mensagem}:{DateTime.UtcNow:O}"))).ToLowerInvariant();

        var jaExiste = await _db.NotificacoesPush
            .AnyAsync(x => x.UsuarioDestinatarioId == usuarioId && x.ChamadoId == chamadoId && x.Titulo == titulo && x.Mensagem == mensagem && x.Status == "Pendente", cancellationToken);

        if (jaExiste)
            return;

        _db.NotificacoesPush.Add(new NotificacaoPush
        {
            Id = Guid.NewGuid(),
            UsuarioDestinatarioId = usuarioId,
            ChamadoId = chamadoId,
            Titulo = titulo,
            Mensagem = mensagem,
            Status = "Pendente",
            Tentativas = 0,
            CriadaEm = DateTime.UtcNow,
            ChaveDeduplicacao = deduplicacao
        });

    }

    public async Task ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        if (string.IsNullOrWhiteSpace(_options.PublicKey) || string.IsNullOrWhiteSpace(_options.PrivateKey))
        {
            _logger.LogWarning("Web Push está habilitado, mas faltam PublicKey ou PrivateKey em PushNotification");
            return;
        }

        var pendentes = await _db.NotificacoesPush
            .Where(x => x.Status == "Pendente" || x.Status == "Retry")
            .OrderBy(x => x.CriadaEm)
            .Take(50)
            .ToListAsync(cancellationToken);

        foreach (var notificacao in pendentes)
        {
            var dispositivos = await _db.DispositivosPush
                .Where(x => x.UsuarioId == notificacao.UsuarioDestinatarioId && x.Ativo)
                .ToListAsync(cancellationToken);

            if (dispositivos.Count == 0)
            {
                notificacao.Status = "Enviada";
                notificacao.EnviadaEm = DateTime.UtcNow;
                notificacao.ErroEnvio = "Nenhum dispositivo ativo para o usuário.";
                continue;
            }

            var enviado = false;
            foreach (var dispositivo in dispositivos)
            {
                try
                {
                    await SendAsync(dispositivo, notificacao.Titulo, notificacao.Mensagem, cancellationToken);
                    enviado = true;
                }
                catch (Exception ex)
                {
                    notificacao.Tentativas += 1;
                    notificacao.ErroEnvio = ex.Message;
                    notificacao.Status = "Retry";
                    notificacao.ProximaTentativaEm = DateTime.UtcNow.AddMinutes(5);
                    _logger.LogError(ex, "Falha ao enviar push para usuário {UsuarioId} e endpoint {Endpoint}", notificacao.UsuarioDestinatarioId, dispositivo.Endpoint);
                }
            }

            if (enviado)
            {
                notificacao.Status = "Enviada";
                notificacao.EnviadaEm = DateTime.UtcNow;
                notificacao.ErroEnvio = null;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SendAsync(DispositivoPush dispositivo, string titulo, string mensagem, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new
        {
            title = titulo,
            body = mensagem,
            icon = "/logo.png",
            badge = "/logo.png",
            tag = "chamado",
            data = new { tipo = "chamado" }
        });

        var vapidDetails = new VapidDetails(
            _options.Subject,
            _options.PublicKey,
            _options.PrivateKey);

        var subscription = new PushSubscription(dispositivo.Endpoint, dispositivo.ChavePublica, dispositivo.ChaveAutenticacao);
        var client = new WebPushClient();

        await client.SendNotificationAsync(subscription, payload, vapidDetails, cancellationToken);
    }
}
