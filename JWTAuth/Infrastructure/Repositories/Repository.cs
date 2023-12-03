using Domain.Exceptions;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ToklenAPI.Data;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly string DefaultDataBaseConnectionError = "A connectivity connection error has ocurred between Client and Database";
        protected readonly ApplicationDbContext Context;
        protected readonly ILogger<Repository<T>> Logger;
        protected readonly DbSet<T> Entities;

        public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
        {
            Context = context;
            Logger = logger;
            Entities = context.Set<T>();
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken)
        {
            await Entities.AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
        {
            await Entities.AddRangeAsync(entities, cancellationToken);
        }

        public async Task<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            try
            {
                return await Entities.AllAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);

                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            try
            {
                return await Entities.AnyAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);

                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }
        }

        public async Task<T?> FindAsync(CancellationToken cancellationToken, params object[] id)
        {
            try
            {
                return await Entities.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {

                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);

                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken, bool? asNoTracking = null)
        {
            try
            {
                return asNoTracking is null or false
                  ? await Entities.FirstOrDefaultAsync(predicate, cancellationToken)
                  : await Entities.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);


                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }

        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken, bool? asNoTracking = null)
        {
            try
            {
                return asNoTracking is null or false
                    ? await Entities.ToListAsync(cancellationToken)
                    : await Entities.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);

                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }
        }

        public void Remove(T entity)
        {
            Entities.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            Entities.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            Entities.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            Entities.UpdateRange(entities);
        }

        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken, bool? asNoTracking = null)
        {
            try
            {
                return asNoTracking is null or false
                    ? await Entities.Where(predicate).ToListAsync(cancellationToken)
                    : await Entities.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An exception has ocurred {Message}.", ex.Message);

                throw new InfrastructureException(DefaultDataBaseConnectionError);
            }
        }
    }
}
