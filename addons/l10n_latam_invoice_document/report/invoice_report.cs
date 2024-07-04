csharp
public partial class InvoiceReport
{
    public override string ToString()
    {
        // Implement string representation logic here
        return base.ToString();
    }

    protected override SQL Select()
    {
        return new SQL($"{base.Select()}, Move.LatamDocumentType as LatamDocumentType");
    }
}
