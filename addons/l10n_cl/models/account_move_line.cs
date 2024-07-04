csharp
public partial class AccountMoveLine
{
    public Dictionary<string, object> L10nClPricesAndTaxes()
    {
        var invoice = Move;
        var includedTaxes = invoice.L10nClIncludeSii() ? TaxIds.Where(x => x.L10nClSiiCode == 14) : TaxIds;

        decimal priceUnit, priceSubtotal, priceNet;

        if (!includedTaxes.Any())
        {
            var priceUnitCalculation = TaxIds.ComputeAll(PriceUnit, invoice.Currency, 1.0m, Product, invoice.Partner);
            priceUnit = priceUnitCalculation["total_excluded"];
            priceSubtotal = PriceSubtotal;
        }
        else
        {
            var priceUnitCalculation = includedTaxes.ComputeAll(PriceUnit, invoice.Currency, 1.0m, Product, invoice.Partner);
            priceUnit = priceUnitCalculation["total_included"];
            decimal price = PriceUnit * (1 - (Discount ?? 0.0m) / 100.0m);
            var priceSubtotalCalculation = includedTaxes.ComputeAll(price, invoice.Currency, Quantity, Product, invoice.Partner);
            priceSubtotal = priceSubtotalCalculation["total_included"];
        }

        priceNet = priceUnit * (1 - (Discount ?? 0.0m) / 100.0m);

        return new Dictionary<string, object>
        {
            ["price_unit"] = priceUnit,
            ["price_subtotal"] = priceSubtotal,
            ["price_net"] = priceNet
        };
    }

    public Dictionary<string, object> L10nClGetLineAmounts()
    {
        if (DisplayType != "product")
        {
            return new Dictionary<string, object> { ["price_subtotal"] = 0 };
        }

        decimal lineSign = PriceSubtotal != 0 ? Math.Sign(PriceSubtotal) : 0;
        bool domesticInvoiceOtherCurrency = Move.Currency != Move.Company.Currency && !Move.L10nLatamDocumentType.IsDocTypeExport();
        bool export = Move.L10nLatamDocumentType.IsDocTypeExport();

        Core.Currency mainCurrency;
        string mainCurrencyField;
        string secondCurrencyField;
        Core.Currency secondCurrency;
        decimal mainCurrencyRate;
        decimal secondCurrencyRate;
        decimal inverseRate;

        if (!export)
        {
            mainCurrency = Move.Company.Currency;
            mainCurrencyField = "Balance";
            secondCurrencyField = "PriceSubtotal";
            secondCurrency = Currency;
            mainCurrencyRate = 1;
            secondCurrencyRate = domesticInvoiceOtherCurrency ? Math.Abs(Balance) / PriceSubtotal : 0;
            inverseRate = domesticInvoiceOtherCurrency ? secondCurrencyRate : mainCurrencyRate;
        }
        else
        {
            mainCurrency = Currency;
            secondCurrency = Move.Company.Currency;
            mainCurrencyField = "PriceSubtotal";
            secondCurrencyField = "Balance";
            inverseRate = Math.Abs(Balance) / PriceSubtotal;
        }

        decimal priceSubtotal = Math.Abs((decimal)GetType().GetProperty(mainCurrencyField).GetValue(this)) * lineSign;
        decimal priceUnit, priceItemDocument, priceLineDocument;

        if (Quantity != 0 && Discount != 100.0m)
        {
            priceUnit = (priceSubtotal / Math.Abs(Quantity)) / (1 - Discount / 100);
            if (Move.L10nLatamDocumentType.IsDocTypeElectronicTicket())
            {
                priceItemDocument = (PriceTotal / Math.Abs(Quantity)) / (1 - Discount / 100);
                priceLineDocument = PriceTotal;
            }
            else
            {
                priceItemDocument = priceUnit;
                priceLineDocument = priceSubtotal;
            }
        }
        else
        {
            priceItemDocument = priceLineDocument = 0.0m;
            priceUnit = PriceUnit;
        }

        decimal discountAmount = (priceSubtotal / (1 - Discount / 100)) * Discount / 100;

        var values = new Dictionary<string, object>
        {
            ["decimal_places"] = mainCurrency.DecimalPlaces,
            ["price_item"] = Math.Round(priceUnit, 6),
            ["price_item_document"] = Math.Round(priceItemDocument, 2),
            ["price_line_document"] = priceLineDocument,
            ["total_discount"] = mainCurrency.Round(discountAmount),
            ["price_subtotal"] = mainCurrency.Round(priceSubtotal),
            ["exempt"] = !TaxIds.Any(),
            ["main_currency"] = mainCurrency
        };

        if (domesticInvoiceOtherCurrency || export)
        {
            decimal priceSubtotalSecond = Math.Abs((decimal)GetType().GetProperty(secondCurrencyField).GetValue(this)) * lineSign;
            decimal priceUnitSecond = (Quantity != 0 && Discount != 100.0m)
                ? (priceSubtotalSecond / Math.Abs(Quantity)) / (1 - Discount / 100)
                : PriceUnit;
            decimal discountAmountSecond = priceUnitSecond * Quantity - priceSubtotalSecond;

            values["second_currency"] = new Dictionary<string, object>
            {
                ["price"] = secondCurrency.Round(priceUnitSecond),
                ["currency_name"] = Move.FormatLength(secondCurrency.Name, 3),
                ["conversion_rate"] = Math.Round(inverseRate, 4),
                ["amount_discount"] = secondCurrency.Round(discountAmountSecond),
                ["total_amount"] = secondCurrency.Round(priceSubtotalSecond),
                ["round_currency"] = secondCurrency.DecimalPlaces
            };
        }

        values["line_description"] = values.ContainsKey("second_currency") && !Move.L10nLatamDocumentType.IsDocTypeExport()
            ? $"{Name} ({((Dictionary<string, object>)values["second_currency"])["currency_name"]}: {FloatRepr(((Dictionary<string, object>)values["second_currency"])["price"], ((Dictionary<string, object>)values["second_currency"])["round_currency"])} @ {Move.FloatReprFloatRound(((Dictionary<string, object>)values["second_currency"])["conversion_rate"], ((Dictionary<string, object>)values["second_currency"])["round_currency"])})"
            : Name;

        return values;
    }

    private string FloatRepr(object value, object decimalPlaces)
    {
        // Implement the float_repr function here
        throw new NotImplementedException();
    }
}
