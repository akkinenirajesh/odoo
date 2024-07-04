csharp
public partial class SaleReport {
    public virtual decimal Margin { get; set; }

    public virtual decimal _SelectAdditionalFields() {
        decimal res = Env.Call<decimal>("sale.report", "_select_additional_fields");
        res = (decimal)Env.Call("sale.report", "_select_additional_fields", "margin", $"SUM(l.margin / {Env.Call<decimal>("sale.report", "_case_value_or_one", "s.currency_rate")} * {Env.Call<decimal>("sale.report", "_case_value_or_one", "currency_table.rate")})");
        return res;
    }
}
