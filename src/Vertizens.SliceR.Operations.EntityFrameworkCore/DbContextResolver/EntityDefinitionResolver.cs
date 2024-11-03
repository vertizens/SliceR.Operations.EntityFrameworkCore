
using Vertizens.SliceR.Operations;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal class EntityDefinitionResolver : IEntityDefinitionResolver, IEntityDefinitionResolverCache
{
    private readonly Dictionary<Type, EntityDefinition> _entityDefinitions = [];
    public EntityDefinition? Get(Type entityType)
    {
        return _entityDefinitions.TryGetValue(entityType, out EntityDefinition? value) ? value : null;
    }

    public IEnumerable<EntityDefinition> Get()
    {
        return _entityDefinitions.Values;
    }

    public void SetEntityDefinition(EntityDefinition entityDefinition)
    {
        _entityDefinitions[entityDefinition.EntityType] = entityDefinition;
    }
}
