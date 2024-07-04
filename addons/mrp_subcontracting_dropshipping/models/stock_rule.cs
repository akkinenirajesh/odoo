csharp
public partial class StockRule
{
    public StockRule PreparePurchaseOrder(Core.Company company, List<string> origins, List<Dictionary<string, object>> values)
    {
        if (!values[0].ContainsKey("PartnerId") && (company.SubcontractingLocationId.ParentPath.Contains(this.LocationDestId.ParentPath) || this.LocationDestId.IsSubcontractingLocation))
        {
            values[0]["PartnerId"] = values[0]["GroupId"].PartnerId.Id;
        }
        return Env.Call<StockRule>("_prepare_purchase_order", company.Id, origins, values);
    }

    public List<Dictionary<string, object>> MakePOGetDomain(Core.Company company, List<Dictionary<string, object>> values, res.Partner partner)
    {
        List<Dictionary<string, object>> domain = Env.Call<List<Dictionary<string, object>>>("_make_po_get_domain", company.Id, values, partner);
        if (values.ContainsKey("PartnerId"))
        {
            domain.Add(new Dictionary<string, object> { { "DestAddressId", values["PartnerId"] } });
        }
        return domain;
    }
}
