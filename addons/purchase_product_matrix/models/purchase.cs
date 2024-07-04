C#
public partial class PurchaseOrder
{
    public bool ReportGrids { get; set; }
    public Product.ProductTemplate GridProductTemplateId { get; set; }
    public bool GridUpdate { get; set; }
    public string Grid { get; set; }

    public void SetGridUp()
    {
        if (GridProductTemplateId != null)
        {
            GridUpdate = false;
            Grid = System.Text.Json.JsonSerializer.Serialize(GetMatrix(GridProductTemplateId));
        }
    }

    public bool MustDeleteDatePlanned(string fieldName)
    {
        return base.MustDeleteDatePlanned(fieldName) || fieldName == "Grid";
    }

    public void ApplyGrid()
    {
        if (Grid != null && GridUpdate)
        {
            var grid = System.Text.Json.JsonSerializer.Deserialize<dynamic>(Grid);
            var productTemplate = Env.Get("Product.ProductTemplate").Browse(grid["ProductTemplateId"]);
            var productIds = new System.Collections.Generic.HashSet<int>();
            var dirtyCells = grid["Changes"];
            var attrib = Env.Get("Product.ProductTemplateAttributeValue");
            var defaultPoLineVals = new System.Collections.Generic.Dictionary<string, object>();
            var newLines = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>();
            foreach (var cell in dirtyCells)
            {
                var combination = attrib.Browse(cell["PtavIds"]);
                var noVariantAttributeValues = combination - combination.WithoutNoVariantAttributes();

                // create or find product variant from combination
                var product = productTemplate.CreateProductVariant(combination);
                // TODO replace the check on product_id by a first check on the ptavs and pnavs?
                // and only create/require variant after no line has been found ???
                var orderLines = this.OrderLine.Where(line => (line.Original == null || line.Original) && line.ProductId == product && (line.Original == null || line.Original).ProductNoVariantAttributeValueIds == noVariantAttributeValues).ToList();

                // if product variant already exist in order lines
                var oldQty = orderLines.Sum(orderLine => orderLine.ProductQty);
                var qty = cell["Qty"];
                var diff = qty - oldQty;

                if (diff == 0)
                {
                    continue;
                }

                productIds.Add(product.Id);

                if (orderLines.Any())
                {
                    if (qty == 0)
                    {
                        if (this.State == "draft" || this.State == "sent")
                        {
                            // Remove lines if qty was set to 0 in matrix
                            // only if PO state = draft/sent
                            this.OrderLine = this.OrderLine.Where(line => !orderLines.Contains(line)).ToList();
                        }
                        else
                        {
                            orderLines.ForEach(orderLine => orderLine.ProductQty = 0.0);
                        }
                    }
                    else
                    {
                        """
                        When there are multiple lines for same product and its quantity was changed in the matrix,
                        An error is raised.

                        A 'good' strategy would be to:
                            * Sets the quantity of the first found line to the cell value
                            * Remove the other lines.

                        But this would remove all business logic linked to the other lines...
                        Therefore, it only raises an Error for now.
                        """
                        if (orderLines.Count > 1)
                        {
                            throw new System.Exception("You cannot change the quantity of a product present in multiple purchase lines.");
                        }
                        else
                        {
                            orderLines[0].ProductQty = qty;
                            // If we want to support multiple lines edition:
                            // removal of other lines.
                            // For now, an error is raised instead
                            // if len(order_lines) > 1:
                            //     # Remove 1+ lines
                            //     self.order_line -= order_lines[1:]
                        }
                    }
                }
                else
                {
                    if (!defaultPoLineVals.Any())
                    {
                        var orderLine = Env.Get("Purchase.PurchaseOrderLine");
                        defaultPoLineVals = orderLine.DefaultGet(orderLine.Fields.Keys);
                    }
                    var lastSequence = this.OrderLine.LastOrDefault()?.Sequence;
                    if (lastSequence != null)
                    {
                        defaultPoLineVals["Sequence"] = lastSequence;
                    }
                    newLines.Add(new System.Collections.Generic.Dictionary<string, object>()
                    {
                        { "defaultPoLineVals", defaultPoLineVals },
                        { "ProductId", product.Id },
                        { "ProductQty", qty },
                        { "ProductNoVariantAttributeValueIds", noVariantAttributeValues.Ids }
                    });
                }
            }
            if (productIds.Any())
            {
                if (newLines.Any())
                {
                    // Add new PO lines
                    this.Update(new System.Collections.Generic.Dictionary<string, object>() { { "OrderLine", newLines } });
                }

                // Recompute prices for new/modified lines:
                foreach (var line in this.OrderLine.Where(line => productIds.Contains(line.ProductId.Id)))
                {
                    line.Product.IdChange();
                    line.OnchangeProductIdWarning();
                }
            }
        }
    }

    private dynamic GetMatrix(Product.ProductTemplate productTemplate)
    {
        var matrix = productTemplate.GetTemplateMatrix(company: this.CompanyId, currency: this.CurrencyId);
        if (this.OrderLine.Any())
        {
            var lines = matrix["Matrix"];
            var orderLines = this.OrderLine.Where(line => line.ProductTemplateId == productTemplate).ToList();
            foreach (var line in lines)
            {
                foreach (var cell in line)
                {
                    if (cell["Name"] == null)
                    {
                        var lineMatch = orderLines.FirstOrDefault(line => HasPtavs(line, cell["PtavIds"]));
                        if (lineMatch != null)
                        {
                            cell["Qty"] = lineMatch.ProductQty;
                        }
                    }
                }
            }
        }
        return matrix;
    }

    private bool HasPtavs(PurchaseOrderLine line, System.Collections.Generic.IEnumerable<int> sortedAttrIds)
    {
        var ptav = line.ProductTemplateAttributeValueIds.Ids;
        var pnav = line.ProductNoVariantAttributeValueIds.Ids;
        var pav = pnav.Concat(ptav).ToList();
        pav.Sort();
        return Enumerable.SequenceEqual(pav, sortedAttrIds);
    }

    public System.Collections.Generic.List<dynamic> GetReportMatrixes()
    {
        var matrixes = new System.Collections.Generic.List<dynamic>();
        if (this.ReportGrids)
        {
            var gridConfiguredTemplates = this.OrderLine.Where(line => line.IsConfigurableProduct).Select(line => line.ProductTemplateId).ToList();
            // TODO is configurable product and product_variant_count > 1
            // configurable products are only configured through the matrix in purchase, so no need to check product_add_mode.
            foreach (var template in gridConfiguredTemplates)
            {
                if (this.OrderLine.Count(line => line.ProductTemplateId == template) > 1)
                {
                    var matrix = GetMatrix(template);
                    var matrixData = new System.Collections.Generic.List<dynamic>();
                    foreach (var row in matrix["Matrix"])
                    {
                        if (row[1:].Any(column => column["Qty"] != 0))
                        {
                            matrixData.Add(row);
                        }
                    }
                    matrix["Matrix"] = matrixData;
                    matrixes.Add(matrix);
                }
            }
        }
        return matrixes;
    }
}

public partial class PurchaseOrderLine
{
    public Product.ProductTemplate ProductTemplateId { get; set; }
    public bool IsConfigurableProduct { get; set; }
    public System.Collections.Generic.List<Product.ProductTemplateAttributeValue> ProductTemplateAttributeValueIds { get; set; }
    public System.Collections.Generic.List<Product.ProductTemplateAttributeValue> ProductNoVariantAttributeValueIds { get; set; }

    public string GetProductPurchaseDescription(Product.Product Product)
    {
        var name = base.GetProductPurchaseDescription(Product);
        foreach (var noVariantAttributeValue in this.ProductNoVariantAttributeValueIds)
        {
            name += "\n" + noVariantAttributeValue.AttributeId.Name + ": " + noVariantAttributeValue.Name;
        }
        return name;
    }
}
