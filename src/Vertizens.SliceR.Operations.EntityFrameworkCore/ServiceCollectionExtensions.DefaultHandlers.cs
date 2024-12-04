using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Vertizens.ServiceProxy;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;

/// <summary>
/// Extensions for a ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers default implementations for entities found in already registered <see cref="DbContext"/> instances to 
    /// handle <see cref="NoFilter"/>, <see cref="ByKey{TKey}"/>, <see cref="Insert{TDomain}"/>
    /// <see cref="Update{TEntity}"/>, <see cref="Delete{TKey, TDomain}"/>.
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="dbContextTypePredicate">Filter to reduce the found set of DbContext implementations</param>
    public static IServiceCollection AddSliceREntityFrameworkCoreDefaultHandlers(this IServiceCollection services, Func<Type, bool>? dbContextTypePredicate = null)
    {
        services.AddInterfaceTypes(typeof(IKeyPredicate<,>), Assembly.GetCallingAssembly(), ServiceLifetime.Singleton);
        services.TryAddScoped<IDbContextStartupFactory, EntityDbContextResolver>();
        services.TryAddScoped<IEntityDbContextResolver, EntityDbContextResolver>();
        services.TryAddSingleton<IEntityDbContextResolverCache, EntityDbContextResolverCache>();
        services.TryAddSingleton<EntityDefinitionResolver>();
        services.TryAddSingleton<IEntityDefinitionResolverCache>(sp => sp.GetRequiredService<EntityDefinitionResolver>());
        services.TryAddSingleton<IEntityDefinitionResolver>(sp => sp.GetRequiredService<EntityDefinitionResolver>());
        services.TryAddSingleton(typeof(IEntityKeyReader<,>), typeof(EntityKeyReader<,>));
        services.TryAddSingleton(typeof(IRelatedEntityQueryInclude<,>), typeof(DefaultRelatedEntityQueryInclude<,>));
        services.TryAddTransient(typeof(IKeyPredicateExpressionBuilder<,>), typeof(DefaultKeyPredicateExpressionBuilder<,>));

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider = serviceProvider.CreateScope().ServiceProvider;

        var startupFactory = serviceProvider.GetRequiredService<IDbContextStartupFactory>();
        var dbContextTypes = services.Where(x => typeof(DbContext).IsAssignableFrom(x.ServiceType)).Select(x => x.ServiceType);
        if (dbContextTypePredicate != null)
        {
            dbContextTypes = dbContextTypes.Where(dbContextTypePredicate);
        }
        var entityTypes = startupFactory.GetEntityTypes(dbContextTypes);
        var keyPredicateServices = services.Where(x => x.ServiceType.IsGenericTypeDefinition && x.ServiceType.GetGenericTypeDefinition() == typeof(IKeyPredicate<,>)).ToList();

        foreach (var entityType in entityTypes)
        {
            services.AddDefaultHandlers(entityType, keyPredicateServices, serviceProvider);
        }

        services.RemoveAll(typeof(IDbContextStartupFactory));
        services.RemoveAll(typeof(IEntityDbContextResolverCache));
        services.AddSingleton(serviceProvider.GetRequiredService<IEntityDbContextResolverCache>());

        services.RemoveAll(typeof(IEntityDefinitionResolverCache));
        services.RemoveAll(typeof(IEntityDefinitionResolver));
        services.TryAddSingleton<IEntityDefinitionResolver>(serviceProvider.GetRequiredService<EntityDefinitionResolver>());

        services.TryAddSingleton<IEntityDomainHandlerRegistrar, EntityDomainHandlerRegistrar>();

        return services;
    }

    private static IServiceCollection AddDefaultHandlers(this IServiceCollection services, IEntityType entityType, IList<ServiceDescriptor> keyPredicateServices, IServiceProvider serviceProvider)
    {
        var entityClassType = entityType.ClrType;

        if (!entityType.IsAbstract())
        {
            var entityDefinitionCache = serviceProvider.GetRequiredService<IEntityDefinitionResolverCache>();

            services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(NoFilter), typeof(IQueryable<>).MakeGenericType(entityClassType)),
                typeof(NoFilterQueryableHandler<>).MakeGenericType(entityClassType));

            var primaryKey = entityType.FindPrimaryKey();
            Type? keyType = null;
            if (primaryKey != null)
            {
                keyType = services.GetKeyType(keyPredicateServices, entityClassType, [.. primaryKey.Properties]);
                if (keyType != null)
                {
                    services.AddKeyHandler(keyType, entityClassType);

                    services.TryAddTransient(
                        typeof(IHandler<,>).MakeGenericType(typeof(Delete<,>).MakeGenericType(keyType, entityClassType), typeof(bool)),
                        typeof(DeleteHandler<,>).MakeGenericType(keyType, entityClassType));
                    services.TryAddTransient(
                        typeof(IHandler<,>).MakeGenericType(typeof(DeleteSet<,>).MakeGenericType(keyType, entityClassType), typeof(int)),
                        typeof(DeleteSetHandler<,>).MakeGenericType(keyType, entityClassType));
                }
            }

            services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(Insert<>).MakeGenericType(entityClassType), entityClassType),
                typeof(InsertHandler<>).MakeGenericType(entityClassType));
            services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(InsertSet<>).MakeGenericType(entityClassType), typeof(IEnumerable<>).MakeGenericType(entityClassType)),
                typeof(InsertSetHandler<>).MakeGenericType(entityClassType));

            services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(Update<>).MakeGenericType(entityClassType), entityClassType),
                typeof(UpdateHandler<>).MakeGenericType(entityClassType));
            services.TryAddTransient(
                typeof(IHandler<,>).MakeGenericType(typeof(UpdateSet<>).MakeGenericType(entityClassType), typeof(IEnumerable<>).MakeGenericType(entityClassType)),
                typeof(UpdateSetHandler<>).MakeGenericType(entityClassType));

            entityDefinitionCache.SetEntityDefinition(new EntityDefinition { EntityType = entityClassType, KeyType = keyType });
        }

        return services;
    }

    private static IServiceCollection AddKeyHandler(this IServiceCollection services, Type keyType, Type entityClassType)
    {
        var byKeyType = typeof(ByKey<>).MakeGenericType(keyType);
        var handlerType = typeof(ByKeyHandler<,>).MakeGenericType(keyType, entityClassType);
        services.TryAddTransient(typeof(IHandler<,>).MakeGenericType(byKeyType, entityClassType), handlerType);

        return services;
    }

    private static Type? GetKeyType(this IServiceCollection services, IList<ServiceDescriptor> keyPredicateServices, Type entityClassType, IList<IProperty> properties)
    {
        Type? keyType = null;
        var propertyTypes = properties.Select(p => p.ClrType).ToArray();
        var entityKeyPredicateService = keyPredicateServices.FirstOrDefault(x => x.ServiceType.GetGenericArguments()[1] == entityClassType);
        if (entityKeyPredicateService != null)
        {
            keyType = entityKeyPredicateService.ServiceType.GetGenericArguments()[0];
        }
        else if (propertyTypes.Length == 1)
        {
            keyType = propertyTypes[0];
            var expressionBuilderInterfaceType = typeof(IKeyPredicate<,>).MakeGenericType(keyType, entityClassType);
            var expressionBuilderImplementationType = typeof(PropertyKeyPredicate<,>).MakeGenericType(keyType, entityClassType);
            object implementationFactory(IServiceProvider serviceProvider)
            {
                return ActivatorUtilities.CreateInstance(serviceProvider, expressionBuilderImplementationType, properties.First().Name);
            }
            services.TryAddSingleton(expressionBuilderInterfaceType, implementationFactory);
        }

        return keyType;
    }
}
