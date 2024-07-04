csharp
public partial class Pricelist {
    public void ComputeDisplayName() {
        this.DisplayName = $"{this.Name} ({Env.Get("Res.Currency").Get(this.Currency).Name})";
    }

    public void Write(Dictionary<string, object> values) {
        var res = base.Write(values);

        if (values.ContainsKey("Company") && this.Count == 1) {
            Env.Get("Product.PricelistItem").Get(this.Items).CheckCompany();
        }
        return res;
    }

    public Dictionary<int, float> GetProductsPrice(List<int> productIds, float quantity, int currencyId = 0, int uomId = 0, DateTime? date = null) {
        this.EnsureOne();
        return this.ComputePriceRule(productIds, quantity, currencyId, uomId, date)
            .Select(x => new { x.Key, Price = x.Value.Item1 })
            .ToDictionary(x => x.Key, x => x.Price);
    }

    public float GetProductPrice(int productId, float quantity, int currencyId = 0, int uomId = 0, DateTime? date = null) {
        this.EnsureOne();
        return this.ComputePriceRule(productId, quantity, currencyId, uomId, date).Item1;
    }

    public Tuple<float, int> GetProductPriceRule(int productId, float quantity, int currencyId = 0, int uomId = 0, DateTime? date = null) {
        this.EnsureOne();
        return this.ComputePriceRule(productId, quantity, currencyId, uomId, date);
    }

    public int GetProductRule(int productId, float quantity, int currencyId = 0, int uomId = 0, DateTime? date = null) {
        this.EnsureOne();
        return this.ComputePriceRule(productId, quantity, currencyId, uomId, date, false).Item2;
    }

    public Dictionary<int, Tuple<float, int>> ComputePriceRule(
        List<int> productIds,
        float quantity,
        int currencyId = 0,
        int uomId = 0,
        DateTime? date = null,
        bool computePrice = true) {
        this.EnsureOne();

        var currency = currencyId > 0 ? Env.Get("Res.Currency").Get(currencyId) : Env.Get("Res.Company").Get(this.Company).Currency;
        if (productIds.Count == 0) {
            return new Dictionary<int, Tuple<float, int>>();
        }

        date = date ?? DateTime.Now;

        var rules = this.GetApplicableRules(productIds, date);

        var results = new Dictionary<int, Tuple<float, int>>();
        foreach (var productId in productIds) {
            var product = Env.Get("Product.Product").Get(productId);
            var productUom = Env.Get("Uom.Uom").Get(product.Uom);
            var targetUom = uomId > 0 ? Env.Get("Uom.Uom").Get(uomId) : productUom;

            var qtyInProductUom = targetUom == productUom ? quantity : targetUom.ComputeQuantity(quantity, productUom, false);

            var suitableRule = Env.Get("Product.PricelistItem").GetEmpty();
            foreach (var rule in rules) {
                if (rule.IsApplicableFor(product, qtyInProductUom)) {
                    suitableRule = rule;
                    break;
                }
            }

            if (computePrice) {
                var price = suitableRule.ComputePrice(product, quantity, targetUom, date, currency);
                results.Add(productId, new Tuple<float, int>(price, suitableRule.Id));
            } else {
                results.Add(productId, new Tuple<float, int>(0, suitableRule.Id));
            }
        }
        return results;
    }

    public List<Product.PricelistItem> GetApplicableRules(List<int> productIds, DateTime date) {
        this.EnsureOne();

        var products = Env.Get("Product.Product").Get(productIds);
        if (products.Count == 0) {
            return new List<Product.PricelistItem>();
        }

        // Do not filter out archived pricelist items, since it means current pricelist is also archived
        // We do not want the computation of prices for archived pricelist to always fallback on the Sales price
        // because no rule was found (thanks to the automatic orm filtering on active field)
        return Env.Get("Product.PricelistItem")
            .WithContext(new Dictionary<string, object> { { "active_test", false } })
            .Search(this.GetApplicableRulesDomain(products, date))
            .WithContext(Env.Context);
    }

    public List<object> GetApplicableRulesDomain(List<int> productIds, DateTime date) {
        this.EnsureOne();

        var products = Env.Get("Product.Product").Get(productIds);
        var domain = new List<object>();
        domain.Add(new Tuple<string, object>("Pricelist", this.Id));
        domain.Add(new Tuple<string, object>("Categ", null, "OR", new Tuple<string, object>("Categ", products.Categ.Ids, "parent_of")));
        domain.Add(new Tuple<string, object>("ProductTemplate", null, "OR", new Tuple<string, object>("ProductTemplate", products.ProductTemplate.Ids)));
        domain.Add(new Tuple<string, object>("Product", null, "OR", new Tuple<string, object>("Product", products.Ids)));
        domain.Add(new Tuple<string, object>("DateStart", null, "OR", new Tuple<string, object>("DateStart", date, "<=")));
        domain.Add(new Tuple<string, object>("DateEnd", null, "OR", new Tuple<string, object>("DateEnd", date, ">=")));

        return domain;
    }

    public Dictionary<int, float> PriceGet(int productId, float quantity, int currencyId = 0, int uomId = 0, DateTime? date = null) {
        return this.ComputePriceRuleMulti(productId, quantity, uomId, date)
            .Select(x => new { x.Key, Price = x.Value[this.Id].Item1 })
            .ToDictionary(x => x.Key, x => x.Price);
    }

    public Dictionary<int, Dictionary<int, Tuple<float, int>>> ComputePriceRuleMulti(int productId, float quantity, int uomId = 0, DateTime? date = null, bool computePrice = true) {
        var pricelists = this.Count > 0 ? this : Env.Get("Product.Pricelist").Search();
        var results = new Dictionary<int, Dictionary<int, Tuple<float, int>>>();
        foreach (var pricelist in pricelists) {
            var subres = pricelist.ComputePriceRule(productId, quantity, uomId, date, computePrice);
            results.Add(productId, new Dictionary<int, Tuple<float, int>> { { pricelist.Id, subres } });
        }
        return results;
    }

    public Dictionary<int, int> GetPartnerPricelistMulti(List<int> partnerIds) {
        var company = Env.Get("Res.Company").Get(this.Company);
        var partners = Env.Get("Res.Partner").WithContext(new Dictionary<string, object> { { "active_test", false } }).Get(partnerIds);

        var specificProperties = Env.Get("Ir.Property").WithContext(new Dictionary<string, object> { { "company", company.Id } }).GetMulti(
            "property_product_pricelist", "Res.Partner", partnerIds);

        var result = new Dictionary<int, int>();
        var remainingPartnerIds = new List<int>();
        foreach (var pid in partnerIds) {
            if (specificProperties.ContainsKey(pid) && specificProperties[pid].GetPartnerPricelistMultiFilterHook()) {
                result.Add(pid, specificProperties[pid].Id);
            } else if (specificProperties.ContainsKey(pid.Origin) && specificProperties[pid.Origin].GetPartnerPricelistMultiFilterHook()) {
                result.Add(pid, specificProperties[pid.Origin].Id);
            } else {
                remainingPartnerIds.Add(pid);
            }
        }

        if (remainingPartnerIds.Count > 0) {
            var plFallback = Env.Get("Product.Pricelist").Search(GetPartnerPricelistMultiSearchDomainHook(company.Id), new Dictionary<string, object> { { "limit", 1 } })
                ?? Env.Get("Ir.Property").Get("property_product_pricelist", "Res.Partner")
                ?? Env.Get("Product.Pricelist").Search(GetPartnerPricelistMultiSearchDomainHook(company.Id), new Dictionary<string, object> { { "limit", 1 } });

            var remainingPartners = Env.Get("Res.Partner").Get(remainingPartnerIds);
            var partnersByCountry = remainingPartners.Grouped("Country");
            foreach (var country in partnersByCountry) {
                var pl = Env.Get("Product.Pricelist").Search(
                    GetPartnerPricelistMultiSearchDomainHook(company.Id)
                    .Add(new Tuple<string, object>("CountryGroups.Country", country.Key.Id, "=", true))
                    , new Dictionary<string, object> { { "limit", 1 } })
                ?? plFallback;
                result.Add(country.Value.Ids, pl.Id);
            }
        }

        return result;
    }

    public List<object> GetPartnerPricelistMultiSearchDomainHook(int companyId) {
        return new List<object> {
            new Tuple<string, object>("Active", true),
            new Tuple<string, object>("Company", new List<int> { companyId, 0 })
        };
    }

    public bool GetPartnerPricelistMultiFilterHook() {
        return this.Active;
    }

    public List<Dictionary<string, object>> GetImportTemplates() {
        return new List<Dictionary<string, object>> {
            new Dictionary<string, object> {
                { "label", "Import Template for Pricelists" },
                { "template", "/product/static/xls/product_pricelist.xls" }
            }
        };
    }

    public void UnlinkExceptUsedAsRuleBase() {
        var linkedItems = Env.Get("Product.PricelistItem").WithContext(new Dictionary<string, object> { { "active_test", false } }).Search(
            new List<object> {
                new Tuple<string, object>("Base", "pricelist"),
                new Tuple<string, object>("BasePricelist", this.Ids, "in"),
                new Tuple<string, object>("Pricelist", this.Ids, "not in")
            });
        if (linkedItems.Count > 0) {
            throw new Exception(
                $"You cannot delete pricelist(s):\n{string.Join("\n", linkedItems.BasePricelist.Select(x => x.DisplayName))}\nThey are used within pricelist(s):\n{string.Join("\n", linkedItems.Pricelist.Select(x => x.DisplayName))}");
        }
    }
}
