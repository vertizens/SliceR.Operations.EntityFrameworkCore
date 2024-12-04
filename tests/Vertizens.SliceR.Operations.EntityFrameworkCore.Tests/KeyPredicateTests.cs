using System.Linq.Expressions;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class KeyPredicateTests
{
    private class Entity1
    {
        public int Id { get; set; }
    }

    private class Entity2
    {
        public int Id { get; set; }
        public DateOnly KeyDate { get; set; }
        public string? SomeData { get; set; }
    }

    private class Entity2Key
    {
        public required int Id { get; init; }
        public required DateOnly KeyDate { get; init; }
    }

    private class Entity2KeyPredicate : IKeyPredicate<Entity2Key, Entity2>
    {
        public Expression<Func<Entity2, Entity2Key, bool>> GetPredicate()
        {
            return (x, key) => x.Id == key.Id && x.KeyDate == key.KeyDate;
        }
    }

    [Fact]
    public void PrimitiveKey()
    {
        var expressionBuilder = new PropertyKeyPredicate<int, Entity1>(nameof(Entity1.Id));

        var expression = expressionBuilder.GetPredicate();
        var byPredicate = new ByPredicate<int, Entity1>(15, expression);
        var predicateExpression = byPredicate.CreateFilterExpression();
        var predicate = predicateExpression.Compile();

        var entities = new List<Entity1> { new() { Id = 14 }, new() { Id = 15 }, new() { Id = 16 } };

        var entity = entities.Where(predicate).Single();

        Assert.NotNull(entity);
        Assert.Equal(15, entity.Id);
    }

    [Fact]
    public void CompoundKey()
    {
        var keyPredicate = new Entity2KeyPredicate();

        var expression = keyPredicate.GetPredicate();
        var byPredicate = new ByPredicate<Entity2Key, Entity2>(new Entity2Key { Id = 15, KeyDate = DateOnly.Parse("2024-12-01") }, expression);
        var predicateExpression = byPredicate.CreateFilterExpression();
        var predicate = predicateExpression.Compile();

        var entities = new List<Entity2>
        {
            new() { Id = 14, KeyDate = DateOnly.Parse("2024-11-01") },
            new() { Id = 15, KeyDate = DateOnly.Parse("2024-11-01") },
            new() { Id = 15, KeyDate = DateOnly.Parse("2024-12-01") }
        };

        var entity = entities.Where(predicate).Single();

        Assert.NotNull(entity);
        Assert.Equal(15, entity.Id);
        Assert.Equal(DateOnly.Parse("2024-12-01"), entity.KeyDate);
    }
}