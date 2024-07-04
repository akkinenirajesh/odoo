csharp
public partial class ItemCode
{
    public override string ToString()
    {
        return $"{Code} {Description}";
    }

    [ComputedField]
    public string DisplayName
    {
        get
        {
            return $"{Code} {Description}";
        }
    }

    public override IEnumerable<string> GetFieldsToSearchByName()
    {
        return new[] { nameof(Code), nameof(Description) };
    }
}
