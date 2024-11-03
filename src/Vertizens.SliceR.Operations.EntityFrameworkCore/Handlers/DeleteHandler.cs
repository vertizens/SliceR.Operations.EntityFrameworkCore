using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
public class DeleteHandler<TKey, TEntity>(
    IEntityDbContextResolver _entityDbContextResolver,
    IEntityKeyExpressionBuilder<TKey, TEntity> _entityKeyExpressionBuilder
    ) : IHandler<Delete<TKey, TEntity>, bool>
    where TEntity : class
{
    public async Task<bool> Handle(Delete<TKey, TEntity> request, CancellationToken cancellationToken = default)
    {
        var dbContext = _entityDbContextResolver.Resolve<TEntity>();
        var predicate = _entityKeyExpressionBuilder.Build();
        var expression = new ByPredicate<TKey, TEntity>(request.Key, predicate).CreateFilterExpression();
        var recordsAffected = await dbContext.Set<TEntity>().Where(expression).ExecuteDeleteAsync(cancellationToken);

        return recordsAffected > 0;
    }
}
