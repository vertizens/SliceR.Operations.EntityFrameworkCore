namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal interface IEntityDbContextResolverCache
{
    void SetDbContext(Type entityType, Type dbContextType);
    Type? GetDbContext(Type entityType);
}
