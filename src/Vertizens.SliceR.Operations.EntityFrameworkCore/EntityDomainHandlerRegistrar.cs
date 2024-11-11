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
    }
}
