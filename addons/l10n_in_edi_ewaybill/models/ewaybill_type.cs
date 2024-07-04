csharp
public partial class EWayBillType
{
    public override string ToString()
    {
        return ComputeDisplayName();
    }

    private string ComputeDisplayName()
    {
        return string.Format("{0} (Sub-Type: {1})", Name, SubType);
    }
}
