csharp
public partial class MrpProductTemplate {
    public void ComputeBomCount() {
        this.BomCount = Env.Model("Mrp.Bom").SearchCount(["|", ("ProductTmplId", "=", this.Id), ("ByproductIds.ProductId.ProductTmplId", "=", this.Id)]);
    }

    public void ComputeIsKits() {
        var domain = new List<object> { ["&", ("Type", "=", "phantom"), ("ProductTmplId", "in", this.Id)] };
        var bomMapping = Env.Model("Mrp.Bom").Sudo().SearchRead(domain, new List<string> { "ProductTmplId" });
        var kitsIds = new HashSet<int>();
        foreach (var bomData in bomMapping) {
            if (bomData["ProductTmplId"] != null) {
                kitsIds.Add((int)bomData["ProductTmplId"]);
            }
        }
        this.IsKits = kitsIds.Contains(this.Id);
    }

    public List<object> SearchIsKits(string operator, object value) {
        if (operator != "=" && operator != "!=") {
            throw new ArgumentException("Unsupported operator");
        }
        var bomTmplQuery = Env.Model("Mrp.Bom").Sudo().Search(
            new List<object> { ["&", ("Company", "in", new List<object> { null }.Concat(Env.Companies.Ids).ToList()), ("Active", "=", true), ("Type", "=", "phantom")] });
        var neg = "";
        if ((operator == "=" && (bool)value == false) || (operator == "!=" && (bool)value == true)) {
            neg = "not ";
        }
        return new List<object> { ["id", neg + "inselect", bomTmplQuery.Subselect("ProductTmplId")] };
    }

    public void ComputeShowQtyStatusButton() {
        base.ComputeShowQtyStatusButton();
        if (this.IsKits) {
            this.ShowOnHandQtyStatusButton = this.ProductVariantCount <= 1;
            this.ShowForecastedQtyStatusButton = false;
        }
    }

    public void ComputeUsedInBomCount() {
        this.UsedInBomCount = Env.Model("Mrp.Bom").SearchCount(
            new List<object> { ("BomLineIds.ProductTmplId", "=", this.Id) });
    }

    public void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Active")) {
            var active = (bool)values["Active"];
            this.Filtered(product => (bool)product["Active"] != active).WithContext(new Dictionary<string, object> { { "ActiveTest", false } }).BomIds.Write(new Dictionary<string, object> { { "Active", active } });
        }
        base.Write(values);
    }

    public IrActionsActions ActionUsedInBom() {
        var action = Env.Model("Ir.Actions.Actions")._ForXmlId("mrp.mrp_bom_form_action");
        action["domain"] = new List<object> { ("BomLineIds.ProductTmplId", "=", this.Id) };
        return action;
    }

    public void ComputeMrpProductQty() {
        this.MrpProductQty = (decimal)Math.Round(this.ProductVariantIds.Sum(variant => (decimal)variant["MrpProductQty"]), this.Uom.Rounding);
    }

    public IrActionsActions ActionViewMos() {
        var action = Env.Model("Ir.Actions.Actions")._ForXmlId("mrp.mrp_production_action");
        action["domain"] = new List<object> { ("State", "=", "done"), ("ProductTmplId", "in", this.Ids) };
        action["context"] = new Dictionary<string, object> { { "SearchDefaultFilterPlanDate", 1 } };
        return action;
    }

    public IrActionsActions ActionArchive() {
        var filteredProducts = Env.Model("Mrp.BomLine").Search(new List<object> { ("ProductId", "in", this.ProductVariantIds.Ids), ("Bom.Active", "=", true) }).ProductId.Select(p => p.DisplayName).ToList();
        var res = base.ActionArchive();
        if (filteredProducts.Count > 0) {
            return new IrActionsActions {
                Type = "ir.actions.client",
                Tag = "display_notification",
                Params = new Dictionary<string, object> {
                    { "title", $"Note that product(s): '{string.Join(", ", filteredProducts)}' is/are still linked to active Bill of Materials, which means that the product can still be used on it/them." },
                    { "type", "warning" },
                    { "sticky", true },
                    { "next", new Dictionary<string, object> { { "type", "ir.actions.act_window_close" } } },
                }
            };
        }
        return res;
    }

    public List<int> GetBackendRootMenuIds() {
        return base.GetBackendRootMenuIds().Concat(new List<int> { Env.Ref("mrp.menu_mrp_root").Id }).ToList();
    }
}

public partial class MrpProductProduct {
    public void ComputeBomCount() {
        this.BomCount = Env.Model("Mrp.Bom").SearchCount(new List<object> { ["|", "|", ("ByproductIds.ProductId", "=", this.Id), ("ProductId", "=", this.Id), "&", ("ProductId", "=", false), ("ProductTmplId", "=", this.ProductTmplId.Id)] });
    }

    public void ComputeIsKits() {
        var domain = new List<object> { ["&", ("Type", "=", "phantom"), ["|", ("ProductId", "in", this.Ids), "&", ("ProductId", "=", false), ("ProductTmplId", "in", this.ProductTmplId.Ids)]] };
        var bomMapping = Env.Model("Mrp.Bom").Sudo().SearchRead(domain, new List<string> { "ProductTmplId", "ProductId" });
        var kitsTemplateIds = new HashSet<int>();
        var kitsProductIds = new HashSet<int>();
        foreach (var bomData in bomMapping) {
            if (bomData["ProductId"] != null) {
                kitsProductIds.Add((int)bomData["ProductId"]);
            } else {
                kitsTemplateIds.Add((int)bomData["ProductTmplId"]);
            }
        }
        this.IsKits = kitsProductIds.Contains(this.Id) || kitsTemplateIds.Contains(this.ProductTmplId.Id);
    }

    public List<object> SearchIsKits(string operator, object value) {
        if (operator != "=" && operator != "!=") {
            throw new ArgumentException("Unsupported operator");
        }
        var bomTmplQuery = Env.Model("Mrp.Bom").Sudo().Search(
            new List<object> { ["&", ("Company", "in", new List<object> { null }.Concat(Env.Companies.Ids).ToList()), ("Active", "=", true), ("Type", "=", "phantom"), ("ProductId", "=", false)] });
        var bomProductQuery = Env.Model("Mrp.Bom").Sudo().Search(
            new List<object> { ["&", ("Company", "in", new List<object> { null }.Concat(Env.Companies.Ids).ToList()), ("Active", "=", true), ("Type", "=", "phantom"), ("ProductId", "!=" , false)] });
        var neg = "";
        var op = "|";
        if ((operator == "=" && (bool)value == false) || (operator == "!=" && (bool)value == true)) {
            neg = "not ";
            op = "&";
        }
        return new List<object> { op, ("ProductTmplId", neg + "inselect", bomTmplQuery.Subselect("ProductTmplId")), ("Id", neg + "inselect", bomProductQuery.Subselect("ProductId")) };
    }

    public void ComputeShowQtyStatusButton() {
        base.ComputeShowQtyStatusButton();
        if (this.IsKits) {
            this.ShowOnHandQtyStatusButton = true;
            this.ShowForecastedQtyStatusButton = false;
        }
    }

    public void ComputeUsedInBomCount() {
        this.UsedInBomCount = Env.Model("Mrp.Bom").SearchCount(
            new List<object> { ("BomLineIds.ProductId", "=", this.Id) });
    }

    public void Write(Dictionary<string, object> values) {
        if (values.ContainsKey("Active")) {
            var active = (bool)values["Active"];
            this.Filtered(product => (bool)product["Active"] != active).WithContext(new Dictionary<string, object> { { "ActiveTest", false } }).VariantBomIds.Write(new Dictionary<string, object> { { "Active", active } });
        }
        base.Write(values);
    }

    public List<int> GetComponents() {
        var bomKit = Env.Model("Mrp.Bom")._BomFind(this, "phantom")[this];
        if (bomKit != null) {
            var boms = bomKit.Explode(this, 1);
            return boms.Select(bomLine => bomLine.ProductId.Id).Where(id => (bool)bomLine.ProductId["IsStorable"]).ToList();
        }
        return base.GetComponents();
    }

    public IrActionsActions ActionUsedInBom() {
        var action = Env.Model("Ir.Actions.Actions")._ForXmlId("mrp.mrp_bom_form_action");
        action["domain"] = new List<object> { ("BomLineIds.ProductId", "=", this.Id) };
        return action;
    }

    public void ComputeMrpProductQty() {
        var dateFrom = Fields.Datetime.ToString(Fields.DateTime.Now - TimeSpan.FromDays(365));
        var domain = new List<object> { ("State", "=", "done"), ("ProductId", "in", this.Ids), ("DateStart", ">", dateFrom) };
        var readGroupRes = Env.Model("Mrp.Production")._ReadGroup(domain, new List<string> { "ProductId" }, new List<string> { "ProductUomQty:sum" });
        var mappedData = readGroupRes.ToDictionary(product => (int)product["ProductId"], product => (decimal)product["ProductUomQty:sum"]);
        this.MrpProductQty = (decimal)Math.Round(mappedData.TryGetValue(this.Id, out var qty) ? qty : 0, this.Uom.Rounding);
    }

    public Dictionary<int, Dictionary<string, decimal>> ComputeQuantitiesDict(int? lotId, int? ownerId, int? packageId, DateTime? fromDate = null, DateTime? toDate = null) {
        var bomKits = Env.Model("Mrp.Bom")._BomFind(this, "phantom");
        var kits = this.Filtered(product => bomKits.ContainsKey(product));
        var regularProducts = this - kits;
        var res = (
            regularProducts.Any()
            ? base.ComputeQuantitiesDict(lotId, ownerId, packageId, fromDate, toDate)
            : new Dictionary<int, Dictionary<string, decimal>>()
        );
        var qties = Env.Context.Get<Dictionary<string, object>>("MrpComputeQuantities") ?? new Dictionary<string, object>();
        qties.Update(res);
        var bomSubLinesPerKit = new Dictionary<MrpProductProduct, List<Tuple<MrpBomLine, Dictionary<string, object>>>>();
        var prefetchComponentIds = new HashSet<int>();
        foreach (var product in bomKits.Keys) {
            var boms = bomKits[product].Explode(product, 1);
            bomSubLinesPerKit[product] = boms;
            foreach (var bomLine in boms) {
                if (!prefetchComponentIds.Contains(bomLine.ProductId.Id)) {
                    prefetchComponentIds.Add(bomLine.ProductId.Id);
                }
            }
        }
        foreach (var product in bomKits.Keys) {
            var bomSubLines = bomSubLinesPerKit[product];
            var bomSubLinesGrouped = new Dictionary<MrpProductProduct, List<Tuple<MrpBomLine, Dictionary<string, object>>>>();
            foreach (var info in bomSubLines) {
                if (!bomSubLinesGrouped.ContainsKey(info.Item1.ProductId)) {
                    bomSubLinesGrouped[info.Item1.ProductId] = new List<Tuple<MrpBomLine, Dictionary<string, object>>>();
                }
                bomSubLinesGrouped[info.Item1.ProductId].Add(info);
            }
            var ratiosVirtualAvailable = new List<decimal>();
            var ratiosQtyAvailable = new List<decimal>();
            var ratiosIncomingQty = new List<decimal>();
            var ratiosOutgoingQty = new List<decimal>();
            var ratiosFreeQty = new List<decimal>();
            foreach (var component in bomSubLinesGrouped.Keys) {
                component = component.WithContext(new Dictionary<string, object> { { "MrpComputeQuantities", qties } }).WithPrefetch(prefetchComponentIds.ToList());
                var qtyPerKit = 0m;
                foreach (var bomLine in bomSubLinesGrouped[component]) {
                    if (!(bool)component["IsStorable"] || (decimal)bomLine.Item2["Qty"] == 0) {
                        continue;
                    }
                    var uomQtyPerKit = (decimal)bomLine.Item2["Qty"] / (decimal)bomLine.Item2["OriginalQty"];
                    qtyPerKit += bomLine.Item1.ProductUom.ComputeQuantity(uomQtyPerKit, bomLine.Item1.ProductId.Uom, false, false);
                }
                if (qtyPerKit == 0) {
                    continue;
                }
                var rounding = component.Uom.Rounding;
                var componentRes = (
                    qties.ContainsKey(component.Id)
                    ? qties[component.Id]
                    : new Dictionary<string, decimal> {
                        { "VirtualAvailable", (decimal)Math.Round(component.VirtualAvailable, rounding) },
                        { "QtyAvailable", (decimal)Math.Round(component.QtyAvailable, rounding) },
                        { "IncomingQty", (decimal)Math.Round(component.IncomingQty, rounding) },
                        { "OutgoingQty", (decimal)Math.Round(component.OutgoingQty, rounding) },
                        { "FreeQty", (decimal)Math.Round(component.FreeQty, rounding) },
                    }
                );
                ratiosVirtualAvailable.Add((decimal)componentRes["VirtualAvailable"] / qtyPerKit);
                ratiosQtyAvailable.Add((decimal)componentRes["QtyAvailable"] / qtyPerKit);
                ratiosIncomingQty.Add((decimal)componentRes["IncomingQty"] / qtyPerKit);
                ratiosOutgoingQty.Add((decimal)componentRes["OutgoingQty"] / qtyPerKit);
                ratiosFreeQty.Add((decimal)componentRes["FreeQty"] / qtyPerKit);
            }
            if (bomSubLines.Any() && ratiosVirtualAvailable.Any()) {
                res[product.Id] = new Dictionary<string, decimal> {
                    { "VirtualAvailable", (decimal)Math.Floor(ratiosVirtualAvailable.Min() * bomKits[product].ProductQty) },
                    { "QtyAvailable", (decimal)Math.Floor(ratiosQtyAvailable.Min() * bomKits[product].ProductQty) },
                    { "IncomingQty", (decimal)Math.Floor(ratiosIncomingQty.Min() * bomKits[product].ProductQty) },
                    { "OutgoingQty", (decimal)Math.Floor(ratiosOutgoingQty.Min() * bomKits[product].ProductQty) },
                    { "FreeQty", (decimal)Math.Floor(ratiosFreeQty.Min() * bomKits[product].ProductQty) },
                };
            } else {
                res[product.Id] = new Dictionary<string, decimal> {
                    { "VirtualAvailable", 0 },
                    { "QtyAvailable", 0 },
                    { "IncomingQty", 0 },
                    { "OutgoingQty", 0 },
                    { "FreeQty", 0 },
                };
            }
        }
        return res;
    }

    public IrActionsActions ActionViewBom() {
        var action = Env.Model("Ir.Actions.Actions")._ForXmlId("mrp.product_open_bom");
        var templateIds = this.ProductTmplId.Ids;
        action["context"] = new Dictionary<string, object> {
            { "DefaultProductTmplId", templateIds[0] },
            { "DefaultProductId", Env.User.HasGroup("product.group_product_variant") ? this.Ids[0] : null },
        };
        action["domain"] = new List<object> { ["|", "|", ("ByproductIds.ProductId", "in", this.Ids), ("ProductId", "in", this.Ids), "&", ("ProductId", "=", false), ("ProductTmplId", "in", templateIds)] };
        return action;
    }

    public IrActionsActions ActionViewMos() {
        var action = this.ProductTmplId.ActionViewMos();
        action["domain"] = new List<object> { ("State", "=", "done"), ("ProductId", "in", this.Ids) };
        return action;
    }

    public IrActionsActions ActionOpenQuants() {
        var bomKits = Env.Model("Mrp.Bom")._BomFind(this, "phantom");
        var components = this - this.Concat(bomKits.Keys.ToList());
        foreach (var product in bomKits.Keys) {
            var boms = bomKits[product].Explode(product, 1);
            components |= this.Concat(boms.Select(l => l.Item1.ProductId).ToList());
        }
        var res = base.ActionOpenQuants();
        if (bomKits.Any()) {
            res["context"]["SingleProduct"] = false;
            res["context"].Remove("DefaultProductTmplId");
        }
        return res;
    }

    public bool MatchAllVariantValues(List<int> productTemplateAttributeValueIds) {
        var productTemplateAttributeValue = Env.Model("Product.Template.Attribute.Value").Browse(productTemplateAttributeValueIds);
        return (this.ProductTemplateAttributeValueIds.Intersect(productTemplateAttributeValue).Count() == productTemplateAttributeValue.Count());
    }

    public List<object> CountReturnedSnProductsDomain(int snLot, List<object> orDomains) {
        orDomains.Add(new List<object> {
            ("ProductionId", "=", false),
            ("LocationId.Usage", "=", "production"),
            ("MoveId.UnbuildId", "!=" , false),
        });
        return base.CountReturnedSnProductsDomain(snLot, orDomains);
    }

    public List<int> SearchQtyAvailableNew(string operator, decimal value, int? lotId = null, int? ownerId = null, int? packageId = null) {
        var productIds = base.SearchQtyAvailableNew(operator, value, lotId, ownerId, packageId);
        var kitBoms = Env.Model("Mrp.Bom").Search(new List<object> { ("Type", "=", "phantom" ) });
        var kitProducts = Env.Model("Mrp.ProductProduct");
        foreach (var kit in kitBoms) {
            if (kit.ProductId != null) {
                kitProducts |= kit.ProductId;
            } else {
                kitProducts |= kit.ProductTmplId.ProductVariantIds;
            }
        }
        foreach (var product in kitProducts) {
            if (OPERATORS[operator]((decimal)product["QtyAvailable"], value)) {
                productIds.Add(product.Id);
            }
        }
        return productIds.Distinct().ToList();
    }

    public IrActionsActions ActionArchive() {
        var filteredProducts = Env.Model("Mrp.BomLine").Search(new List<object> { ("ProductId", "in", this.Ids), ("Bom.Active", "=", true) }).ProductId.Select(p => p.DisplayName).ToList();
        var res = base.ActionArchive();
        if (filteredProducts.Count > 0) {
            return new IrActionsActions {
                Type = "ir.actions.client",
                Tag = "display_notification",
                Params = new Dictionary<string, object> {
                    { "title", $"Note that product(s): '{string.Join(", ", filteredProducts)}' is/are still linked to active Bill of Materials, which means that the product can still be used on it/them." },
                    { "type", "warning" },
                    { "sticky", true },
                    { "next", new Dictionary<string, object> { { "type", "ir.actions.act_window_close" } } },
                }
            };
        }
        return res;
    }

    public List<int> GetBackendRootMenuIds() {
        return base.GetBackendRootMenuIds().Concat(new List<int> { Env.Ref("mrp.menu_mrp_root").Id }).ToList();
    }
}
