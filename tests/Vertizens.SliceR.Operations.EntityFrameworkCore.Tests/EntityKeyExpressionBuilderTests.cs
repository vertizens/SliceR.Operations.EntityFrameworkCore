namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class EntityKeyExpressionBuilderTests
{
    private class Entity1
    {
        public int Id { get; set; }
    }

    [Fact]
    public void PrimitiveKey()
    {
        var expressionBuilder = new EntityKeyExpressionBuilder<int, Entity1>([nameof(Entity1.Id)]);

        var expression = expressionBuilder.Build();
        var byPredicate = new ByPredicate<int, Entity1>(15, expression);
        var predicateExpression = byPredicate.CreateFilterExpression();
        var predicate = predicateExpression.Compile();

        var entities = new List<Entity1> { new() { Id = 14 }, new() { Id = 15 }, new() { Id = 16 } };

        var entity = entities.Where(predicate).Single();

        Assert.NotNull(entity);
        Assert.Equal(15, entity.Id);
    }
}