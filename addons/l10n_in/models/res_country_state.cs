csharp
public partial class CountryState
{
    public override string ToString()
    {
        return $"{Name} ({Code})";
    }

    public string GetFullName()
    {
        return $"{Name}, {Env.Find<Core.Country>(Country).Name}";
    }
}
