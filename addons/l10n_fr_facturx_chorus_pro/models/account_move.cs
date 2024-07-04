csharp
public partial class AccountMove
{
    public override string ToString()
    {
        // Example implementation, adjust as needed
        return $"Move: {BuyerReference ?? ContractReference ?? PurchaseOrderReference ?? "Unnamed"}";
    }

    // You can add additional methods or properties here if needed
}
