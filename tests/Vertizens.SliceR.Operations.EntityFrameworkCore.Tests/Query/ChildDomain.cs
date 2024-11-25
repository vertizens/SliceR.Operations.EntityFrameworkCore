namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class ChildDomain
{
    public int Id { get; set; }
    public int IntProperty { get; set; }
    public GrandChildDomain GrandChild { get; set; }
}

