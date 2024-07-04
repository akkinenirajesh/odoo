csharp
public partial class AccountMove
{
    public void ComputeL10nArWithholdingIds()
    {
        this.L10nArWithholdingIds = this.LineIds.Where(l => l.TaxLineId.L10nArWithholdingPaymentType).ToList();
    }
}
