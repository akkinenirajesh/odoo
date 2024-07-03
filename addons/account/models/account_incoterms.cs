csharp
public partial class AccountIncoterms
{
    public override string ToString()
    {
        return DisplayName;
    }

    public void ComputeDisplayName()
    {
        DisplayName = $"{(!string.IsNullOrEmpty(Code) ? $"[{Code}] " : "")}{Name}";
    }
}
