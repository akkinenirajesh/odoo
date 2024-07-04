csharp
public partial class SalePdfQuoteBuilder.ProductDocument 
{
    public void CheckAttachedOnAndDatasCompatibility()
    {
        if (this.AttachedOnSale == "inside")
        {
            if (this.Type != "binary")
            {
                throw new Exception("When attached inside a quote, the document must be a file, not a URL.");
            }
            if (!string.IsNullOrEmpty(this.Datas) && !this.Mimetype.EndsWith("pdf"))
            {
                throw new Exception("Only PDF documents can be attached inside a quote.");
            }
            if (!string.IsNullOrEmpty(this.Datas))
            {
                // Here you need to implement logic for base64 decoding and encryption check
                // You can use the .NET libraries for that
                // Example:
                // byte[] decodedData = Convert.FromBase64String(this.Datas);
                // ... your encryption check logic ...
            }
        }
    }

    public string ComputeDocumentUrl()
    {
        // Implement the logic to calculate the DocumentURL based on the document's type and attributes
        // You might need to access other model data or use the Env to access services
        // Example:
        // if (this.Type == "binary")
        // {
        //     return this.Env.Get("Website").BaseUrl + "/web/content/" + this.Id + "/" + this.Datas;
        // }
        // ... your logic for other document types ...
        return "";
    }

    public string ComputeDocumentName()
    {
        // Implement the logic to calculate the DocumentName based on the document's attributes
        // Example:
        // return this.Name;
        return "";
    }

    public int ComputeDocumentSize()
    {
        // Implement the logic to calculate the DocumentSize based on the document's attributes
        // Example:
        // if (!string.IsNullOrEmpty(this.Datas))
        // {
        //     byte[] decodedData = Convert.FromBase64String(this.Datas);
        //     return decodedData.Length;
        // }
        return 0;
    }
}
