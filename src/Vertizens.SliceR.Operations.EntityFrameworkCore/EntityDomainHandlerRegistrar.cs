using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class EntityDomainHandlerRegistrar : IEntityDomainHandlerRegistrar
{
    public void Register(EntityDomainHandlerContext context)
    {
        if (context.RequestType.IsGenericType && context.RequestType.GetGenericTypeDefinition() == typeof(ByKey<>))
        {
            context.Services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(context.RequestType, context.DomainType),
                typeof(ByKeyHandler<,,>).MakeGenericType(context.EntityDefinition.KeyType!, context.EntityDefinition.EntityType, context.DomainType));
        }

        if (context.RequestType == typeof(NoFilter))
        {
            context.Services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(context.RequestType, context.ResultType),
                typeof(NoFilterQueryableHandler<,>).MakeGenericType(context.EntityDefinition.EntityType, context.DomainType));
        }

        if (context.RequestType.IsGenericType && context.RequestType.GetGenericTypeDefinition() == typeof(Update<,>))
        {
            var updateDomainType = context.RequestType.GetGenericArguments()[1];
            context.Services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(ByKeyForUpdate<,>).MakeGenericType(context.EntityDefinition.KeyType!, updateDomainType), context.EntityDefinition.EntityType),
                typeof(ByKeyForUpdateHandler<,,>).MakeGenericType(context.EntityDefinition.KeyType!, updateDomainType, context.EntityDefinition.EntityType));
        }
    }
}
