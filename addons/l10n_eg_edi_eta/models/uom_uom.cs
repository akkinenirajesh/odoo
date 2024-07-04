csharp
public partial class UomCode
{
    public override string ToString()
    {
        return Name;
    }
}

public partial class Uom
{
    public override string ToString()
    {
        // Assuming there's a Name field in the Uom class
        return Name;
    }
}
