csharp
public partial class AccountMoveLine {
    // all the model methods are written here.
    public void CopyDataExtendBusinessFields(Dictionary<string, object> values) {
        // OVERRIDE to copy the 'SaleLineIds' field as well.
        var baseValues = new Dictionary<string, object>();
        baseValues = this.Env.CallMethod("copy_data_extend_business_fields", baseValues);
        values["SaleLineIds"] = new List<object> { new { command = 6, ids = this.SaleLineIds.Ids } };
    }

    public List<object> PrepareAnalyticLines() {
        // Note: This method is called only on the move.line that having an analytic distribution, and
        // so that should create analytic entries.
        var valuesList = this.Env.CallMethod("prepare_analytic_lines", new List<object>());

        // filter the move lines that can be reinvoiced: a cost (negative amount) analytic line without SO line but with a product can be reinvoiced
        var moveToReinvoice = this.Env.Create("account.move.line");
        if (valuesList.Count > 0) {
            for (var index = 0; index < this.Ids.Count; index++) {
                var values = valuesList[index];
                if (!values.ContainsKey("so_line")) {
                    if (this.Ids[index].SaleCanToBeReinvoice()) {
                        moveToReinvoice = moveToReinvoice.Add(this.Ids[index]);
                    }
                }
            }
        }

        // insert the sale line in the create values of the analytic entries
        if (moveToReinvoice.Filtered(aml => !aml.MoveId.ReversedEntryId)) {  // only if the move line is not a reversal one
            var mapSaleLinePerMove = moveToReinvoice.SaleCreateReinvoiceSaleLine();
            for (var i = 0; i < valuesList.Count; i++) {
                var saleLine = mapSaleLinePerMove.GetValueOrDefault(valuesList[i]["move_line_id"]);
                if (saleLine != null) {
                    valuesList[i]["so_line"] = saleLine.Id;
                }
            }
        }

        return valuesList;
    }

    public bool SaleCanToBeReinvoice() {
        // determine if the generated analytic line should be reinvoiced or not.
        // For Vendor Bill flow, if the product has a 'erinvoice policy' and is a cost, then we will find the SO on which reinvoice the AAL
        if (this.SaleLineIds.Count > 0) {
            return false;
        }
        var uomPrecisionDigits = this.Env.CallMethod("decimal.precision", "precision_get", new List<object> { "Product Unit of Measure" });
        return this.Env.CallMethod("float_compare", new List<object> { this.Credit ?? 0.0, this.Debit ?? 0.0, uomPrecisionDigits }) != 1 && this.ProductId.ExpensePolicy != "no";
    }

    public Dictionary<int, SaleOrderLine> SaleCreateReinvoiceSaleLine() {
        var saleOrderMap = this.SaleDetermineOrder();

        var saleLineValuesToCreate = new List<Dictionary<string, object>>();  // the list of creation values of sale line to create.
        var existingSaleLineCache = new Dictionary<Tuple<int, int, decimal>, object>();  // in the sales_price-delivery case, we can reuse the same sale line. This cache will avoid doing a search each time the case happen
        // `mapMoveSaleLine` is map where
        //   - key is the move line identifier
        //   - value is either a sale.order.line record (existing case), or an integer representing the index of the sale line to create in
        //     the `saleLineValuesToCreate` (not existing case, which will happen more often than the first one).
        var mapMoveSaleLine = new Dictionary<int, object>();

        foreach (var moveLine in this.Ids) {
            var saleOrder = saleOrderMap.GetValueOrDefault(moveLine.Id);

            // no reinvoice as no sales order was found
            if (saleOrder == null) {
                continue;
            }

            // raise if the sale order is not currently open
            if (saleOrder.State == "draft" || saleOrder.State == "sent") {
                throw new Exception("The Sales Order " + saleOrder.Name + " linked to the Analytic Account " + saleOrder.AnalyticAccountId.Name + " must be validated before registering expenses.");
            } else if (saleOrder.State == "cancel") {
                throw new Exception("The Sales Order " + saleOrder.Name + " linked to the Analytic Account " + saleOrder.AnalyticAccountId.Name + " is cancelled. You cannot register an expense on a cancelled Sales Order.");
            } else if (saleOrder.Locked) {
                throw new Exception("The Sales Order " + saleOrder.Name + " linked to the Analytic Account " + saleOrder.AnalyticAccountId.Name + " is currently locked. You cannot register an expense on a locked Sales Order. Please create a new SO linked to this Analytic Account.");
            }

            var price = moveLine.SaleGetInvoicePrice(saleOrder);

            // find the existing sale.line or keep its creation values to process this in batch
            SaleOrderLine saleLine = null;
            if (moveLine.ProductId.ExpensePolicy == "sales_price" && moveLine.ProductId.InvoicePolicy == "delivery" && !this.Env.Context.Get("force_split_lines")) {
                // for those case only, we can try to reuse one
                var mapEntryKey = new Tuple<int, int, decimal>(saleOrder.Id, moveLine.ProductId.Id, price);  // cache entry to limit the call to search
                if (existingSaleLineCache.ContainsKey(mapEntryKey)) {  // already search, so reuse it. sale_line can be sale.order.line record or index of a "to create values" in `saleLineValuesToCreate`
                    mapMoveSaleLine[moveLine.Id] = existingSaleLineCache[mapEntryKey];
                    existingSaleLineCache[mapEntryKey] = existingSaleLineCache[mapEntryKey];
                } else {  // search for existing sale line
                    saleLine = this.Env.SearchOne("sale.order.line", new List<object> { new { field = "order_id", value = saleOrder.Id }, new { field = "price_unit", value = price }, new { field = "product_id", value = moveLine.ProductId.Id }, new { field = "is_expense", value = true } });
                    if (saleLine != null) {  // found existing one, so keep the browse record
                        mapMoveSaleLine[moveLine.Id] = existingSaleLineCache[mapEntryKey] = saleLine;
                    } else {  // should be create, so use the index of creation values instead of browse record
                        // save value to create it
                        saleLineValuesToCreate.Add(moveLine.SalePrepareSaleLineValues(saleOrder, price));
                        // store it in the cache of existing ones
                        existingSaleLineCache[mapEntryKey] = saleLineValuesToCreate.Count - 1;  // save the index of the value to create sale line
                        // store it in the map_move_sale_line map
                        mapMoveSaleLine[moveLine.Id] = saleLineValuesToCreate.Count - 1;  // save the index of the value to create sale line
                    }
                }
            } else {  // save its value to create it anyway
                saleLineValuesToCreate.Add(moveLine.SalePrepareSaleLineValues(saleOrder, price));
                mapMoveSaleLine[moveLine.Id] = saleLineValuesToCreate.Count - 1;  // save the index of the value to create sale line
            }
        }

        // create the sale lines in batch
        var newSaleLines = this.Env.Create("sale.order.line", saleLineValuesToCreate);

        // build result map by replacing index with newly created record of sale.order.line
        var result = new Dictionary<int, SaleOrderLine>();
        foreach (var moveLineId in mapMoveSaleLine.Keys) {
            if (mapMoveSaleLine[moveLineId] is int) {  // index of newly created sale line
                result[moveLineId] = newSaleLines[mapMoveSaleLine[moveLineId]];
            } else if (mapMoveSaleLine[moveLineId] is SaleOrderLine) {  // already record of sale.order.line
                result[moveLineId] = mapMoveSaleLine[moveLineId];
            }
        }
        return result;
    }

    public Dictionary<int, SaleOrder> SaleDetermineOrder() {
        // Get the mapping of move.line with the sale.order record on which its analytic entries should be reinvoiced
        // :return a dict where key is the move line id, and value is sale.order record (or None).
        var mapping = new Dictionary<int, SaleOrder>();
        foreach (var moveLine in this.Ids) {
            if (moveLine.AnalyticDistribution != null) {
                var distributionJson = moveLine.AnalyticDistribution;
                var accountIds = distributionJson.Keys.SelectMany(key => key.Split(',')).Select(account => Convert.ToInt32(account)).ToList();

                var saleOrder = this.Env.SearchOne("sale.order", new List<object> { new { field = "analytic_account_id", operator = "in", value = accountIds }, new { field = "state", value = "sale" } }, "create_date ASC");
                if (saleOrder != null) {
                    mapping[moveLine.Id] = saleOrder;
                } else {
                    saleOrder = this.Env.SearchOne("sale.order", new List<object> { new { field = "analytic_account_id", operator = "in", value = accountIds } }, "create_date ASC");
                    mapping[moveLine.Id] = saleOrder;
                }
            }
        }
        // map of AAL index with the SO on which it needs to be reinvoiced. Maybe be None if no SO found
        return mapping;
    }

    public Dictionary<string, object> SalePrepareSaleLineValues(SaleOrder order, decimal price) {
        // Generate the sale.line creation value from the current move line
        var lastSoLine = this.Env.SearchOne("sale.order.line", new List<object> { new { field = "order_id", value = order.Id } }, "sequence DESC");
        var lastSequence = lastSoLine != null ? lastSoLine.Sequence + 1 : 100;

        var fpos = order.FiscalPositionId ?? order.FiscalPositionId.GetFiscalPosition(order.PartnerId);
        var productTaxes = this.ProductId.TaxesId.FilterTaxesByCompany(order.CompanyId);
        var taxes = fpos.MapTax(productTaxes);

        return new Dictionary<string, object> {
            { "order_id", order.Id },
            { "name", this.Name },
            { "sequence", lastSequence },
            { "price_unit", price },
            { "tax_id", taxes.Select(x => x.Id).ToList() },
            { "discount", 0.0 },
            { "product_id", this.ProductId.Id },
            { "product_uom", this.ProductUomId.Id },
            { "product_uom_qty", 0.0 },
            { "is_expense", true },
        };
    }

    public decimal SaleGetInvoicePrice(SaleOrder order) {
        // Based on the current move line, compute the price to reinvoice the analytic line that is going to be created (so the
        // price of the sale line).
        var unitAmount = this.Quantity;
        var amount = (this.Credit ?? 0.0) - (this.Debit ?? 0.0);

        if (this.ProductId.ExpensePolicy == "sales_price") {
            return order.PricelistId.GetProductPrice(this.ProductId, 1.0, this.ProductUomId, order.DateOrder);
        }

        var uomPrecisionDigits = this.Env.CallMethod("decimal.precision", "precision_get", new List<object> { "Product Unit of Measure" });
        if (this.Env.CallMethod("float_is_zero", new List<object> { unitAmount, uomPrecisionDigits })) {
            return 0.0;
        }

        // Prevent unnecessary currency conversion that could be impacted by exchange rate
        // fluctuations
        if (this.CompanyId.CurrencyId != null && amount != null && this.CompanyId.CurrencyId == order.CurrencyId) {
            return this.CompanyId.CurrencyId.Round(Math.Abs(amount / unitAmount));
        }

        var priceUnit = Math.Abs(amount / unitAmount);
        var currencyId = this.CompanyId.CurrencyId;
        if (currencyId != null && currencyId != order.CurrencyId) {
            priceUnit = currencyId.Convert(priceUnit, order.CurrencyId, order.CompanyId, order.DateOrder ?? DateTime.Now);
        }
        return priceUnit;
    }

    public List<AccountMoveLine> GetDownpaymentLines() {
        // OVERRIDE
        return this.SaleLineIds.Filtered(line => line.IsDownpayment).InvoiceLines.Filtered(line => line.MoveId.IsDownpayment());
    }
}
