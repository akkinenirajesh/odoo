csharp
public partial class SaleOrder
{
    public void ActionQuotationSend()
    {
        L10nItEdiDoiCheckConfiguration();
        // Call base method
    }

    public void ActionQuotationSent()
    {
        L10nItEdiDoiCheckConfiguration();
        // Call base method
    }

    public void ActionConfirm()
    {
        L10nItEdiDoiCheckConfiguration();
        // Call base method
    }

    public void L10nItEdiDoiCheckConfiguration()
    {
        var errors = new List<string>();
        var declaration = L10nItEdiDoiId;
        if (declaration != null)
        {
            var validityWarnings = declaration.GetValidityWarnings(
                Company, Partner.CommercialPartnerId, Currency, L10nItEdiDoiDate,
                onlyBlocking: true, salesOrder: true
            );
            errors.AddRange(validityWarnings);
        }

        var declarationOfIntentTax = Company.L10nItEdiDoiTaxId;
        if (declarationOfIntentTax != null)
        {
            var declarationTaxLines = OrderLine.Where(line => line.TaxId.Contains(declarationOfIntentTax));
            if (declarationTaxLines.Any() && L10nItEdiDoiId == null)
            {
                errors.Add($"Given the tax {declarationOfIntentTax.Name} is applied, there should be a Declaration of Intent selected.");
            }
            if (declarationTaxLines.Any(line => line.TaxId != declarationOfIntentTax))
            {
                errors.Add($"A line using tax {declarationOfIntentTax.Name} should not contain any other taxes");
            }
        }

        if (errors.Any())
        {
            throw new UserException(string.Join("\n", errors));
        }
    }

    public void ActionOpenDeclarationOfIntent()
    {
        return new
        {
            name = $"Declaration of Intent for {DisplayName}",
            type = "ir.actions.act_window",
            viewMode = "form",
            resModel = "SalesOrder.DeclarationOfIntent",
            resId = L10nItEdiDoiId.Id
        };
    }

    public decimal L10nItEdiDoiGetAmountNotYetInvoiced(DeclarationOfIntent declaration, Dictionary<int, float> additionalInvoicedQty = null)
    {
        if (declaration == null)
            return 0;

        additionalInvoicedQty ??= new Dictionary<int, float>();

        var tax = declaration.Company.L10nItEdiDoiTaxId;
        if (tax == null)
            return 0;

        decimal notYetInvoiced = 0;
        if (declaration == L10nItEdiDoiId)
        {
            var orderLines = OrderLine.Where(line => line.TaxId.SequenceEqual(new[] { tax }));
            decimal orderNotYetInvoiced = 0;
            foreach (var line in orderLines)
            {
                var priceReduce = line.PriceUnit * (1 - (line.Discount ?? 0.0m) / 100.0m);
                var qtyInvoiced = line.QtyInvoicedPosted;
                if (additionalInvoicedQty.TryGetValue(line.Id, out float additionalQty))
                {
                    qtyInvoiced += additionalQty;
                }
                var qtyToInvoice = line.ProductUomQty - qtyInvoiced;
                orderNotYetInvoiced += priceReduce * qtyToInvoice;
            }
            if (declaration.Currency.CompareAmounts(orderNotYetInvoiced, 0) > 0)
            {
                notYetInvoiced += orderNotYetInvoiced;
            }
        }

        return notYetInvoiced;
    }
}
