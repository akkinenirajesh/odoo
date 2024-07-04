csharp
public partial class AfipResponsibilityType
{
    public override string ToString()
    {
        return Name;
    }

    public List<string> LoadPosDataFields(int configId)
    {
        return new List<string> { "Name" };
    }
}
