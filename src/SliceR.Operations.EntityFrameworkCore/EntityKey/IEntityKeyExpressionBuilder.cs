using System.Linq.Expressions;

namespace SliceR.Operations.EntityFrameworkCore;
public interface IEntityKeyExpressionBuilder<TKey, TEntity>
{
    Expression<Func<TEntity, TKey, bool>> Build();
}
