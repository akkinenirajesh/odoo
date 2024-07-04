csharp
public partial class IdentificationType
{
    public override string ToString()
    {
        return Name;
    }

    public string ComputeDisplayName()
    {
        var multiLocalization = Env.Query<IdentificationType>().Select(t => t.Country).Distinct().Count() > 1;
        return $"{Name}{(multiLocalization && Country != null ? $" ({Country.Code})" : "")}";
    }
}
