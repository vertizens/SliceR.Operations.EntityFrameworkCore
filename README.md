# SliceR.Operations.EntityFrameworkCore

Adds to SliceR .NET EntityFramework Core boilerplate functionality for working with entities

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

    services.AddSlicerRHandllers();