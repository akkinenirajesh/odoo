csharp
public partial class AccountTax 
{
    public override string ToString()
    {
        return Name;
    }

    public AccountTax ComputeAll(decimal priceUnit, Core.Currency currency = null, decimal quantity = 1.0m, Core.Product product = null, Core.Partner partner = null, bool isRefund = false, bool handlePriceInclude = true, bool includeCabaTags = false)
    {
        // Implementation of ComputeAll method
        // This method would need to be implemented to match the logic in the Python code
        // It should return a new AccountTax object with the computed values
        throw new NotImplementedException();
    }

    public List<AccountTax> FlattenTaxesHierarchy()
    {
        // Implementation of FlattenTaxesHierarchy method
        // This method would need to be implemented to match the logic in the Python code
        // It should return a List<AccountTax> with the flattened tax hierarchy
        throw new NotImplementedException();
    }

    public List<Core.AccountAccountTag> GetTaxTags(bool isRefund, string repartitionType)
    {
        // Implementation of GetTaxTags method
        // This method would need to be implemented to match the logic in the Python code
        // It should return a List<Core.AccountAccountTag> with the tax tags
        throw new NotImplementedException();
    }
}
