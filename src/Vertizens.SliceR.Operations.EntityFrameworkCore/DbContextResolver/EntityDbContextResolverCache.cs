namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class EntityDbContextResolverCache : IEntityDbContextResolverCache
{
    private readonly Dictionary<Type, Type> _dbContextTypeByEntityType = [];

    public void SetDbContext(Type entityType, Type dbContextType)
    {
        _dbContextTypeByEntityType[entityType] = dbContextType;
    }

    public Type? GetDbContext(Type entityType)
    {
        _dbContextTypeByEntityType.TryGetValue(entityType, out var dbContextType);
        return dbContextType;
    }
}
