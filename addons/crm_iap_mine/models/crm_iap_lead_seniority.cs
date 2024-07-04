csharp
public partial class IapLeadSeniority
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeDisplayName()
    {
        DisplayName = (Name ?? "").Replace("_", " ").ToTitleCase();
    }
}
