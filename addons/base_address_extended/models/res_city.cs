csharp
public partial class City
{
    public override string ToString()
    {
        return ComputeDisplayName();
    }

    private string ComputeDisplayName()
    {
        if (string.IsNullOrEmpty(Zipcode))
        {
            return Name;
        }
        return $"{Name} ({Zipcode})";
    }
}
