namespace Vertizens.SliceR.Operations.EntityFrameworkCore.Tests;

public class RootEntity
{
    public int Id { get; set; }
    public string RootStringProperty { get; set; }
    public ChildEntity Child { get; set; }
}

