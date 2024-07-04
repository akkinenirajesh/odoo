csharp
public partial class L10nLatamDocumentType
{
    public string FormatDocumentNumber(string documentNumber)
    {
        if (this.Country.Code != "CL")
        {
            return base.FormatDocumentNumber(documentNumber);
        }

        if (string.IsNullOrEmpty(documentNumber))
        {
            return null;
        }

        return documentNumber.PadLeft(6, '0');
    }

    public bool IsDocTypeVendor()
    {
        return this.Code == "46";
    }

    public bool IsDocTypeExport()
    {
        return (this.Code == "110" || this.Code == "111" || this.Code == "112") && this.Country.Code == "CL";
    }

    public bool IsDocTypeElectronicTicket()
    {
        return (this.Code == "39" || this.Code == "41") && this.Country.Code == "CL";
    }
}
