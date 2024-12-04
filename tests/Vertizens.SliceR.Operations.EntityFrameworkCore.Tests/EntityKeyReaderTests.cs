using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;
public class EntityKeyReaderTests
{
    private readonly IServiceProvider _serviceProvider;

    private class ChildEntityKey
    {
        public int Id { get; set; }
    }

    private class ChildEntityKeyPredicate : IKeyPredicate<ChildEntityKey, ChildEntity>
    {
        public Expression<Func<ChildEntity, ChildEntityKey, bool>> GetPredicate()
        {
            return (x, y) => x.Id == y.Id;
        }
    }

    private class CompoundKey
    {
        public int Id { get; set; }
        public DateOnly EffectiveDate { get; set; }
    }

    private class CompoundKeyPredicate(
        IKeyPredicateExpressionBuilder<CompoundKey, CompoundKeyEntity> _expressionBuilder
        ) : IKeyPredicate<CompoundKey, CompoundKeyEntity>
    {
        private readonly Expression<Func<CompoundKeyEntity, CompoundKey, bool>> _expression = _expressionBuilder.ApplyNameMatch().Build();

        public Expression<Func<CompoundKeyEntity, CompoundKey, bool>> GetPredicate()
        {
            return _expression;
        }
    }

    public EntityKeyReaderTests()
    {
        var services = new ServiceCollection();

        services.AddTypeMappers();
        services.AddDbContext<QueryDbContext>(options => options.UseSqlServer());
        services.AddSliceREntityFrameworkCoreDefaultHandlers();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void PrimitiveKeyReader()
    {
        var keyReader = _serviceProvider.GetRequiredService<IEntityKeyReader<int, RootEntity>>();

        var entity = new RootEntity { Id = 1 };
        var key = keyReader.ReadKey(entity);

        Assert.Equal(key, entity.Id);
    }

    [Fact]
    public void PrimitiveKeyReader_WithType()
    {
        var keyReader = _serviceProvider.GetRequiredService<IEntityKeyReader<ChildEntityKey, ChildEntity>>();

        var entity = new ChildEntity { Id = 1 };
        var key = keyReader.ReadKey(entity);

        Assert.Equal(key.Id, entity.Id);
    }

    [Fact]
    public void CompoundKeyReader()
    {
        var keyReader = _serviceProvider.GetRequiredService<IEntityKeyReader<CompoundKey, CompoundKeyEntity>>();

        var entity = new CompoundKeyEntity { Id = 1, EffectiveDate = DateOnly.Parse("2024-12-01") };
        var key = keyReader.ReadKey(entity);

        Assert.Equal(key.Id, entity.Id);
        Assert.Equal(key.EffectiveDate, entity.EffectiveDate);
    }
}
