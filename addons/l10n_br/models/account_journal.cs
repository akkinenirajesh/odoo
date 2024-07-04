csharp
public partial class AccountJournal
{
    public override string ComputeDisplayName()
    {
        string displayName = base.ComputeDisplayName();
        
        if (!string.IsNullOrEmpty(this.L10nBrInvoiceSerial))
        {
            displayName = $"{this.L10nBrInvoiceSerial}-{displayName}";
        }

        return displayName;
    }
}
