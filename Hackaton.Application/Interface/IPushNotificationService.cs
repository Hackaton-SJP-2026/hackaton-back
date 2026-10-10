namespace Hackaton.Application.Interface;

public interface IPushNotificationService
{
    Task RegisterDeviceAsync(Guid usuarioId, string endpoint, string chavePublica, string chaveAutenticacao, CancellationToken cancellationToken = default);
    Task EnqueueNotificationAsync(Guid usuarioId, Guid? chamadoId, string titulo, string mensagem, CancellationToken cancellationToken = default);
    Task ProcessPendingAsync(CancellationToken cancellationToken = default);
}
