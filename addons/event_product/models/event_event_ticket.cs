csharp
public partial class EventTicket
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeSaleAvailable()
    {
        if (!this.Product.Active)
        {
            this.SaleAvailable = false;
        }
        else
        {
            // Implement the logic for super()._compute_sale_available() here
        }
    }

    public void ComputePriceReduceTaxInc()
    {
        var taxIds = this.Product.TaxIds.Where(r => r.Company == this.Event.Company);
        var taxes = taxIds.ComputeAll(this.PriceReduce, this.Event.Company.Currency, 1.0m, this.Product);
        this.PriceReduceTaxInc = taxes.TotalIncluded;
    }

    public void ComputePriceIncl()
    {
        if (this.Product != null && this.Price != 0)
        {
            var taxIds = this.Product.TaxIds.Where(r => r.Company == this.Event.Company);
            var taxes = taxIds.ComputeAll(this.Price, this.Currency, 1.0m, this.Product);
            this.PriceIncl = taxes.TotalIncluded;
        }
        else
        {
            this.PriceIncl = 0;
        }
    }
}
