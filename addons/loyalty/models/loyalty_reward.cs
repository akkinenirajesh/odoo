csharp
public partial class LoyaltyReward {

    public bool Active { get; set; }
    public LoyaltyProgram ProgramId { get; set; }
    public string ProgramType { get; set; }
    public CoreCompany CompanyId { get; set; }
    public CoreCurrency CurrencyId { get; set; }
    public string Description { get; set; }
    public string RewardType { get; set; }
    public bool UserHasDebug { get; set; }
    public float Discount { get; set; }
    public string DiscountMode { get; set; }
    public string DiscountApplicability { get; set; }
    public string DiscountProductDomain { get; set; }
    public List<ProductProduct> DiscountProductIds { get; set; }
    public ProductProductCategory DiscountProductCategoryId { get; set; }
    public ProductProductTag DiscountProductTagId { get; set; }
    public List<ProductProduct> AllDiscountProductIds { get; set; }
    public string RewardProductDomain { get; set; }
    public decimal DiscountMaxAmount { get; set; }
    public List<ProductProduct> DiscountLineProductId { get; set; }
    public bool IsGlobalDiscount { get; set; }
    public ProductProduct RewardProductId { get; set; }
    public ProductProductTag RewardProductTagId { get; set; }
    public bool MultiProduct { get; set; }
    public List<ProductProduct> RewardProductIds { get; set; }
    public int RewardProductQty { get; set; }
    public UomUom RewardProductUomId { get; set; }
    public float RequiredPoints { get; set; }
    public string PointName { get; set; }
    public bool ClearWallet { get; set; }

    public void ComputeUserHasDebug() {
        this.UserHasDebug = Env.User.HasGroup("base.group_no_one");
    }

    public void ComputeAllDiscountProductIds() {
        string computeAllDiscountProduct = Env.IrConfigParameter.GetParam("loyalty.compute_all_discount_product_ids", "enabled");
        if (computeAllDiscountProduct == "enabled") {
            this.AllDiscountProductIds = Env.ProductProduct.Search(GetDiscountProductDomain());
        }
        else {
            this.AllDiscountProductIds = new List<ProductProduct>();
        }
    }

    public void ComputeRewardProductDomain() {
        string computeAllDiscountProduct = Env.IrConfigParameter.GetParam("loyalty.compute_all_discount_product_ids", "enabled");
        if (computeAllDiscountProduct == "enabled") {
            this.RewardProductDomain = "null";
        }
        else {
            this.RewardProductDomain = Json.Serialize(GetDiscountProductDomain());
        }
    }

    public void ComputeMultiProduct() {
        List<ProductProduct> products = new List<ProductProduct>(this.RewardProductId);
        products.AddRange(this.RewardProductTagId.ProductIds);
        this.MultiProduct = this.RewardType == "product" && products.Count > 1;
        this.RewardProductIds = this.RewardType == "product" ? products : new List<ProductProduct>();
    }

    public void ComputeRewardProductUomId() {
        this.RewardProductUomId = this.RewardProductIds.FirstOrDefault()?.ProductTmplId.UomId;
    }

    public void ComputeIsGlobalDiscount() {
        this.IsGlobalDiscount = this.RewardType == "discount" && this.DiscountApplicability == "order" &&
            (this.DiscountMode == "perOrder" || this.DiscountMode == "percent");
    }

    public List<object[]> GetDiscountProductDomain() {
        List<object[]> constrains = new List<object[]>();
        if (this.DiscountProductIds != null) {
            constrains.Add(new object[] { "Id", "in", this.DiscountProductIds.Select(p => p.Id).ToList() });
        }
        if (this.DiscountProductCategoryId != null) {
            List<int> productCategoryIds = FindAllCategoryChildren(this.DiscountProductCategoryId, new List<int>());
            productCategoryIds.Add(this.DiscountProductCategoryId.Id);
            constrains.Add(new object[] { "CategId", "in", productCategoryIds });
        }
        if (this.DiscountProductTagId != null) {
            constrains.Add(new object[] { "AllProductTagIds", "in", this.DiscountProductTagId.Id });
        }
        if (constrains.Count > 0) {
            return new List<object[]> { constrains };
        }
        else {
            return new List<object[]>();
        }
    }

    private List<int> FindAllCategoryChildren(ProductProductCategory category, List<int> childIds) {
        if (category.ChildId.Count > 0) {
            foreach (ProductProductCategory child in category.ChildId) {
                childIds.Add(child.Id);
                FindAllCategoryChildren(child, childIds);
            }
        }
        return childIds;
    }

    public void CreateMissingDiscountLineProducts() {
        if (this.DiscountLineProductId == null) {
            var product = Env.ProductProduct.Create(GetDiscountProductValues());
            this.DiscountLineProductId = new List<ProductProduct> { product };
        }
    }

    public List<object> GetDiscountProductValues() {
        return new List<object> { new Dictionary<string, object> {
            {"Name", this.Description},
            {"Type", "service"},
            {"SaleOk", false},
            {"PurchaseOk", false},
            {"LstPrice", 0},
        }};
    }
}
