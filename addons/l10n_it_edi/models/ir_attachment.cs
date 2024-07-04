csharp
public partial class IrAttachment
{
    private static readonly Regex FATTURAPA_FILENAME_RE = new Regex(@"[A-Z]{2}[A-Za-z0-9]{2,28}_[A-Za-z0-9]{0,5}\.((?i:xml\.p7m|xml))");

    public List<Dictionary<string, object>> DecodeEdiL10nItEdi(string name, byte[] content)
    {
        XmlDocument ParseXml(string fileName, byte[] fileContent)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(System.Text.Encoding.UTF8.GetString(fileContent));
                return xmlDoc;
            }
            catch (XmlException e)
            {
                Env.Logger.Info($"XML parsing of {fileName} failed: {e.Message}");
                return null;
            }
        }

        var xmlTree = ParseXml(name, content);
        if (xmlTree == null)
        {
            // The file may have a Cades signature, trying to remove it
            xmlTree = ParseXml(name, RemoveSignature(content));
            if (xmlTree == null)
            {
                Env.Logger.Info($"Italian EDI invoice file {name} cannot be decoded.");
                return new List<Dictionary<string, object>>();
            }
        }

        return xmlTree.SelectNodes("//FatturaElettronicaBody").Cast<XmlNode>().Select(xmlMoveTree => new Dictionary<string, object>
        {
            ["Filename"] = name,
            ["Content"] = content,
            ["Attachment"] = this,
            ["XmlTree"] = xmlMoveTree,
            ["Type"] = "l10n_it_edi",
            ["SortWeight"] = 11
        }).ToList();
    }

    public bool IsL10nItEdiImportFile()
    {
        bool isXml = Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
            || MimeType.EndsWith("/xml", StringComparison.OrdinalIgnoreCase)
            || (MimeType.Contains("text/plain") && Raw != null && Raw.Take(5).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("<?xml")));

        bool isP7m = MimeType == "application/pkcs7-mime";

        return (isXml || isP7m) && FATTURAPA_FILENAME_RE.IsMatch(Name);
    }

    public List<Dictionary<string, object>> GetEdiSupportedFormats()
    {
        var baseFormats = base.GetEdiSupportedFormats();
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                ["Format"] = "l10n_it_edi",
                ["Check"] = new Func<IrAttachment, bool>(a => a.IsL10nItEdiImportFile()),
                ["Decoder"] = new Func<string, byte[], List<Dictionary<string, object>>>(DecodeEdiL10nItEdi)
            }
        }.Concat(baseFormats).ToList();
    }

    private byte[] RemoveSignature(byte[] content)
    {
        // Implementation of remove_signature function
        // This is a placeholder and should be replaced with actual implementation
        return content;
    }
}
