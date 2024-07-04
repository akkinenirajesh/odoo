C#
public partial class PosLoyalty.ProductProduct
{
    public virtual List<Account.AccountTag> AllProductTagIds { get; set; }

    public virtual List<object> LoadPosDataFields(Pos.Config configId)
    {
        List<object> params = Env.Call("PosLoyalty.ProductProduct", "_LoadPosDataFields", configId);
        params.Add("AllProductTagIds");

        List<string> missingFields = Env.Call("Loyalty.Reward", "_GetRewardProductDomainFields", configId).Except(params).ToList();

        if (missingFields.Any())
        {
            params.AddRange(missingFields.Where(field => this.GetType().GetProperty(field) != null));
        }

        return params;
    }

    public virtual Dictionary<string, object> LoadPosData(Dictionary<string, object> data)
    {
        Dictionary<string, object> res = Env.Call("PosLoyalty.ProductProduct", "_LoadPosData", data);
        Pos.Config configId = Env.Call<Pos.Config>("Pos.Config", "Browse", data["pos.config"]["data"][0]["id"]);
        List<Loyalty.Reward> rewards = configId.GetProgramIds().RewardIds;
        List<Product.Product> rewardProducts = rewards.DiscountLineProductId.Union(rewards.RewardProductIds).Union(rewards.RewardProductId).ToList();
        List<Product.Product> triggerProducts = configId.GetProgramIds().Where(p => p.ProgramType == "ewallet" || p.ProgramType == "gift_card").SelectMany(p => p.TriggerProductIds).ToList();

        List<int> loyaltyProductIds = new List<int>(rewardProducts.Select(p => p.Id).Union(triggerProducts.Select(p => p.Id)));
        List<int> classicProductIds = res["data"].Cast<Dictionary<string, object>>().Select(p => (int)p["id"]).ToList();
        List<Product.Product> products = Env.Call<List<Product.Product>>("Product.Product", "Browse", loyaltyProductIds.Except(classicProductIds).ToList());
        products = products.Read(res["fields"], false);
        ProcessPosUiProductProduct(products, configId);

        res["pos.session"]["data"][0]["_pos_special_products_ids"].AddRange(rewardProducts.Where(p => !res["data"].Cast<Dictionary<string, object>>().Any(d => (int)d["id"] == p.Id)).Select(p => p.Id));
        res["data"].AddRange(products);

        return res;
    }

    private void ProcessPosUiProductProduct(List<Product.Product> products, Pos.Config configId)
    {
        // Implement the logic from the original _process_pos_ui_product_product method
    }
}
