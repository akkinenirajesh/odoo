csharp
public partial class AccountMove
{
    public void ComputeL10nEsIsSimplified()
    {
        base.ComputeL10nEsIsSimplified();
        
        if (PosOrderIds.Any())
        {
            L10nEsIsSimplified = PosOrderIds.First().IsL10nEsSimplifiedInvoice;
        }
    }
}
