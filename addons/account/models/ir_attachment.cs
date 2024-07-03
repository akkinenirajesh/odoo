csharp
public partial class IrAttachment
{
    public byte[] BuildZipFromAttachments()
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var attachment in this.Env.IrAttachments.Browse(this.Id))
                {
                    var entry = zipArchive.CreateEntry(attachment.DisplayName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(attachment.Raw, 0, attachment.Raw.Length);
                    }
                }
            }
            return memoryStream.ToArray();
        }
    }

    public List<Dictionary<string, object>> DecodeEdiXml(string filename, byte[] content)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(System.Text.Encoding.UTF8.GetString(content));

            if (xmlDoc.DocumentElement != null)
            {
                return new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "attachment", this },
                        { "filename", filename },
                        { "content", content },
                        { "xml_tree", xmlDoc },
                        { "sort_weight", 10 },
                        { "type", "xml" }
                    }
                };
            }
        }
        catch (Exception e)
        {
            this.Env.Logger.Info($"Error when reading the xml file \"{filename}\": {e.Message}");
        }

        return new List<Dictionary<string, object>>();
    }

    public List<Dictionary<string, object>> DecodeEdiPdf(string filename, byte[] content)
    {
        var toProcess = new List<Dictionary<string, object>>();

        try
        {
            using (var pdfReader = new PdfReader(content))
            {
                // Process embedded files (This is a simplified version, as C# PDF libraries might differ)
                // You may need to implement custom logic to extract embedded XML files from PDF

                // Process the PDF itself
                toProcess.Add(new Dictionary<string, object>
                {
                    { "filename", filename },
                    { "content", content },
                    { "pdf_reader", pdfReader },
                    { "attachment", this },
                    { "sort_weight", 20 },
                    { "type", "pdf" }
                });
            }
        }
        catch (Exception e)
        {
            this.Env.Logger.Info($"Error when reading the pdf file \"{filename}\": {e.Message}");
        }

        return toProcess;
    }

    public List<Dictionary<string, object>> DecodeEdiBinary(string filename, byte[] content)
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "filename", filename },
                { "content", content },
                { "attachment", this },
                { "sort_weight", 100 },
                { "type", "binary" }
            }
        };
    }

    public List<Dictionary<string, object>> GetEdiSupportedFormats()
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "format", "pdf" },
                { "check", new Func<IrAttachment, bool>(a => a.Mimetype.Contains("pdf")) },
                { "decoder", new Func<string, byte[], List<Dictionary<string, object>>>(DecodeEdiPdf) }
            },
            new Dictionary<string, object>
            {
                { "format", "xml" },
                { "check", new Func<IrAttachment, bool>(IsXml) },
                { "decoder", new Func<string, byte[], List<Dictionary<string, object>>>(DecodeEdiXml) }
            },
            new Dictionary<string, object>
            {
                { "format", "binary" },
                { "check", new Func<IrAttachment, bool>(a => true) },
                { "decoder", new Func<string, byte[], List<Dictionary<string, object>>>(DecodeEdiBinary) }
            }
        };
    }

    private bool IsXml(IrAttachment attachment)
    {
        var isTextPlainXml = attachment.Mimetype.Contains("text/plain") && 
                             (attachment.Raw.Take(5).SequenceEqual(System.Text.Encoding.ASCII.GetBytes("<?xml")) ||
                              attachment.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
        return attachment.Mimetype.EndsWith("/xml") || isTextPlainXml;
    }

    public List<Dictionary<string, object>> UnwrapEdiAttachments()
    {
        var toProcess = new List<Dictionary<string, object>>();

        var supportedFormats = GetEdiSupportedFormats();
        foreach (var format in supportedFormats)
        {
            var check = (Func<IrAttachment, bool>)format["check"];
            if (check(this))
            {
                var decoder = (Func<string, byte[], List<Dictionary<string, object>>>)format["decoder"];
                toProcess.AddRange(decoder(this.Name, this.Raw));
            }
        }

        toProcess.Sort((a, b) => ((int)a["sort_weight"]).CompareTo((int)b["sort_weight"]));

        return toProcess;
    }

    public void ActionDownloadXsdFiles()
    {
        // To be extended by localizations, where they can download their necessary XSD files
        // Note: they should always call base.ActionDownloadXsdFiles() at the end
    }
}
