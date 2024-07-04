csharp
public partial class TestInheritMother 
{
    public string Name { get; set; }
    public string State { get; set; }
    public string Surname { get; set; }

    public void ComputeSurname()
    {
        this.Surname = this.Name ?? "";
    }
}

public partial class TestInheritDaughter 
{
    public TestInheritMother TemplateId { get; set; }
    public string FieldInDaughter { get; set; }
}

public partial class TestInheritMother2 
{
    public string FieldInMother { get; set; }
    public ResPartner PartnerId { get; set; }
}

public partial class TestInheritMother3 
{
}

public partial class TestInheritDaughter2 
{
}

public partial class ResPartner 
{
}

public partial class TestInheritTestInheritProperty 
{
    public string Name { get; set; }
    public int PropertyFoo { get; set; }
    public int PropertyBar { get; set; }
}

public partial class TestInheritTestInheritProperty2 
{
    public void ComputeBar()
    {
        this.PropertyBar = 42;
    }
}

public partial class TestInheritParent1 
{
    public string Stuff()
    {
        return "P1";
    }
}

public partial class TestInheritChild 
{
    public int Bar { get; set; }

    public string Stuff()
    {
        return base.Stuff() + "C1";
    }
}

public partial class TestInheritParent2 
{
    public int Foo { get; set; }

    public string Stuff()
    {
        return base.Stuff() + "P2";
    }
}

public partial class TestNewApiSelection 
{
}

public partial class TestInheritIsPublishedMixin 
{
    public bool Published { get; set; }
}

public partial class TestNewApiMessage 
{
}
