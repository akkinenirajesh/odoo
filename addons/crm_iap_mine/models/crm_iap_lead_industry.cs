csharp
public partial class IapLeadIndustry
{
    public override string ToString()
    {
        return Name;
    }

    public string[] GetRevealIds()
    {
        return RevealIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
    }

    public void SetRevealIds(IEnumerable<string> ids)
    {
        RevealIds = string.Join(",", ids);
    }
}
