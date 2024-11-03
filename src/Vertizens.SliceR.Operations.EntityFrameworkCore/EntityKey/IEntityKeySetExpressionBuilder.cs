using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public interface IEntityKeySetExpressionBuilder<TKey, TEntity>
{
    Expression<Func<TEntity, ICollection<TKey>, bool>> Build();
}
