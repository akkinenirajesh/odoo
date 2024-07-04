csharp
public partial class Mrp.StockQuant 
{
    public void CheckKits()
    {
        if (this.ProductId.Any(p => p.IsKits)) 
        {
            throw new UserError("You should update the components quantity instead of directly updating the quantity of the kit product.");
        }
    }
}
