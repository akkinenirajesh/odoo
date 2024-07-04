csharp
public partial class SaleOrderLine {
    public virtual float ProductQuantity { get; set; }
    public virtual float ProductPrice { get; set; }
    public virtual float Discount { get; set; }
    public virtual ICollection<AccountTax> TaxIDs { get; set; }
    public virtual ProductUom ProductUom { get; set; }
    public virtual string ProductDescription { get; set; }
    public virtual ProductProduct Product { get; set; }
    public virtual float PriceSubtotal { get; set; }
    public virtual float PriceTotal { get; set; }
    public virtual SaleOrder SaleOrder { get; set; }
    public virtual string SaleOrderState { get; set; }
    public virtual DateTime? DeliveryDate { get; set; }

    public SaleOrderLine()
    {
        TaxIDs = new List<AccountTax>();
    }

    public virtual void CalculatePriceSubtotal()
    {
        // C# code to calculate PriceSubtotal
        // Using this.ProductQuantity, this.ProductPrice, this.Discount
        // Set PriceSubtotal = calculated value
    }

    public virtual void CalculatePriceTotal()
    {
        // C# code to calculate PriceTotal
        // Using this.PriceSubtotal, this.TaxIDs, ...
        // Set PriceTotal = calculated value
    }
}
