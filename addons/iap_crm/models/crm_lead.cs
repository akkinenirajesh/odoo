csharp
public partial class Lead
{
    public override IEnumerable<string> GetMergeFields()
    {
        var baseFields = base.GetMergeFields();
        return baseFields.Concat(new[] { "RevealId" });
    }
}
