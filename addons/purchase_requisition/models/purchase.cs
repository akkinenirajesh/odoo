csharp
public partial class PurchaseOrderGroup {
    public virtual void Write(Dictionary<string, object> vals) {
        var res = Env.Model("Purchase.PurchaseOrderGroup").Call("Write", this, vals);
        if (Env.Model("Purchase.PurchaseOrderGroup").Call<int>("Length", this) <= 1) {
            Env.Model("Purchase.PurchaseOrderGroup").Call("Unlink", this);
        }
    }
}

public partial class PurchaseOrder {
    public virtual void OnchangeRequisitionId() {
        if (this.RequisitionId == null) {
            return;
        }
        var company = Env.Model("Res.Company").Call<ResCompany>("Browse", this.CompanyId);
        var requisition = Env.Model("Purchase.PurchaseRequisition").Call<PurchaseRequisition>("Browse", this.RequisitionId);
        var partner = this.PartnerId != null ? Env.Model("Res.Partner").Call<ResPartner>("Browse", this.PartnerId) : requisition.VendorId;
        var paymentTerm = partner.PropertySupplierPaymentTermId;

        var fiscalPosition = Env.Model("Account.Fiscal.Position").Call<AccountFiscalPosition>("_GetFiscalPosition", partner);

        this.PartnerId = partner.Id;
        this.FiscalPositionId = fiscalPosition.Id;
        this.PaymentTermId = paymentTerm.Id;
        this.CompanyId = requisition.CompanyId.Id;
        this.CurrencyId = requisition.CurrencyId.Id;
        if (this.Origin == null || !this.Origin.Contains(requisition.Name)) {
            if (this.Origin != null) {
                this.Origin = this.Origin + ", " + requisition.Name;
            } else {
                this.Origin = requisition.Name;
            }
        }
        this.Notes = requisition.Description;
        if (requisition.DateStart != null) {
            this.DateOrder = DateTime.Now > DateTime.Parse(requisition.DateStart) ? DateTime.Now : DateTime.Parse(requisition.DateStart);
        } else {
            this.DateOrder = DateTime.Now;
        }
        var orderLines = new List<object>();
        foreach (var line in requisition.LineIds) {
            var productLang = Env.Model("Product.Product").Call<ProductProduct>("WithContext", line.ProductId, new Dictionary<string, object>() { { "Lang", partner.Lang }, { "PartnerId", partner.Id } });
            var name = productLang.DisplayName;
            if (productLang.DescriptionPurchase != null) {
                name += "\n" + productLang.DescriptionPurchase;
            }
            var taxesIds = fiscalPosition.MapTax(line.ProductId.SupplierTaxesId.Where(tax => tax.CompanyId == requisition.CompanyId).ToList()).Select(x => x.Id).ToList();
            var productQty = line.ProductUomId != line.ProductId.UomPOId ? line.ProductUomId.ComputeQuantity(line.ProductQty, line.ProductId.UomPOId) : line.ProductQty;
            var priceUnit = line.ProductUomId != line.ProductId.UomPOId ? line.ProductUomId.ComputePrice(line.PriceUnit, line.ProductId.UomPOId) : line.PriceUnit;
            if (requisition.RequisitionType != "PurchaseTemplate") {
                productQty = 0;
            }
            var orderLineValues = line._PreparePurchaseOrderLine(name, productQty, priceUnit, taxesIds);
            orderLines.Add(new Dictionary<string, object>() { { "Id", 0 }, { "Command", "0" }, { "Values", orderLineValues } });
        }
        this.OrderLine = orderLines;
    }
    public virtual void ButtonConfirm() {
        if (this.AlternativePOIds != null && !Env.Context.ContainsKey("SkipAlternativeCheck") || !Env.Context["SkipAlternativeCheck"].Equals(true)) {
            var alternativePOIds = this.AlternativePOIds.Where(po => po.State.In("Draft", "Sent", "To Approve") && po.Id != this.Id).ToList();
            if (alternativePOIds.Any()) {
                var view = Env.Ref("purchase_requisition.purchase_requisition_alternative_warning_form");
                var action = new Dictionary<string, object>() {
                    { "Name", _("What about the alternative Requests for Quotations?") },
                    { "Type", "ir.actions.act_window" },
                    { "ViewMode", "form" },
                    { "ResModel", "purchase.requisition.alternative.warning" },
                    { "Views", new List<object>() { new List<object>() { view.Id, "form" } } },
                    { "Target", "new" },
                    { "Context", new Dictionary<string, object>() { { "DefaultAlternativePOIds", alternativePOIds.Select(x => x.Id).ToList() }, { "DefaultPOIds", new List<object>() { this.Id } } } }
                };
                Env.Model("ir.actions.act_window").Call("Create", action);
                return;
            }
        }
        var res = Env.Model("Purchase.PurchaseOrder").Call("ButtonConfirm", this);
    }
    public virtual void ComputeHasAlternatives() {
        this.HasAlternatives = false;
        if (Env.User.HasGroup("purchase_requisition.group_purchase_alternatives")) {
            this.HasAlternatives = this.PurchaseGroupId != null;
        }
    }
    public virtual void Create(Dictionary<string, object> vals) {
        var orders = Env.Model("Purchase.PurchaseOrder").Call<List<PurchaseOrder>>("Create", vals);
        if (Env.Context.ContainsKey("OriginPOId")) {
            var originPOId = Env.Model("Purchase.PurchaseOrder").Call<PurchaseOrder>("Browse", Env.Context["OriginPOId"]);
            if (originPOId.PurchaseGroupId != null) {
                originPOId.PurchaseGroupId.OrderIds.AddRange(orders);
            } else {
                Env.Model("Purchase.PurchaseOrderGroup").Call("Create", new Dictionary<string, object>() { { "OrderIds", new List<object>() { new List<object>() { "Command", "set" }, orders.Select(x => x.Id).ToList(), originPOId.Id } } });
            }
        }
        foreach (var order in orders) {
            if (order.RequisitionId != null) {
                order.MessagePostWithSource("mail.message_origin_link", new Dictionary<string, object>() { { "Self", order }, { "Origin", order.RequisitionId } }, "mail.mt_note");
            }
        }
    }
    public virtual void Write(Dictionary<string, object> vals) {
        var origPurchaseGroup = this.PurchaseGroupId;
        var result = Env.Model("Purchase.PurchaseOrder").Call("Write", this, vals);
        if (vals.ContainsKey("RequisitionId")) {
            foreach (var order in this) {
                order.MessagePostWithSource("mail.message_origin_link", new Dictionary<string, object>() { { "Self", order }, { "Origin", order.RequisitionId }, { "Edit", true } }, "mail.mt_note");
            }
        }
        if (vals.ContainsKey("AlternativePOIds")) {
            if (this.PurchaseGroupId == null && this.AlternativePOIds.Count + 1 > 1) {
                Env.Model("Purchase.PurchaseOrderGroup").Call("Create", new Dictionary<string, object>() { { "OrderIds", new List<object>() { new List<object>() { "Command", "set" }, this.AlternativePOIds.Select(x => x.Id).ToList(), this.Id } } });
            } else if (this.PurchaseGroupId != null && this.AlternativePOIds.Count + 1 <= 1) {
                this.PurchaseGroupId.Unlink();
            }
        }
        if (vals.ContainsKey("PurchaseGroupId")) {
            var additionalGroups = origPurchaseGroup.Where(x => x != this.PurchaseGroupId).ToList();
            if (additionalGroups.Any()) {
                var additionalPos = additionalGroups.SelectMany(x => x.OrderIds.Where(y => y != this.PurchaseGroupId.OrderIds)).ToList();
                additionalGroups.ForEach(x => x.Unlink());
                if (additionalPos.Any()) {
                    this.PurchaseGroupId.OrderIds.AddRange(additionalPos);
                }
            }
        }
        return result;
    }
    public virtual void ActionCreateAlternative() {
        var ctx = new Dictionary<string, object>() { { "DefaultOriginPOId", this.Id } };
        var action = new Dictionary<string, object>() {
            { "Name", _("Create alternative") },
            { "Type", "ir.actions.act_window" },
            { "ViewMode", "form" },
            { "ResModel", "purchase.requisition.create.alternative" },
            { "ViewId", Env.Ref("purchase_requisition.purchase_requisition_create_alternative_form").Id },
            { "Target", "new" },
            { "Context", ctx }
        };
        Env.Model("ir.actions.act_window").Call("Create", action);
    }
    public virtual void ActionCompareAlternativeLines() {
        var ctx = new Dictionary<string, object>() {
            { "SearchDefaultGroupByProduct", true },
            { "PurchaseOrderId", this.Id }
        };
        var viewId = Env.Ref("purchase_requisition.purchase_order_line_compare_tree").Id;
        var action = new Dictionary<string, object>() {
            { "Name", _("Compare Order Lines") },
            { "Type", "ir.actions.act_window" },
            { "ViewMode", "list" },
            { "ResModel", "purchase.order.line" },
            { "Views", new List<object>() { new List<object>() { viewId, "list" } } },
            { "Domain", new List<object>() { new List<object>() { "Order_Id", "in", new List<object>() { this.Id }.Concat(this.AlternativePOIds.Select(x => x.Id).ToList()).ToList() }, { "DisplayType", "=", false } } },
            { "Context", ctx }
        };
        Env.Model("ir.actions.act_window").Call("Create", action);
    }
    public virtual List<object> GetTenderBestLines() {
        var productToBestPriceLine = new Dictionary<int, PurchaseOrderLine>();
        var productToBestDateLine = new Dictionary<int, PurchaseOrderLine>();
        var productToBestPriceUnit = new Dictionary<int, PurchaseOrderLine>();
        var poAlternatives = this.AlternativePOIds.Concat(new List<PurchaseOrder>() { this }).ToList();
        var multipleCurrencies = poAlternatives.Select(x => x.CurrencyId).Distinct().Count() > 1;
        foreach (var line in poAlternatives.SelectMany(x => x.OrderLine).ToList()) {
            if (line.ProductQty == 0 || line.PriceSubtotal == 0 || line.State.In("Cancel", "Purchase", "Done")) {
                continue;
            }
            if (!productToBestPriceLine.ContainsKey(line.ProductId)) {
                productToBestPriceLine.Add(line.ProductId, line);
                productToBestPriceUnit.Add(line.ProductId, line);
            } else {
                var priceSubtotal = line.PriceSubtotal;
                var priceUnit = line.PriceUnit;
                var currentPriceSubtotal = productToBestPriceLine[line.ProductId].PriceSubtotal;
                var currentPriceUnit = productToBestPriceUnit[line.ProductId].PriceUnit;
                if (multipleCurrencies) {
                    priceSubtotal /= line.Order.CurrencyRate;
                    priceUnit /= line.Order.CurrencyRate;
                    currentPriceSubtotal /= productToBestPriceLine[line.ProductId].Order.CurrencyRate;
                    currentPriceUnit /= productToBestPriceUnit[line.ProductId].Order.CurrencyRate;
                }
                if (currentPriceSubtotal > priceSubtotal) {
                    productToBestPriceLine[line.ProductId] = line;
                } else if (currentPriceSubtotal == priceSubtotal) {
                    productToBestPriceLine[line.ProductId] = line;
                }
                if (currentPriceUnit > priceUnit) {
                    productToBestPriceUnit[line.ProductId] = line;
                } else if (currentPriceUnit == priceUnit) {
                    productToBestPriceUnit[line.ProductId] = line;
                }
            }
            if (!productToBestDateLine.ContainsKey(line.ProductId) || productToBestDateLine[line.ProductId].DatePlanned > line.DatePlanned) {
                productToBestDateLine.Add(line.ProductId, line);
            } else if (productToBestDateLine.ContainsKey(line.ProductId) && productToBestDateLine[line.ProductId].DatePlanned == line.DatePlanned) {
                productToBestDateLine[line.ProductId] = line;
            }
        }
        var bestPriceIds = new List<int>();
        var bestDateIds = new List<int>();
        var bestPriceUnitIds = new List<int>();
        foreach (var lines in productToBestPriceLine.Values) {
            bestPriceIds.AddRange(lines.Select(x => x.Id).ToList());
        }
        foreach (var lines in productToBestDateLine.Values) {
            bestDateIds.AddRange(lines.Select(x => x.Id).ToList());
        }
        foreach (var lines in productToBestPriceUnit.Values) {
            bestPriceUnitIds.AddRange(lines.Select(x => x.Id).ToList());
        }
        return new List<object>() { bestPriceIds, bestDateIds, bestPriceUnitIds };
    }
}

public partial class PurchaseOrderLine {
    public virtual void _ComputeAmount() {
        var super = Env.Model("Purchase.PurchaseOrderLine").Call("ComputeAmount", this);
        this.PriceTotalCC = this.PriceSubtotal / this.Order.CurrencyRate;
    }
    public virtual void _ComputePriceUnitAndDatePlannedAndName() {
        var poLinesWithoutRequisition = new List<PurchaseOrderLine>();
        foreach (var pol in this) {
            if (!pol.Order.RequisitionId.LineIds.Select(x => x.ProductId).Contains(pol.ProductId)) {
                poLinesWithoutRequisition.Add(pol);
                continue;
            }
            foreach (var line in pol.Order.RequisitionId.LineIds) {
                if (line.ProductId == pol.ProductId) {
                    pol.PriceUnit = line.ProductUomId.ComputePrice(line.PriceUnit, pol.ProductUom);
                    var partner = pol.Order.PartnerId != null ? Env.Model("Res.Partner").Call<ResPartner>("Browse", pol.Order.PartnerId) : pol.Order.RequisitionId.VendorId;
                    var paramsDict = new Dictionary<string, object>() { { "Order_Id", pol.Order } };
                    var seller = pol.ProductId._SelectSeller(partner, pol.ProductQty, pol.Order.DateOrder?.Date, line.ProductUomId, paramsDict);
                    if (pol.DatePlanned == null) {
                        pol.DatePlanned = pol._GetDatePlanned(seller).ToString(DEFAULT_SERVER_DATETIME_FORMAT);
                    }
                    var productCtx = new Dictionary<string, object>() { { "Seller_Id", seller.Id }, { "Lang", Env.Model("Res.Lang").Call("GetLang", new List<object>() { Env, partner.Lang }).Code } };
                    var name = pol._GetProductPurchaseDescription(pol.ProductId.WithContext(productCtx));
                    if (line.ProductDescriptionVariants != null) {
                        name += "\n" + line.ProductDescriptionVariants;
                    }
                    pol.Name = name;
                    break;
                }
            }
        }
        if (poLinesWithoutRequisition.Any()) {
            Env.Model("Purchase.PurchaseOrderLine").Call("_ComputePriceUnitAndDatePlannedAndName", poLinesWithoutRequisition);
        }
    }
    public virtual void ActionClearQuantities() {
        var zeroedLines = this.Where(l => !l.State.In("Cancel", "Purchase", "Done")).ToList();
        zeroedLines.ForEach(x => x.ProductQty = 0);
        if (this.Count() > zeroedLines.Count) {
            Env.Model("ir.actions.client").Call("Create", new Dictionary<string, object>() {
                { "Tag", "display_notification" },
                { "Params", new Dictionary<string, object>() { { "Title", _("Some not cleared") }, { "Message", _("Some quantities were not cleared because their status is not a RFQ status.") }, { "Sticky", false } } }
            });
        }
    }
    public virtual void ActionChoose() {
        var orderLines = this.Order.AlternativePOIds.SelectMany(x => x.OrderLine).Concat(this.Order.OrderLine).ToList();
        orderLines = orderLines.Where(l => l.ProductQty != 0 && l.ProductId == this.ProductId && !this.Contains(l)).ToList();
        if (orderLines.Any()) {
            orderLines.ActionClearQuantities();
        } else {
            Env.Model("ir.actions.client").Call("Create", new Dictionary<string, object>() {
                { "Tag", "display_notification" },
                { "Params", new Dictionary<string, object>() { { "Title", _("Nothing to clear") }, { "Message", _("There are no quantities to clear.") }, { "Sticky", false } } }
            });
        }
    }
}
