csharp
public partial class EventTicket
{
    public string GetTicketMultilineDescription()
    {
        if (!string.IsNullOrEmpty(this.Product?.DescriptionSale))
        {
            return $"{this.Product.DescriptionSale}\n{this.Event?.DisplayName}";
        }
        
        // Assuming there's a base implementation in a base class or interface
        return base.GetTicketMultilineDescription();
    }

    public override string ToString()
    {
        // Default string representation
        return this.Name;
    }
}
