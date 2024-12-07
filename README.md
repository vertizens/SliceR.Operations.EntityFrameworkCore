# SliceR.Operations.EntityFrameworkCore

Adds EntityFramework Core boilerplate functionality for working with entities to SliceR.

## Getting Started

If you use EntityFramework Core for doing data access then this library can look at the registered DbContext types and for each entity register a `IHandler` that are these types: `NoFilterQueryableHandler`, `ByKeyHandler` (if primary key), `DeleteHandler` (if primary key), `InsertHandler`, and `UpdateHandler`.

Do this by registering these default handlers after the DbContext types have been registered.  Take care that to be able to get the list of entities the code creates the DbContext in this method so make sure they can be created according to their dependencies up to this point.  Also confirm that each entity only exists in one DbContext if there are multiple.

    services.AddSliceREntityFrameworkCoreDefaultHandlers();

## Default Handlers

`NoFilterQueryableHandler<TEntity>` - Use `NoFilter` type to signify that no parameters are passed in.  
Use it by injecting: `IHandler<NoFilter,IQueryable<TEntity>>` into your handler.

`ByKeyHandler<TKey, TEntity>` - Use `ByKey<TKey>` type to specify the key for the entity.  
Use it by injecting: `IHandler<ByKey<TKey>,TEntity?>` into your handler.

`InsertHandler<TEntity>` - Use `Insert<TEntity>` type to specify the entity to insert.  
Use it by injecting: `IHandler<Insert<TEntity>,TEntity>` into your handler. Calls `dbContext.SaveChangesAsync();`

`UpdateHandler<TEntity>` - Use `Update<TEntity>` type to specify the entity to update.  
Use it by injecting: `IHandler<Update<TEntity>,TEntity>` into your handler. Calls `dbContext.SaveChangesAsync();`

`DeleteHandler<TKey, TEntity>` - Use `Delete<TKey, TEntity>` type to specify the entity type and key to delete.  
Use it by injecting: `IHandler<Delete<TKey, TEntity>, bool>` into your handler. It returns true if records affected are > 0.

## Custom DbContext Operations

If you have custom queries or multiple operations that need to take place in the same handler simply create a custom handler.  Inject the DbContext like normal or possibly use `IEntityDbContextResolver` if you have multiple DbContext and don't want to hardcode which DbContext it is defined in.  
Make sure to register custom handlers first with:

    services.AddSlicerRHandlers();

## Keys

When registering handlers, the code looks for a registered service type of `IKeyPredicate<,>` where the 
second generic type is the entity.  If found then the first generic type must be the key type to be used.
By implementing `IKeyPredicate` then it takes over the definition of what key an entity uses and provides a way
to get an entity and match the key with its expression definiton.  The `GetPredicate` method returns 
`Expression<Func<TEntity, TKey, bool>>`.  If not specifically implemented and the entity primary key is only 
one property then the key is assumed to be the type of that one property.  If the key is has multiple properties,
ie compound key, then no key based handlers are registered unless the IKeyPredicate is registered.

## Domain and Minimal APIs

If the code is also using Minimal API or just rolling its own handlers for using this library with domain projection
then an implementation of `IEntityDomainHandlerRegistrar` is registered.  This allows this library to respond to 
Entity/Domain combinations by also registering any handlers required for returning domain instead of an entity.
Examples are:

* NoFilterQueryableHandler<TEntity, TDomain>
* ByKeyHandler<TKey, TEntity, TDomain>
* ByKeyForUpdateHandler<TKey, TUpdateDomain, TEntity>

There reason `ByKeyForUpdateHandler` exists is the special case where we want to get the existing entity and map onto to it
from TUpdateDomain and that could also mean any relationships and treat the entity as a graph update. 
There is special code that looks at the mapping of TUpdateDomain to TEntity and adds any `Includes` to the 
queryable getting the existing TEntity so it can be mapped to and allow EF Core to update relationships instances.