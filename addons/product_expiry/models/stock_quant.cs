csharp
public partial class StockQuant
{
    public string _get_gs1_barcode(Dictionary<string, string> gs1_quantity_rules_ai_by_uom)
    {
        string barcode = Env.Call<string>("StockQuant", "_get_gs1_barcode", this, gs1_quantity_rules_ai_by_uom);
        if (UseExpirationDate)
        {
            if (LotId.ExpirationDate != null)
            {
                barcode = "17" + LotId.ExpirationDate.ToString("yyMMdd") + barcode;
            }
            if (LotId.UseDate != null)
            {
                barcode = "15" + LotId.UseDate.ToString("yyMMdd") + barcode;
            }
        }
        return barcode;
    }

    public string _get_removal_strategy_order(string removalStrategy)
    {
        if (removalStrategy == "fefo")
        {
            return "removal_date, in_date, id";
        }
        return Env.Call<string>("StockQuant", "_get_removal_strategy_order", this, removalStrategy);
    }
}
