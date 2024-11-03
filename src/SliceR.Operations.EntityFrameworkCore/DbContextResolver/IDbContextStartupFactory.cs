using Microsoft.EntityFrameworkCore.Metadata;

namespace SliceR.Operations.EntityFrameworkCore;
internal interface IDbContextStartupFactory
{
    IEnumerable<IEntityType> GetEntityTypes(IEnumerable<Type> dbContextTypes);
}
