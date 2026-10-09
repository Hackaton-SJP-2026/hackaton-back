namespace Hackaton.Domain.Interface
{
    public interface IUnitOfWork
    {
        void Begin();
        Task<int> Confirm(CancellationToken cancellationToken = default);

        Task BeginTransaction(CancellationToken cancellationToken = default);
        Task ConfirmTransaction(CancellationToken cancellationToken = default);
        Task RejectTransaction(CancellationToken cancellationToken = default);
    }
}
