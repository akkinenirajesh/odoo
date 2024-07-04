csharp
public partial class Ddt
{
    public override string ToString()
    {
        return DisplayName;
    }

    private void _ComputeDisplayName()
    {
        DisplayName = $"{Name} ({Date:d})";
    }
}
