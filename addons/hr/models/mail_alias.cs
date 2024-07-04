csharp
public partial class Alias
{
    public string GetAliasContactDescription()
    {
        if (this.AliasContact == AliasContactType.Employees)
        {
            return "addresses linked to registered employees";
        }
        return base.GetAliasContactDescription();
    }
}
