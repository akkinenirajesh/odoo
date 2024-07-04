csharp
public partial class SaleOrder {
  public void SetGridUp() {
    if (this.GridProductTemplateId != null) {
      this.GridUpdate = false;
      this.Grid = Json.Serialize(GetMatrix(this.GridProductTemplateId));
    }
  }

  public void ApplyGrid() {
    if (this.Grid != null && this.GridUpdate) {
      var grid = Json.Deserialize<Dictionary<string, object>>(this.Grid);
      var productTemplate = Env.Get<ProductTemplate>().Browse(grid["product_template_id"]);
      var dirtyCells = (List<Dictionary<string, object>>)grid["changes"];
      var attrib = Env.Get<ProductTemplateAttributeValue>();
      var defaultSoLineVals = new Dictionary<string, object>();
      var newLines = new List<Dictionary<string, object>>();

      foreach (var cell in dirtyCells) {
        var combination = attrib.Browse(cell["ptav_ids"]);
        var noVariantAttributeValues = combination - combination.WithoutNoVariantAttributes();

        var product = productTemplate.CreateProductVariant(combination);
        var orderLines = this.OrderLine.Where(line => line.ProductId == product.Id
          && line.ProductNoVariantAttributeValueIds.Select(x => x.Id).ToList() == noVariantAttributeValues.Select(x => x.Id).ToList()
        ).ToList();

        var oldQty = orderLines.Sum(x => x.ProductUomQty);
        var qty = (decimal)cell["qty"];
        var diff = qty - oldQty;

        if (diff == 0) {
          continue;
        }

        if (orderLines.Count > 0) {
          if (qty == 0) {
            if (this.State == "draft" || this.State == "sent") {
              this.OrderLine.Remove(orderLines);
            } else {
              orderLines.ForEach(x => x.ProductUomQty = 0);
            }
          } else {
            if (orderLines.Count > 1) {
              throw new Exception("You cannot change the quantity of a product present in multiple sale lines.");
            } else {
              orderLines[0].ProductUomQty = qty;
            }
          }
        } else {
          if (defaultSoLineVals.Count == 0) {
            defaultSoLineVals = Env.Get<SaleOrderLine>().DefaultGet(Env.Get<SaleOrderLine>().Fields.Keys.ToList());
          }
          var lastSequence = this.OrderLine.LastOrDefault().Sequence;
          if (lastSequence != null) {
            defaultSoLineVals["Sequence"] = lastSequence;
          }

          newLines.Add(new Dictionary<string, object> {
            {"product_id", product.Id},
            {"product_uom_qty", qty},
            {"product_no_variant_attribute_value_ids", noVariantAttributeValues.Select(x => x.Id).ToList()}
          });
          newLines.ForEach(x => x.Merge(defaultSoLineVals));
        }
      }

      if (newLines.Count > 0) {
        this.OrderLine.AddRange(newLines);
      }
    }
  }

  public Dictionary<string, object> GetMatrix(ProductTemplate productTemplate) {
    var matrix = productTemplate.GetTemplateMatrix(
      companyId: this.CompanyId,
      currencyId: this.CurrencyId,
      displayExtraPrice: true
    );

    if (this.OrderLine.Count > 0) {
      var lines = (List<List<Dictionary<string, object>>>)matrix["matrix"];
      var orderLines = this.OrderLine.Where(line => line.ProductTemplateId == productTemplate).ToList();

      foreach (var line in lines) {
        foreach (var cell in line) {
          if (cell["name"] == null) {
            var orderLine = orderLines.Where(line => line.ProductTemplateAttributeValueIds.Select(x => x.Id).ToList() == cell["ptav_ids"]).FirstOrDefault();
            if (orderLine != null) {
              cell["qty"] = orderLine.ProductUomQty;
            }
          }
        }
      }
    }

    return matrix;
  }

  public List<Dictionary<string, object>> GetReportMatrixes() {
    var matrixes = new List<Dictionary<string, object>>();

    if (this.ReportGrids) {
      var gridConfiguredTemplates = this.OrderLine.Where(x => x.IsConfigurableProduct).Select(x => x.ProductTemplateId).Distinct().ToList();
      gridConfiguredTemplates = gridConfiguredTemplates.Where(x => x.ProductAddMode == "matrix").ToList();

      foreach (var template in gridConfiguredTemplates) {
        if (this.OrderLine.Where(line => line.ProductTemplateId == template).ToList().Count > 1) {
          var matrix = GetMatrix(template);
          var matrixData = new List<List<Dictionary<string, object>>>();

          foreach (var row in (List<List<Dictionary<string, object>>>)matrix["matrix"]) {
            if (row[1].Where(x => x["qty"] != 0).ToList().Count > 0) {
              matrixData.Add(row);
            }
          }

          matrix["matrix"] = matrixData;
          matrixes.Add(matrix);
        }
      }
    }

    return matrixes;
  }
}
