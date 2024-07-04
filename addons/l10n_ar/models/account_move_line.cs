csharp
public partial class AccountMoveLine
{
    public PricesAndTaxes L10nArPricesAndTaxes()
    {
        var invoice = this.Move;
        var includedTaxes = invoice.L10nArIncludeVat() 
            ? this.TaxIds.Where(t => t.TaxGroup.L10nArVatAfipCode != null).ToList() 
            : null;

        decimal priceUnit, priceSubtotal, priceNet;

        if (includedTaxes == null || !includedTaxes.Any())
        {
            var priceComputation = Env.TaxService.ComputeAll(this.TaxIds, this.PriceUnit, invoice.Currency, 1.0m, this.Product, invoice.Partner);
            priceUnit = priceComputation.TotalExcluded;
            priceSubtotal = this.PriceSubtotal;
        }
        else
        {
            var priceComputation = Env.TaxService.ComputeAll(includedTaxes, this.PriceUnit, invoice.Currency, 1.0m, this.Product, invoice.Partner);
            priceUnit = priceComputation.TotalIncluded;
            var price = this.PriceUnit * (1 - (this.Discount ?? 0.0m) / 100.0m);
            var subtotalComputation = Env.TaxService.ComputeAll(includedTaxes, price, invoice.Currency, this.Quantity, this.Product, invoice.Partner);
            priceSubtotal = subtotalComputation.TotalIncluded;
        }

        priceNet = priceUnit * (1 - (this.Discount ?? 0.0m) / 100.0m);

        return new PricesAndTaxes
        {
            PriceUnit = priceUnit,
            PriceSubtotal = priceSubtotal,
            PriceNet = priceNet
        };
    }

    public class PricesAndTaxes
    {
        public decimal PriceUnit { get; set; }
        public decimal PriceSubtotal { get; set; }
        public decimal PriceNet { get; set; }
    }
}
