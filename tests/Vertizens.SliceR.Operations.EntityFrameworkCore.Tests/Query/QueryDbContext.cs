using Microsoft.EntityFrameworkCore;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;
internal class QueryDbContext(DbContextOptions<QueryDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RootEntity>(x =>
        {
            x.ToTable(nameof(RootEntity));
        });

        modelBuilder.Entity<ChildEntity>(x =>
        {
            x.ToTable(nameof(ChildEntity));
        });

        modelBuilder.Entity<GrandChildEntity>(x =>
        {
            x.ToTable(nameof(GrandChildEntity));
        });
    }
}
