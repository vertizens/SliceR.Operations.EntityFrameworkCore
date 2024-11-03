using Microsoft.EntityFrameworkCore;

namespace SliceR.Operations.EntityFrameworkCore;
public interface IEntityDbContextResolver
{

    DbContext Resolve<T>();
    IDbContextFactory<DbContext> ResolveFactory<T>();
}
