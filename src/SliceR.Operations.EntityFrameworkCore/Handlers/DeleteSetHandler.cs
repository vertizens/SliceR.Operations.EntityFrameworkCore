using Microsoft.EntityFrameworkCore;
using Vertizens.SliceR;
using Vertizens.SliceR.Operations;

namespace SliceR.Operations.EntityFrameworkCore;
public class DeleteSetHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeySetExpressionBuilder<TKey, TEntity> _entityKeySetExpressionBuilder
    ) : IHandler<DeleteSet<TKey, TEntity>, int>
    where TEntity : class
{
    public async Task<int> Handle(DeleteSet<TKey, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var predicate = _entityKeySetExpressionBuilder.Build();
        var expression = new ByPredicate<ICollection<TKey>, TEntity>(request.Keys, predicate).CreateFilterExpression();
        var recordsAffected = await dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync(cancellationToken);

        return recordsAffected;
    }
}
