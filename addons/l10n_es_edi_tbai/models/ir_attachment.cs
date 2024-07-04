csharp
public partial class IrAttachment
{
    public void ActionDownloadXsdFiles()
    {
        // Download xmldsig-core-schema.xsd
        XmlUtils.LoadXsdFilesFromUrl(
            Env,
            "https://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd",
            "xmldsig-core-schema.xsd",
            xsdNamePrefix: "l10n_es_edi_tbai"
        );

        string[] agencies = { "gipuzkoa", "araba", "bizkaia" };
        foreach (var agency in agencies)
        {
            var urls = L10nEsEdiTbaiAgencies.GetKey(agency, "xsd_url");
            var names = L10nEsEdiTbaiAgencies.GetKey(agency, "xsd_name");

            if (urls is Dictionary<string, string> urlDict)
            {
                // For Bizkaia, one url per XSD (post/cancel)
                foreach (var moveType in new[] { "post", "cancel" })
                {
                    XmlUtils.LoadXsdFilesFromUrl(
                        Env,
                        urlDict[moveType],
                        names[moveType],
                        xsdNamePrefix: "l10n_es_edi_tbai"
                    );
                }
            }
            else if (urls is string url)
            {
                // For other agencies, single url to zip file
                XmlUtils.LoadXsdFilesFromUrl(
                    Env,
                    url,
                    xsdNamePrefix: "l10n_es_edi_tbai",
                    xsdNamesFilter: names.Values.ToList()
                );
            }
        }

        base.ActionDownloadXsdFiles();
    }
}
