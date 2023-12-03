using ToklenAPI.Models;
using ToklenAPI.Models.Session;

namespace Domain.Interfaces
{
    public interface IUnitOfWork :IDisposable
    {
        IRepository<User> BasicUsers { get; }
        IRepository<RefreshToken> RefreshTokens { get; }
        bool SaveChanges();
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken);
        public Task BeginTransactionAsync();
        public Task CommitTransactionAsync(CancellationToken cancellationToken);
        public void RollbackTransaction();
        public Task<T> ExecuteWithinTransactionAsync<T>(Func<Task<T>> action);
    }
}
