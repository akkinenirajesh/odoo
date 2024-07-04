C#
public partial class ProductTemplate {
    public bool AvailableInPos { get; set; }
    public bool ToWeight { get; set; }
    public ICollection<PosCategory> PosCategIds { get; set; }
    public ICollection<PosCombo> ComboIds { get; set; }
    public string Type { get; set; }
    public void OnDelete(bool AtUninstall) {
        if (AtUninstall) return;
        // Check if product is available in POS and if any POS session is open
        if (Env.Context.ActiveTest) {
            if (Env.SearchCount<ProductTemplate>(x => x.AvailableInPos && x.Id == this.Id) > 0) {
                if (Env.SearchCount<PosSession>(x => x.State != "closed") > 0) {
                    throw new UserError("To delete a product, make sure all point of sale sessions are closed.\n\n"
                                            + "Deleting a product available in a session would be like attempting to snatch a"
                                            + "hamburger from a customer’s hand mid-bite; chaos will ensue as ketchup and mayo go flying everywhere!");
                }
            }
        }
    }
    public string PrepareTooltip() {
        string tooltip = base.PrepareTooltip();
        if (this.Type == "combo") {
            tooltip = "Combos allows to choose one product amongst a selection of choices per category.";
        }
        return tooltip;
    }
    public void OnChangeSaleOk() {
        if (!this.SaleOk) {
            this.AvailableInPos = false;
        }
    }
    public void OnChangeAvailableInPos() {
        if (this.AvailableInPos && !this.SaleOk) {
            this.SaleOk = true;
        }
    }
    public void OnChangeType() {
        base.OnChangeType();
        if (this.Type == "combo") {
            this.TaxesId = null;
            this.SupplierTaxesId = null;
        }
    }
    public void CheckComboInclusions() {
        if (!this.AvailableInPos) {
            var comboName = Env.Search<PosComboLine>(x => x.ProductId == this.ProductVariantIds.First().Id).First().ComboId.Name;
            if (!string.IsNullOrEmpty(comboName)) {
                throw new UserError($"You must first remove this product from the {comboName} combo");
            }
        }
    }
    public void CreateVariantIds() {
        base.CreateVariantIds();
        var archivedProduct = Env.Search<ProductProduct>(x => x.ProductTmplId == this.Id && !x.Active).FirstOrDefault();
        if (archivedProduct != null) {
            var comboChoicesToDelete = Env.Search<PosComboLine>(x => x.ProductId == archivedProduct.Id);
            if (comboChoicesToDelete.Count > 0) {
                var comboIds = comboChoicesToDelete.Select(x => x.ComboId);
                comboChoicesToDelete.Delete();
                var newVariants = this.ProductVariantIds.Where(x => x.Active);
                Env.Create<PosComboLine>(newVariants.Select(x => new PosComboLine() {
                    ProductId = x.Id,
                    ComboId = comboIds.First()
                }));
            }
        }
    }
    public void OnChangeType() {
        if (this.Type == "combo" && this.AttributeLineIds.Count > 0) {
            throw new UserError("Combo products cannot contains variants or attributes");
        }
        base.OnChangeType();
    }
}
