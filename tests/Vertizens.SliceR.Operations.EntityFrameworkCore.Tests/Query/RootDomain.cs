namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class RootDomain
{
    public int Id { get; set; }
    public string RootStringProperty { get; set; }
    public ChildDomain Child { get; set; }
}

