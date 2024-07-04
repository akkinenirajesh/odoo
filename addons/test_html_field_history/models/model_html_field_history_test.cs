C#
public partial class TestHtmlFieldHistory.ModelHtmlFieldHistoryTest 
{
    public List<string> GetVersionedFields()
    {
        return new List<string>()
        {
            nameof(VersionedField1),
            nameof(VersionedField2),
        };
    }
}
