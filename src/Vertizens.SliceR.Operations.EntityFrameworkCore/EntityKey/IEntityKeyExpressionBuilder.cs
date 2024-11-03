using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public interface IEntityKeyExpressionBuilder<TKey, TEntity>
{
    Expression<Func<TEntity, TKey, bool>> Build();
}
