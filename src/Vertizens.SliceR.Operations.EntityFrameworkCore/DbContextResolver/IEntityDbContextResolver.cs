using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public interface IEntityDbContextResolver
{

    DbContext Resolve<T>();
    IDbContextFactory<DbContext> ResolveFactory<T>();
}
