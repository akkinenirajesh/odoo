csharp
public partial class MrpAccount.ProductTemplate
{
  public Account.Account GetProductAccounts()
  {
    var accounts = base.GetProductAccounts();
    accounts["production"] = this.CategId.PropertyStockAccountProductionCostId;
    return accounts;
  }

  public object ActionBomCost()
  {
    if (this.ProductVariantCount == 1 && this.BomCount > 0)
    {
      return this.ProductVariantId.ActionBomCost();
    }
    return null;
  }

  public object ButtonBomCost()
  {
    if (this.ProductVariantCount == 1 && this.BomCount > 0)
    {
      return this.ProductVariantId.ButtonBomCost();
    }
    return null;
  }
}

public partial class MrpAccount.ProductProduct
{
  public void ButtonBomCost()
  {
    this.SetPriceFromBom();
  }

  public void ActionBomCost()
  {
    var bomsToRecompute = Env.Get("Mrp.Bom").Search("[|", ("ProductId", "in", this.Ids), "&", ("ProductId", "=", false), ("ProductTmplId", "in", this.Mapped("ProductTmplId").Ids)];
    foreach (var product in this)
    {
      product.SetPriceFromBom(bomsToRecompute);
    }
  }

  public void SetPriceFromBom(Mrp.Bom bomsToRecompute = null)
  {
    var bom = Env.Get("Mrp.Bom")._BomFind(this)[this];
    if (bom != null)
    {
      this.StandardPrice = this.ComputeBomPrice(bom, bomsToRecompute);
    }
    else
    {
      bom = Env.Get("Mrp.Bom").Search("[('ByproductIds.ProductId', '=', this.Id)]", order: "Sequence, ProductId, Id", limit: 1);
      if (bom != null)
      {
        var price = this.ComputeBomPrice(bom, bomsToRecompute, byproductBom: true);
        if (price != 0)
        {
          this.StandardPrice = price;
        }
      }
    }
  }

  public double ComputeAveragePrice(double qtyInvoiced, double qtyToInvoice, Stock.Move stockMoves, bool isReturned = false)
  {
    if (stockMoves.ProductId == this)
    {
      return base.ComputeAveragePrice(qtyInvoiced, qtyToInvoice, stockMoves, isReturned);
    }
    var bom = Env.Get("Mrp.Bom")._BomFind(this, companyId: stockMoves.CompanyId.Id, bomType: "phantom")[this];
    if (bom == null)
    {
      return base.ComputeAveragePrice(qtyInvoiced, qtyToInvoice, stockMoves, isReturned);
    }
    double value = 0;
    var (dummy, bomLines) = bom.Explode(this, 1);
    bomLines = bomLines.ToDictionary(x => x.Key, x => x.Value);
    foreach (var (bomLine, movesList) in stockMoves.Where(sm => sm.State != "cancel").GroupBy(sm => sm.BomLineId))
    {
      if (!bomLines.ContainsKey(bomLine))
      {
        foreach (var move in movesList)
        {
          value += move.ProductQty * move.ProductId.ComputeAveragePrice(qtyInvoiced * move.ProductQty, qtyToInvoice * move.ProductQty, move, isReturned);
        }
        continue;
      }
      var lineQty = bomLine.ProductUomId.ComputeQuantity(bomLines[bomLine]["qty"], bomLine.ProductId.UomId);
      var moves = Env.Get("Stock.Move").Concat(movesList.ToArray());
      value += lineQty * bomLine.ProductId.ComputeAveragePrice(qtyInvoiced * lineQty, qtyToInvoice * lineQty, moves, isReturned);
    }
    return value;
  }

  public double ComputeBomPrice(Mrp.Bom bom, Mrp.Bom bomsToRecompute = null, bool byproductBom = false)
  {
    if (bom == null)
    {
      return 0;
    }
    if (bomsToRecompute == null)
    {
      bomsToRecompute = new Mrp.Bom[0];
    }
    double total = 0;
    foreach (var opt in bom.OperationIds)
    {
      if (opt.SkipOperationLine(this))
      {
        continue;
      }
      var durationExpected = (opt.WorkcenterId.GetExpectedDuration(this) + opt.TimeCycle * 100 / opt.WorkcenterId.TimeEfficiency);
      total += (durationExpected / 60) * opt.TotalCostPerHour();
    }
    foreach (var line in bom.BomLineIds)
    {
      if (line.SkipBomLine(this))
      {
        continue;
      }
      if (line.ChildBomId != null && line.ChildBomId.IsIn(bomsToRecompute))
      {
        var childTotal = line.ProductId.ComputeBomPrice(line.ChildBomId, bomsToRecompute);
        total += line.ProductId.UomId.ComputePrice(childTotal, line.ProductUomId) * line.ProductQty;
      }
      else
      {
        total += line.ProductId.UomId.ComputePrice(line.ProductId.StandardPrice, line.ProductUomId) * line.ProductQty;
      }
    }
    if (byproductBom)
    {
      var byproductLines = bom.ByproductIds.Where(b => b.ProductId == this && b.CostShare != 0);
      double productUomQty = 0;
      foreach (var line in byproductLines)
      {
        productUomQty += line.ProductUomId.ComputeQuantity(line.ProductQty, this.UomId, round: false);
      }
      var byproductCostShare = byproductLines.Sum(b => b.CostShare);
      if (byproductCostShare != 0 && productUomQty != 0)
      {
        return total * byproductCostShare / 100 / productUomQty;
      }
    }
    else
    {
      var byproductCostShare = bom.ByproductIds.Sum(b => b.CostShare);
      if (byproductCostShare != 0)
      {
        total *= Math.Round(1 - byproductCostShare / 100, 4);
      }
      return bom.ProductUomId.ComputePrice(total / bom.ProductQty, this.UomId);
    }
    return 0;
  }
}

public partial class MrpAccount.ProductCategory
{
  public List<string> GetStockAccountPropertyFieldNames()
  {
    return base.GetStockAccountPropertyFieldNames().Append("ProductionAccount").ToList();
  }
}
