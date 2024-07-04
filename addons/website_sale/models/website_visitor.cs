csharp
public partial class WebsiteVisitor {

    public void ComputeProductStatistics() {
        var results = Env.GetModel("Website.Track").ReadGroup(
            new List<object> {
                new Dictionary<string, object> { { "VisitorId", "in", this.Id } },
                new Dictionary<string, object> { { "ProductId", "!=", false } },
                new Dictionary<string, object> { { "ProductId", "any", Env.GetModel("Product.Product").CheckCompanyDomain(Env.Companies) } }
            },
            new List<string> { "VisitorId" },
            new List<string> { "ProductId:array_agg", "__count" }
        );
        var mappedData = new Dictionary<int, Dictionary<string, object>>();
        foreach (var result in results) {
            mappedData.Add(
                (int)result["VisitorId"],
                new Dictionary<string, object> {
                    { "ProductCount", result["__count"] },
                    { "ProductIds", result["ProductId:array_agg"] }
                }
            );
        }

        var visitorInfo = mappedData.GetValueOrDefault(this.Id, new Dictionary<string, object> { { "ProductIds", new List<object>() }, { "ProductCount", 0 } });
        this.ProductIds = (List<object>)visitorInfo["ProductIds"];
        this.VisitorProductCount = (int)visitorInfo["ProductCount"];
        this.ProductCount = ((List<object>)visitorInfo["ProductIds"]).Count;
    }

    public void AddViewedProduct(int productId) {
        if (productId != 0 && Env.GetModel("Product.Product").Browse(productId).IsVariantPossible()) {
            var domain = new List<object> { new Dictionary<string, object> { { "ProductId", "=", productId } } };
            var websiteTrackValues = new Dictionary<string, object> { { "ProductId", productId } };
            AddTracking(domain, websiteTrackValues);
        }
    }

    private void AddTracking(List<object> domain, Dictionary<string, object> values) {
        // TODO: Implement _add_tracking logic here
    }
}
