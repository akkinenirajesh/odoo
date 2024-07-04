csharp
public partial class PriceRule
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeName()
    {
        string name = $"if {Variable} {Operator} {MaxValue:F2} then";
        string basePrice, price;

        if (CurrencyId != null)
        {
            basePrice = Env.FormatAmount(ListBasePrice, CurrencyId);
            price = Env.FormatAmount(ListPrice, CurrencyId);
        }
        else
        {
            basePrice = $"{ListBasePrice:F2}";
            price = $"{ListPrice:F2}";
        }

        if (ListBasePrice != 0 && ListPrice == 0)
        {
            name = $"{name} fixed price {basePrice}";
        }
        else if (ListPrice != 0 && ListBasePrice == 0)
        {
            name = $"{name} {price} times {VariableFactor}";
        }
        else
        {
            name = $"{name} fixed price {basePrice} plus {price} times {VariableFactor}";
        }

        Name = name;
    }
}
