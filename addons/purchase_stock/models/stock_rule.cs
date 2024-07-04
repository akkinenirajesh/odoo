csharp
public partial class PurchaseStock.StockRule
{
    public void ComputePickingTypeCodeDomain()
    {
        if (this.Action == "Buy")
        {
            this.PickingTypeCodeDomain = "incoming";
        }
        else
        {
            // Call base implementation for other actions
            // ...
        }
    }

    public void OnChangeAction()
    {
        if (this.Action == "Buy")
        {
            this.LocationSrcId = null;
        }
    }

    public void RunBuy(List<ProcurementGroup.Procurement> procurements)
    {
        var procurementsByPoDomain = new Dictionary<List<object>, List<ProcurementGroup.Procurement>>();
        var errors = new List<Tuple<ProcurementGroup.Procurement, string>>();

        foreach (var procurement in procurements)
        {
            var procurementDatePlanned = DateTime.Parse(procurement.Values["date_planned"]);

            var supplier = procurement.Values.ContainsKey("supplierinfo_id") ?
                Env.GetModel("Purchase.SupplierInfo").Browse(procurement.Values["supplierinfo_id"]) :
                (procurement.Values.ContainsKey("orderpoint_id") ?
                    Env.GetModel("Stock.Warehouse.Orderpoint").Browse(procurement.Values["orderpoint_id"]).Supplier :
                    procurement.Product.SelectSeller(
                        partnerId: procurement.Values.ContainsKey("supplierinfo_name") ?
                            Env.GetModel("Res.Partner").Browse(procurement.Values["supplierinfo_name"]) :
                            procurement.Values.ContainsKey("group_id") ?
                                Env.GetModel("Purchase.Group").Browse(procurement.Values["group_id"]).Partner :
                                null,
                        quantity: procurement.ProductQty,
                        date: DateTime.Now.Date > procurementDatePlanned.Date ? DateTime.Now.Date : procurementDatePlanned.Date,
                        uomId: procurement.ProductUom)
                );

            supplier = supplier ?? procurement.Product.PrepareSellers(false).Where(s => s.Company == null || s.Company == procurement.Company).FirstOrDefault();

            if (supplier == null)
            {
                errors.Add(new Tuple<ProcurementGroup.Procurement, string>(
                    procurement,
                    $"There is no matching vendor price to generate the purchase order for product {procurement.Product.DisplayName} (no vendor defined, minimum quantity not reached, dates not valid, ...). Go on the product form and complete the list of vendors."
                ));
                continue;
            }

            var partner = supplier.Partner;
            procurement.Values["supplier"] = supplier;
            procurement.Values["propagateCancel"] = this.PropagateCancel;

            var domain = this.MakePoGetDomain(procurement.Company, procurement.Values, partner);
            if (!procurementsByPoDomain.ContainsKey(domain))
            {
                procurementsByPoDomain.Add(domain, new List<ProcurementGroup.Procurement>());
            }
            procurementsByPoDomain[domain].Add(procurement);
        }

        if (errors.Count > 0)
        {
            throw new ProcurementException(errors);
        }

        foreach (var kvp in procurementsByPoDomain)
        {
            var domain = kvp.Key;
            var procurements = kvp.Value;

            var origins = new HashSet<string>(procurements.Select(p => p.Origin));
            var po = Env.GetModel("Purchase.Order").Search(domain, limit: 1);
            var companyId = procurements[0].Company.Id;

            if (po == null)
            {
                var positiveValues = procurements.Where(p => p.ProductQty >= 0).Select(p => p.Values).ToList();
                if (positiveValues.Count > 0)
                {
                    var vals = this.PreparePurchaseOrder(companyId, origins, positiveValues);
                    po = Env.GetModel("Purchase.Order").WithCompany(companyId).WithUser(Env.SUPERUSER_ID).Create(vals);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(po.Origin))
                {
                    var missingOrigins = origins.Except(po.Origin.Split(',').Select(s => s.Trim())).ToList();
                    if (missingOrigins.Count > 0)
                    {
                        po.Write(new Dictionary<string, object> { { "Origin", po.Origin + ", " + string.Join(", ", missingOrigins) } });
                    }
                }
                else
                {
                    po.Write(new Dictionary<string, object> { { "Origin", string.Join(", ", origins) } });
                }
            }

            var procurementsToMerge = this.GetProcurementsToMerge(procurements);
            procurements = this.MergeProcurements(procurementsToMerge);

            var poLinesByProduct = new Dictionary<long, Purchase.Order.Line>();
            foreach (var group in procurements.GroupBy(p => p.Product.Id))
            {
                poLinesByProduct.Add(group.Key, Env.GetModel("Purchase.Order.Line").Concat(group.Select(p => p)));
            }

            var poLineValues = new List<Dictionary<string, object>>();
            foreach (var procurement in procurements)
            {
                var poLines = poLinesByProduct.ContainsKey(procurement.Product.Id) ? poLinesByProduct[procurement.Product.Id] : Env.GetModel("Purchase.Order.Line");
                var poLine = poLines.FindCandidate(procurement);

                if (poLine != null)
                {
                    var vals = this.UpdatePurchaseOrderLine(procurement.Product, procurement.ProductQty, procurement.ProductUom, companyId, procurement.Values, poLine);
                    poLine.Write(vals);
                }
                else
                {
                    if (procurement.ProductQty <= 0)
                    {
                        continue;
                    }

                    var partner = procurement.Values["supplier"].Partner;
                    poLineValues.Add(Env.GetModel("Purchase.Order.Line").PreparePurchaseOrderLineFromProcurement(procurement, po));

                    var orderDatePlanned = procurement.Values["date_planned"];
                    var orderDate = DateTime.Parse(orderDatePlanned).AddDays(-1 * procurement.Values["supplier"].Delay);
                    if (DateTime.Parse(po.DateOrder).Date < orderDate.Date)
                    {
                        po.DateOrder = orderDate.ToString();
                    }
                }
            }

            Env.GetModel("Purchase.Order.Line").Create(poLineValues);
        }
    }

    public List<Tuple<long, Purchase.Order.Line>> GetProcurementsToMergeGroupBy(ProcurementGroup.Procurement procurement)
    {
        return new List<Tuple<long, Purchase.Order.Line>>
        {
            new Tuple<long, Purchase.Order.Line>(procurement.Product.Id, procurement.ProductUom),
            procurement.Values.ContainsKey("orderpoint_id") && !procurement.Values.ContainsKey("move_dest_ids") ?
                new Tuple<long, Purchase.Order.Line>(procurement.Values["orderpoint_id"], null) :
                null
        };
    }

    public List<ProcurementGroup.Procurement> GetProcurementsToMerge(List<ProcurementGroup.Procurement> procurements)
    {
        var groupedProcurements = new List<List<ProcurementGroup.Procurement>>();
        foreach (var group in procurements.GroupBy(p => this.GetProcurementsToMergeGroupBy(p)))
        {
            groupedProcurements.Add(group.ToList());
        }

        return groupedProcurements;
    }

    public List<ProcurementGroup.Procurement> MergeProcurements(List<List<ProcurementGroup.Procurement>> procurementsToMerge)
    {
        var mergedProcurements = new List<ProcurementGroup.Procurement>();
        foreach (var procurements in procurementsToMerge)
        {
            var quantity = 0.0;
            var moveDestIds = Env.GetModel("Stock.Move");
            var orderpointId = Env.GetModel("Stock.Warehouse.Orderpoint");
            foreach (var procurement in procurements)
            {
                if (procurement.Values.ContainsKey("move_dest_ids"))
                {
                    moveDestIds |= Env.GetModel("Stock.Move").Browse(procurement.Values["move_dest_ids"]);
                }

                if (orderpointId == null && procurement.Values.ContainsKey("orderpoint_id"))
                {
                    orderpointId = Env.GetModel("Stock.Warehouse.Orderpoint").Browse(procurement.Values["orderpoint_id"]);
                }

                quantity += procurement.ProductQty;
            }

            var values = new Dictionary<string, object>(procurements[0].Values);
            values["move_dest_ids"] = moveDestIds;
            values["orderpoint_id"] = orderpointId;

            var mergedProcurement = new ProcurementGroup.Procurement(
                procurements[0].Product,
                quantity,
                procurements[0].ProductUom,
                procurements[0].LocationId,
                procurements[0].Name,
                procurements[0].Origin,
                procurements[0].Company,
                values
            );

            mergedProcurements.Add(mergedProcurement);
        }

        return mergedProcurements;
    }

    public Dictionary<string, object> UpdatePurchaseOrderLine(ProductProduct.Product product, double productQty, ProductProduct.ProductUom productUom, Res.Company company, Dictionary<string, object> values, Purchase.Order.Line line)
    {
        var partner = values["supplier"].Partner;

        var procurementUomPoQty = productUom.ComputeQuantity(productQty, product.UomPoId, "HALF-UP");
        var seller = product.WithCompany(company).SelectSeller(
            partnerId: partner,
            quantity: line.ProductQty + procurementUomPoQty,
            date: line.Order.DateOrder != null ? DateTime.Parse(line.Order.DateOrder).Date : DateTime.Now.Date,
            uomId: product.UomPoId
        );

        var priceUnit = seller != null ?
            Env.GetModel("Account.Tax").FixTaxIncludedPriceCompany(seller.Price, product.SupplierTaxesId, line.TaxesId, company) :
            0.0;

        if (priceUnit > 0 && seller != null && line.Order.Currency != seller.Currency)
        {
            priceUnit = seller.Currency.Convert(priceUnit, line.Order.Currency, line.Order.Company, DateTime.Now.Date);
        }

        var res = new Dictionary<string, object>
        {
            { "ProductQty", line.ProductQty + procurementUomPoQty },
            { "PriceUnit", priceUnit },
            { "MoveDestIds", values.ContainsKey("move_dest_ids") ? values["move_dest_ids"].Cast<long>().Select(id => new Tuple<string, long>("4", id)).ToList() : new List<Tuple<string, long>>() }
        };

        if (values.ContainsKey("orderpoint_id"))
        {
            res["orderpoint_id"] = values["orderpoint_id"];
        }

        return res;
    }

    public Dictionary<string, object> PreparePurchaseOrder(Res.Company company, HashSet<string> origins, List<Dictionary<string, object>> values)
    {
        var purchaseDate = values.Min(v => v.ContainsKey("date_order") ?
            DateTime.Parse(v["date_order"]) :
            DateTime.Parse(v["date_planned"]).AddDays(-1 * v["supplier"].Delay));

        var partner = values[0]["supplier"].Partner;

        var fpos = Env.GetModel("Account.Fiscal.Position").WithCompany(company).GetFiscalPosition(partner);

        var group = this.GroupPropagationOption == "Fixed" ?
            this.GroupId :
            (this.GroupPropagationOption == "Propagate" && values[0].ContainsKey("group_id") ?
                values[0]["group_id"] :
                null);

        return new Dictionary<string, object>
        {
            { "PartnerId", partner.Id },
            { "UserId", partner.Buyer.Id },
            { "PickingTypeId", this.PickingTypeId.Id },
            { "CompanyId", company.Id },
            { "CurrencyId", partner.WithCompany(company).PropertyPurchaseCurrencyId?.Id ?? company.CurrencyId.Id },
            { "DestAddressId", values[0].ContainsKey("partner_id") ? values[0]["partner_id"] : null },
            { "Origin", string.Join(", ", origins) },
            { "PaymentTermId", partner.WithCompany(company).PropertySupplierPaymentTermId?.Id },
            { "DateOrder", purchaseDate.ToString() },
            { "FiscalPositionId", fpos.Id },
            { "GroupId", group }
        };
    }

    public List<object> MakePoGetDomain(Res.Company company, Dictionary<string, object> values, Res.Partner partner)
    {
        var group = this.GroupPropagationOption == "Fixed" ?
            this.GroupId :
            (this.GroupPropagationOption == "Propagate" && values.ContainsKey("group_id") ?
                values["group_id"] :
                null);

        var domain = new List<object>
        {
            new Tuple<string, object>("partner_id", partner.Id),
            new Tuple<string, object>("state", "draft"),
            new Tuple<string, object>("picking_type_id", this.PickingTypeId.Id),
            new Tuple<string, object>("company_id", company.Id),
            new Tuple<string, object>("user_id", partner.Buyer.Id)
        };

        var deltaDays = Env.GetConfigParameter("purchase_stock.delta_days_merge");
        if (values.ContainsKey("orderpoint_id") && deltaDays != null)
        {
            var procurementDate = DateTime.Parse(values["date_planned"]).AddDays(-1 * values["supplier"].Delay);
            var delta = int.Parse(deltaDays);
            domain.Add(new Tuple<string, object>("date_order", "<=", (procurementDate.Date.AddDays(delta)).ToString()));
            domain.Add(new Tuple<string, object>("date_order", ">=", (procurementDate.Date.AddDays(-delta)).ToString()));
        }

        if (group != null)
        {
            domain.Add(new Tuple<string, object>("group_id", group.Id));
        }

        return domain;
    }

    public Dictionary<string, object> PushPrepareMoveCopyValues(Stock.Move moveToCopy, string newDate)
    {
        var res = new Dictionary<string, object>(base.PushPrepareMoveCopyValues(moveToCopy, newDate));
        res["purchase_line_id"] = null;
        if (this.LocationDestId.Usage == "supplier")
        {
            res["purchase_line_id"] = moveToCopy.GetPurchaseLineAndPartnerFromChain()[0];
            res["partner_id"] = moveToCopy.GetPurchaseLineAndPartnerFromChain()[1];
        }

        return res;
    }

    public Tuple<long, long> GetPurchaseLineAndPartnerFromChain()
    {
        // ...
    }

    public Dictionary<string, object> GetLeadDays(ProductProduct.Product product, Dictionary<string, object> values)
    {
        var delays = new Dictionary<string, double>();
        var delayDescription = new List<Tuple<string, string>>();

        // ...

        return new Dictionary<string, object> { { "delays", delays }, { "delayDescription", delayDescription } };
    }
}
