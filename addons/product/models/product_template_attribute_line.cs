csharp
public partial class ProductTemplateAttributeLine {
    public void ComputeValueCount() {
        this.ValueCount = this.ValueIds.Count;
    }

    public void OnChangeAttributeId() {
        this.ValueIds = this.ValueIds.Where(pav => pav.AttributeId == this.AttributeId).ToList();
    }

    public bool CheckValidValues() {
        if (this.Active && this.ValueIds.Count == 0) {
            throw new Exception($"The attribute {this.AttributeId.DisplayName} must have at least one value for the product {this.ProductTmplId.DisplayName}.");
        }
        foreach (var pav in this.ValueIds) {
            if (pav.AttributeId != this.AttributeId) {
                throw new Exception($"On the product {this.ProductTmplId.DisplayName} you cannot associate the value {pav.DisplayName} with the attribute {this.AttributeId.DisplayName} because they do not match.");
            }
        }
        return true;
    }

    public void Create(List<Dictionary<string, object>> valsList) {
        List<ProductTemplateAttributeLine> createValues = new List<ProductTemplateAttributeLine>();
        List<ProductTemplateAttributeLine> activatedLines = new List<ProductTemplateAttributeLine>();
        foreach (var value in valsList) {
            Dictionary<string, object> vals = new Dictionary<string, object>(value) { { "Active", value.GetValueOrDefault("Active", true) } };
            ProductTemplateAttributeLine archivedPtal = Env.GetModel("Product.ProductTemplateAttributeLine").SearchFirst(new Dictionary<string, object> {
                {"Active", false},
                {"ProductTmplId", vals["ProductTmplId"]},
                {"AttributeId", vals["AttributeId"]}
            });
            if (archivedPtal != null) {
                archivedPtal.WithContext(new Dictionary<string, object> { {"UpdateProductTemplateAttributeValues", false }}).Write(vals);
                activatedLines.Add(archivedPtal);
            } else {
                createValues.Add(Env.GetModel("Product.ProductTemplateAttributeLine").Create(vals));
            }
        }
        List<ProductTemplateAttributeLine> res = activatedLines.Concat(createValues).ToList();
        if (Env.Context.GetValueOrDefault("CreateProductProduct", true)) {
            res.ForEach(r => r.UpdateProductTemplateAttributeValues());
        }
    }

    public void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("ProductTmplId")) {
            if (this.ProductTmplId.Id != (long)values["ProductTmplId"]) {
                throw new Exception($"You cannot move the attribute {this.AttributeId.DisplayName} from the product {this.ProductTmplId.DisplayName} to the product {values["ProductTmplId"]}.");
            }
        }

        if (values.ContainsKey("AttributeId")) {
            if (this.AttributeId.Id != (long)values["AttributeId"]) {
                throw new Exception($"On the product {this.ProductTmplId.DisplayName} you cannot transform the attribute {this.AttributeId.DisplayName} into the attribute {values["AttributeId"]}.");
            }
        }

        if (values.ContainsKey("Active") && !(bool)values["Active"]) {
            values["ValueIds"] = new List<object>();
        }
        base.Write(values);
        if (values.ContainsKey("Active")) {
            Env.FlushAll();
            Env.GetModel("Product.ProductTemplate").InvalidateCache(new List<string> { "AttributeLineIds" });
        }
        if (Env.Context.GetValueOrDefault("UpdateProductTemplateAttributeValues", true)) {
            this.UpdateProductTemplateAttributeValues();
        }
    }

    public void Unlink() {
        this.ProductTemplateValueIds.Where(ptav => ptav.PtavActive).ToList().ForEach(ptav => ptav.Unlink());
        ProductTemplate productTemplate = this.ProductTmplId;
        List<ProductTemplateAttributeLine> ptalToArchive = new List<ProductTemplateAttributeLine>();
        foreach (var ptal in this) {
            try {
                using (Env.Cr.Savepoint()) {
                    base.Unlink();
                }
            } catch (Exception) {
                ptalToArchive.Add(ptal);
            }
        }
        ptalToArchive.ForEach(ptal => ptal.ActionArchive());
        List<ProductTemplate> templates = new List<ProductTemplate>(productTemplate.Where(pt => !ptalToArchive.Contains(pt.AttributeLineIds.FirstOrDefault())).ToList());
        templates.ForEach(template => template.CreateVariantIds());
    }

    public void UpdateProductTemplateAttributeValues() {
        List<ProductTemplateAttributeValue> ptavToCreate = new List<ProductTemplateAttributeValue>();
        List<ProductTemplateAttributeValue> ptavToUnlink = new List<ProductTemplateAttributeValue>();
        foreach (var ptal in this) {
            List<ProductTemplateAttributeValue> ptavToActivate = new List<ProductTemplateAttributeValue>();
            List<ProductAttributeValue> remainingPav = new List<ProductAttributeValue>(ptal.ValueIds);
            foreach (var ptav in ptal.ProductTemplateValueIds) {
                if (!remainingPav.Contains(ptav.ProductAttributeValueId)) {
                    if (ptav.PtavActive) {
                        ptavToUnlink.Add(ptav);
                    }
                } else {
                    remainingPav.Remove(ptav.ProductAttributeValueId);
                    if (!ptav.PtavActive) {
                        ptavToActivate.Add(ptav);
                    }
                }
            }

            foreach (var pav in remainingPav) {
                ProductTemplateAttributeValue ptav = Env.GetModel("Product.ProductTemplateAttributeValue").SearchFirst(new Dictionary<string, object> {
                    {"PtavActive", false},
                    {"ProductTmplId", ptal.ProductTmplId.Id},
                    {"AttributeId", ptal.AttributeId.Id},
                    {"ProductAttributeValueId", pav.Id}
                });
                if (ptav != null) {
                    ptav.Write(new Dictionary<string, object> { { "PtavActive", true }, { "AttributeLineId", ptal.Id } });
                    ptavToUnlink.Remove(ptav);
                } else {
                    ptavToCreate.Add(Env.GetModel("Product.ProductTemplateAttributeValue").Create(new Dictionary<string, object> {
                        {"ProductAttributeValueId", pav.Id},
                        {"AttributeLineId", ptal.Id},
                        {"PriceExtra", pav.DefaultExtraPrice}
                    }));
                }
            }
            ptavToActivate.ForEach(ptav => ptav.Write(new Dictionary<string, object> { { "PtavActive", true } }));
            ptavToUnlink.ForEach(ptav => ptav.Write(new Dictionary<string, object> { { "PtavActive", false } }));
        }
        ptavToUnlink.ForEach(ptav => ptav.Unlink());
        Env.GetModel("Product.ProductTemplateAttributeValue").Create(ptavToCreate);
        this.ProductTmplId.CreateVariantIds();
    }

    public List<ProductTemplateAttributeLine> WithoutNoVariantAttributes() {
        return this.Where(ptal => ptal.AttributeId.CreateVariant != "no_variant").ToList();
    }

    public Dictionary<string, object> ActionOpenAttributeValues() {
        return new Dictionary<string, object> {
            {"type", "ir.actions.act_window"},
            {"name", "Product Variant Values"},
            {"res_model", "Product.ProductTemplateAttributeValue"},
            {"view_mode", "tree,form"},
            {"domain", new List<object> { ["id", "in", this.ProductTemplateValueIds.Select(ptav => ptav.Id).ToList()] }},
            {"views", new List<object> {
                new List<object> { Env.Ref("product.product_template_attribute_value_view_tree").Id, "list" },
                new List<object> { Env.Ref("product.product_template_attribute_value_view_form").Id, "form" },
            }},
            {"context", new Dictionary<string, object> {
                {"search_default_active", 1},
                {"product_invisible", true}
            }}
        };
    }
}
