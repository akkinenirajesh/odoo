csharp
public partial class PosOrder {
    public void _PrepareInvoiceVals() {
        var vals = Env.Call("POSOrder", "_PrepareInvoiceVals", this);
        if (Env.Call("Core.Company", "GetCountry", this.CompanyId).Code == "SA") {
            vals.Add("L10nSaConfirmationDatetime", this.DateOrder);
        }
        return vals;
    }
}
