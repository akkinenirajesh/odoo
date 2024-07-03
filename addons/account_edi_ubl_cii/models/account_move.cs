csharp
public partial class AccountMove
{
    public override string ToString()
    {
        // Implement string representation logic here
        return base.ToString();
    }

    public void ComputeLinkedAttachmentId()
    {
        // Implement the computation logic for UblCiiXmlId here
        // This method should set the value of UblCiiXmlId based on UblCiiXmlFile
    }

    public Core.Attachment GetUblCiiBuilderFromXmlTree(XmlElement tree)
    {
        var customizationId = tree.SelectSingleNode("//*[local-name()='CustomizationID']");
        if (tree.Name == "{urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100}CrossIndustryInvoice")
        {
            return Env.Get<AccountEdiXmlCii>();
        }

        var ublVersion = tree.SelectSingleNode("//*[local-name()='UBLVersionID']");
        if (ublVersion != null)
        {
            if (ublVersion.InnerText == "2.0")
                return Env.Get<AccountEdiXmlUbl20>();
            if (new[] { "2.1", "2.2", "2.3" }.Contains(ublVersion.InnerText))
                return Env.Get<AccountEdiXmlUbl21>();
        }

        if (customizationId != null)
        {
            if (customizationId.InnerText.Contains("xrechnung"))
                return Env.Get<AccountEdiXmlUblDe>();
            if (customizationId.InnerText == "urn:cen.eu:en16931:2017#compliant#urn:fdc:nen.nl:nlcius:v1.0")
                return Env.Get<AccountEdiXmlUblNl>();
            if (customizationId.InnerText == "urn:cen.eu:en16931:2017#conformant#urn:fdc:peppol.eu:2017:poacc:billing:international:aunz:3.0")
                return Env.Get<AccountEdiXmlUblANz>();
            if (customizationId.InnerText == "urn:cen.eu:en16931:2017#conformant#urn:fdc:peppol.eu:2017:poacc:billing:international:sg:3.0")
                return Env.Get<AccountEdiXmlUblSg>();
            if (customizationId.InnerText.Contains("urn:cen.eu:en16931:2017"))
                return Env.Get<AccountEdiXmlUblBis3>();
        }

        return null;
    }

    public Func<Dictionary<string, object>, bool, object> GetEdiDecoder(Dictionary<string, object> fileData, bool isNew = false)
    {
        if ((string)fileData["type"] == "xml")
        {
            var ublCiiXmlBuilder = GetUblCiiBuilderFromXmlTree((XmlElement)fileData["xml_tree"]);
            if (ublCiiXmlBuilder != null)
            {
                return ublCiiXmlBuilder.ImportInvoiceUblCii;
            }
        }

        // Call base implementation (assuming it's available in the base class)
        return base.GetEdiDecoder(fileData, isNew);
    }

    public bool NeedUblCiiXml()
    {
        return !InvoicePdfReportId.HasValue
            && !UblCiiXmlId.HasValue
            && IsSaleDocument()
            && PartnerCommercialPartner.UblCiiFormat;
    }
}
