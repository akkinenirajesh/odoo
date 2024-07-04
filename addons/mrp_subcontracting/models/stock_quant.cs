csharp
public partial class StockQuant
{
    public bool SearchIsSubcontract(string operator, bool value)
    {
        if (operator != "=" && operator != "!=")
        {
            throw new Exception("Operation not supported");
        }

        return Env.Ref("Stock.Location").Search(x => x.IsSubcontractingLocation == value);
    }
}
