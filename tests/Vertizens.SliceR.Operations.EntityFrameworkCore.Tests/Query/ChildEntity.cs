namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class ChildEntity
{
    public int Id { get; set; }
    public int IntProperty { get; set; }
    public GrandChildEntity GrandChild { get; set; }
}

