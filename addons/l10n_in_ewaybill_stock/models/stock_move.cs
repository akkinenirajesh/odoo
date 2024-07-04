csharp
public partial class StockMove
{
    public void ComputeL10nInEwaybillPriceUnit()
    {
        if (L10nInEwaybillId?.State == "pending" && PickingId?.CountryCode == "IN")
        {
            EwaybillPriceUnit = L10nInGetProductPriceUnit();
        }
    }

    public void ComputeL10nInTaxIds()
    {
        if (L10nInEwaybillId?.State == "pending" && PickingId?.CountryCode == "IN")
        {
            var taxesDetails = L10nInGetProductTax();
            var taxes = taxesDetails.Taxes;
            if (taxesDetails.IsFromOrder)
            {
                // Don't map taxes if they are from sale/purchase order
                EwaybillTaxIds = taxes;
            }
            else
            {
                var fiscalPosition = L10nInEwaybillId?.FiscalPositionId;
                if (fiscalPosition != null)
                {
                    taxes = fiscalPosition.MapTax(taxes);
                }
                EwaybillTaxIds = taxes.Where(t => Env.AccountTax.CheckCompanyDomain(CompanyId, t)).ToList();
            }
        }
    }

    private decimal L10nInGetProductPriceUnit()
    {
        // Implementation for getting product price unit
        throw new NotImplementedException();
    }

    private (List<Account.Tax> Taxes, bool IsFromOrder) L10nInGetProductTax()
    {
        // Implementation for getting product tax
        throw new NotImplementedException();
    }
}
