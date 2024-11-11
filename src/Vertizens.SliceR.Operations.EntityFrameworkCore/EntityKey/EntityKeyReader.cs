using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class EntityKeyReader<TKey, TEntity>(
    ICollection<string> _keyPropertyNames
    ) : IEntityKeyReader<TKey, TEntity>
{
    private Func<TEntity, TKey>? _readerCache;

    public TKey ReadKey(TEntity entity)
    {
        _readerCache ??= BuildReader(_keyPropertyNames);
        return _readerCache(entity);
    }

    private static Func<TEntity, TKey> BuildReader(ICollection<string> keyPropertyNames)
    {
        var parameterEntity = Expression.Parameter(typeof(TEntity), "x");
        var entityProperty = Expression.Property(parameterEntity, typeof(TEntity).GetProperty(keyPropertyNames.First())!);

        var expression = Expression.Lambda<Func<TEntity, TKey>>(entityProperty!, parameterEntity);
        return expression.Compile();
    }
}
