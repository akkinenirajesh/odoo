csharp
public partial class AccountMove
{
    public Account.Edi.Xml.OioUbl201 GetUblCiiBuilderFromXmlTree(XmlElement tree)
    {
        var customizationId = tree.SelectSingleNode("//*[local-name()='CustomizationID']");
        if (customizationId != null && customizationId.InnerText.Contains("OIOUBL-2"))
        {
            return Env.Get<Account.Edi.Xml.OioUbl201>();
        }
        return base.GetUblCiiBuilderFromXmlTree(tree);
    }
}
