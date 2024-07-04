C#
public partial class LunchProductCategory 
{
    public virtual byte[] defaultImage { get; set; }
    public virtual string Name { get; set; }
    public virtual Res.Company CompanyId { get; set; }
    public virtual Res.Currency CurrencyId { get; set; }
    public virtual int ProductCount { get; set; }
    public virtual bool Active { get; set; }
    public virtual byte[] Image1920 { get; set; }

    public virtual void ComputeProductCount()
    {
        var productData = Env.GetModel("Lunch.Product").ReadGroup(new[] { new KeyValuePair<string, object>("CategoryId", this.Id) }, new[] { "CategoryId" }, new[] { "__count" });
        var data = productData.Select((x, i) => new KeyValuePair<int, int>((int)x["CategoryId"], (int)x["__count"])).ToDictionary(x => x.Key, x => x.Value);
        ProductCount = data.TryGetValue(Id, out int count) ? count : 0;
    }

    public virtual void ToggleActive()
    {
        var res = Env.CallMethod("toggleActive", new object[] { this });
        var products = Env.GetModel("Lunch.Product").Search(new[] { new KeyValuePair<string, object>("CategoryId", this.Id) }, new Dictionary<string, object>() { { "activeTest", false } });
        products.SyncActiveFromRelated();
    }
}
