using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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
        services.TryAddScoped<IDbContextStartupFactory, EntityDbContextResolver>();
        services.TryAddScoped<IEntityDbContextResolver, EntityDbContextResolver>();
        services.TryAddSingleton<IEntityDbContextResolverCache, EntityDbContextResolverCache>();
        services.TryAddSingleton<EntityDefinitionResolver>();
        services.TryAddSingleton<IEntityDefinitionResolverCache>(sp => sp.GetRequiredService<EntityDefinitionResolver>());
        services.TryAddSingleton<IEntityDefinitionResolver>(sp => sp.GetRequiredService<EntityDefinitionResolver>());

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        serviceProvider = serviceProvider.CreateScope().ServiceProvider;

        var startupFactory = serviceProvider.GetRequiredService<IDbContextStartupFactory>();
        var dbContextTypes = services.Where(x => typeof(DbContext).IsAssignableFrom(x.ServiceType)).Select(x => x.ServiceType);
        if (dbContextTypePredicate != null)
        {
            dbContextTypes = dbContextTypes.Where(dbContextTypePredicate);
        }
        var entityTypes = startupFactory.GetEntityTypes(dbContextTypes);

        foreach (var entityType in entityTypes)
        {
            services.AddDefaultHandlers(entityType, serviceProvider);
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

    private static IServiceCollection AddDefaultHandlers(this IServiceCollection services, IEntityType entityType, IServiceProvider serviceProvider)
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
                keyType = GetKeyType([.. primaryKey.Properties]);
                switch (primaryKey.Properties.Count)
                {
                    case 1:
                        services.AddKeyHandler(entityClassType, [.. primaryKey.Properties]);
                        services.AddKeySetHandler(entityClassType, primaryKey.Properties[0]);

                        services.TryAddTransient(
                            typeof(IHandler<,>).MakeGenericType(typeof(Delete<,>).MakeGenericType(keyType, entityClassType), typeof(bool)),
                            typeof(DeleteHandler<,>).MakeGenericType(keyType, entityClassType));
                        services.TryAddTransient(
                            typeof(IHandler<,>).MakeGenericType(typeof(DeleteSet<,>).MakeGenericType(keyType, entityClassType), typeof(int)),
                            typeof(DeleteSetHandler<,>).MakeGenericType(keyType, entityClassType));

                        object implementationFactory(IServiceProvider serviceProvider)
                        {
                            return ActivatorUtilities.CreateInstance(serviceProvider,
                                typeof(EntityKeyReader<,>).MakeGenericType(keyType, entityClassType),
                                (ICollection<string>)primaryKey.Properties.Select(p => p.Name).ToArray());
                        }
                        services.TryAddSingleton(typeof(IEntityKeyReader<,>).MakeGenericType(keyType, entityClassType), implementationFactory);

                        break;
                    default:
                        services.AddKeyHandler(entityClassType, [.. primaryKey.Properties]);
                        services.TryAddTransient(
                            typeof(IHandler<,>).MakeGenericType(typeof(Delete<,>)
                            .MakeGenericType(keyType, entityClassType), typeof(bool)),
                            typeof(DeleteHandler<,>).MakeGenericType(keyType, entityClassType));
                        keyType = null;
                        break;
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

    private static IServiceCollection AddKeyHandler(this IServiceCollection services, Type entityClassType, IList<IProperty> properties)
    {
        Type keyType = GetKeyType(properties);
        var expressionBuilderInterfaceType = typeof(IEntityKeyExpressionBuilder<,>).MakeGenericType(keyType, entityClassType);
        var expressionBuilderImplementationType = typeof(EntityKeyExpressionBuilder<,>).MakeGenericType(keyType, entityClassType);
        object implementationFactory(IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, expressionBuilderImplementationType, (ICollection<string>)properties.Select(p => p.Name).ToArray());
        }
        services.TryAddSingleton(expressionBuilderInterfaceType, implementationFactory);

        var byKeyType = typeof(ByKey<>).MakeGenericType(keyType);
        var handlerType = typeof(ByKeyHandler<,>).MakeGenericType(keyType, entityClassType);
        services.TryAddTransient(MakeHandler(byKeyType, entityClassType), handlerType);

        return services;
    }

    private static Type GetKeyType(IList<IProperty> properties)
    {
        var propertyTypes = properties.Select(p => p.ClrType).ToArray();
        Type keyType;
        if (propertyTypes.Length == 1)
        {
            keyType = properties.First().ClrType;
        }
        else
        {
            var createMethod = typeof(ValueTuple).GetMethod(nameof(ValueTuple.Create), BindingFlags.Static | BindingFlags.Public, propertyTypes);
            var keyCreateMethod = createMethod!.MakeGenericMethod(propertyTypes);
            keyType = keyCreateMethod.ReturnType;
        }

        return keyType;
    }

    private static IServiceCollection AddKeySetHandler(this IServiceCollection services, Type entityClassType, IProperty property)
    {
        var expressionBuilderInterfaceType = typeof(IEntityKeySetExpressionBuilder<,>).MakeGenericType(property.ClrType, entityClassType);
        var expressionBuilderImplementationType = typeof(EntityKeySetExpressionBuilder<,>).MakeGenericType(property.ClrType, entityClassType);
        object implementationFactory(IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, expressionBuilderImplementationType, property.Name);
        }
        services.TryAddSingleton(expressionBuilderInterfaceType, implementationFactory);

        var handlerType = typeof(ByKeySetHandler<,>).MakeGenericType(property.ClrType, entityClassType);
        services.TryAddTransient(MakeHandler(typeof(ByKeySet<>).MakeGenericType(property.ClrType), typeof(IQueryable<>).MakeGenericType(entityClassType)), handlerType);

        return services;
    }

    private static Type MakeHandler(Type requestType, Type resultType)
    {
        return typeof(IHandler<,>).MakeGenericType(requestType, resultType);
    }
}
