csharp
public partial class AccountMove
{
    public string InvoiceReference { get; set; }
    public string KidNumber { get; set; }

    public void _GetInvoiceReferenceNoInvoice()
    {
        InvoiceReference = _GetKidNumber();
    }

    public void _GetInvoiceReferenceNoPartner()
    {
        InvoiceReference = _GetKidNumber();
    }

    public string _GetKidNumber()
    {
        string invoiceName = new string(Name.Where(char.IsDigit).ToArray()).PadLeft(7, '0');
        string partnerIdStr = PartnerId.Id.ToString().PadLeft(7, '0').Substring(PartnerId.Id.ToString().Length - 7);
        string refStr = partnerIdStr + invoiceName.Substring(invoiceName.Length - 7);
        return refStr + luhn.calc_check_digit(refStr);
    }
}
