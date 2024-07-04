csharp
public partial class WebsiteSaleProductTemplateAttributeValue {
    public decimal GetExtraPrice(Dictionary<string, object> combinationInfo) {
        if (this.PriceExtra == 0) {
            return 0;
        }

        decimal priceExtra = this.PriceExtra;
        if (priceExtra == 0) {
            return priceExtra;
        }

        WebsiteSaleProductTemplate productTemplate = Env.Ref<WebsiteSaleProductTemplate>(this.ProductTmplId);
        ResCurrency currency = Env.Ref<ResCurrency>(this.CurrencyId);
        if (currency != productTemplate.CurrencyId) {
            priceExtra = currency.Convert(
                fromAmount: priceExtra,
                toCurrency: currency,
                company: Env.Company,
                date: combinationInfo["date"]
            );
        }

        List<AccountTax> productTaxes = (List<AccountTax>)combinationInfo["product_taxes"];
        if (productTaxes != null) {
            priceExtra = Env.Ref<WebsiteSaleProductTemplate>().ApplyTaxesToPrice(
                priceExtra,
                currency,
                productTaxes,
                (List<AccountTax>)combinationInfo["taxes"],
                productTemplate
            );
        }

        return priceExtra;
    }
}
