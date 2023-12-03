using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ToklenAPI.Data;
using ToklenAPI.Models;
using ToklenAPI.Models.Session;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(ApplicationDbContext context, IDbContextTransaction currentTransaction, IRepository<User> basicUsers, IRepository<RefreshToken> refreshTokens)
        {
            _context = context;
            _currentTransaction = currentTransaction;
            BasicUsers = basicUsers;
            RefreshTokens = refreshTokens;
        }

        public IRepository<User> BasicUsers { get; }
        public IRepository<RefreshToken> RefreshTokens { get; }

        public void Dispose()
        {
            _context.Dispose();
        }

        public bool SaveChanges()
        {
            try
            {
                var saved = _context.SaveChanges();

                return saved > 0;
            }
            catch (Exception ex)
            {

                throw new InfrastructureException(ex.Message);
            }
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken)
        {

            try
            {
                var saved = await _context.SaveChangesAsync(cancellationToken);

                return saved > 0;
            }
            catch (Exception ex)
            {
                throw new InfrastructureException(ex.Message);
            }
        }
        public async Task BeginTransactionAsync()
        {
            _currentTransaction ??= await _context.Database.BeginTransactionAsync();
        }
        public async Task CommitTransactionAsync(CancellationToken cancellationToken)
        {
            await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                try
                {
                    await SaveChangesAsync(cancellationToken);
                    _currentTransaction?.Commit();
                }
                finally
                {
                    if (_currentTransaction != null)
                    {
                        _currentTransaction.Dispose();
                        _currentTransaction = null;
                    }
                }
            });
        }
        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        public async Task<T> ExecuteWithinTransactionAsync<T>(Func<Task<T>> action)
        {
            return await _context.Database.CreateExecutionStrategy().ExecuteAsync(action);
        }
    }
}
