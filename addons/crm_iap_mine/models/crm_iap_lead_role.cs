csharp
public partial class IapLeadRole
{
    public override string ToString()
    {
        return this.ComputeDisplayName();
    }

    private string ComputeDisplayName()
    {
        return (this.Name ?? "").Replace("_", " ").ToTitleCase();
    }
}
