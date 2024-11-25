using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vertizens.TypeMapper;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;


public partial class DefaultRelatedEntityQueryIncludeTests
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultRelatedEntityQueryIncludeTests()
    {
        var services = new ServiceCollection();

        services.AddTypeMappers();
        services.AddDbContext<QueryDbContext>(options => options.UseSqlServer());
        services.AddSliceREntityFrameworkCoreDefaultHandlers();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Include_GrandChildEntity()
    {
        var typeMapperBuilder = _serviceProvider.GetRequiredService<ITypeMapperBuilder<RootDomain, RootEntity>>();
        var typeMapperExpressionBuilder = _serviceProvider.GetRequiredService<ITypeMapperExpressionBuilder<RootDomain, RootEntity>>();
        var entityDefinitionResolver = _serviceProvider.GetRequiredService<IEntityDefinitionResolver>();

        var queryIncludes = new DefaultRelatedEntityQueryInclude<RootDomain, RootEntity>(typeMapperBuilder, typeMapperExpressionBuilder, entityDefinitionResolver);

        var includes = queryIncludes.GetIncludes();


    }
}
