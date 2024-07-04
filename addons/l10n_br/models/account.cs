csharp
public partial class AccountTax
{
    public override string ToString()
    {
        // You might want to return a meaningful string representation of the AccountTax
        return $"AccountTax: Discount={TaxDiscount}, Reduction={BaseReduction}, MVA={AmountMva}";
    }

    // You can add other custom methods or properties here if needed
}
