using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace SliceR.Operations.EntityFrameworkCore;
internal class EntityDbContextResolver(
    IServiceProvider _serviceProvider,
    IEntityDbContextResolverCache _cache
    ) : IEntityDbContextResolver, IDbContextStartupFactory
{

    public IEnumerable<IEntityType> GetEntityTypes(IEnumerable<Type> dbContextTypes)
    {
        var contexts = dbContextTypes.Select(Resolve);
        var allEntityTypes = new List<IEntityType>();

        foreach (var context in contexts)
        {
            var contextType = context.GetType();
            var contextEntityTypes = context.Model.GetEntityTypes();
            foreach (var contextEntityType in contextEntityTypes)
            {
                allEntityTypes.Add(contextEntityType);
                _cache.SetDbContext(contextEntityType.ClrType, contextType);
            }
        }

        return allEntityTypes;
    }

    public DbContext Resolve<T>()
    {
        var dbContextType = _cache.GetDbContext(typeof(T));
        if (dbContextType != null)
        {
            return Resolve(dbContextType);
        }

        throw new InvalidOperationException("DbContext not found.");
    }

    private DbContext Resolve(Type dbContextType)
    {
        return (DbContext)_serviceProvider.GetRequiredService(dbContextType);
        throw new InvalidOperationException("DbContext not found.");
    }

    public IDbContextFactory<DbContext> ResolveFactory<T>()
    {
        var dbContextType = _cache.GetDbContext(typeof(T));
        if (dbContextType != null)
        {
            var dbContextFactory = _serviceProvider.GetService(typeof(IDbContextFactory<>).MakeGenericType(dbContextType));

            if (dbContextFactory != null)
            {
                return ((IDbContextFactory<DbContext>)dbContextFactory);
            }
        }

        throw new InvalidOperationException("IDbContextFactory not found.");
    }
}
