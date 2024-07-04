csharp
public partial class StockQuant {
  public decimal Value { get; set; }
  public Core.Currency CurrencyId { get; set; }
  public DateTime AccountingDate { get; set; }
  public string CostMethod { get; set; }

  public void ComputeValue() {
    this.CurrencyId = Env.Company.CurrencyId;
    if (this.LocationId == null || this.ProductId == null || 
        !this.LocationId.ShouldNotBeValued() ||
        this.ShouldExcludeForValuation() ||
        Env.FloatUtils.IsZero(this.Quantity, this.ProductId.UomId.Rounding)) {
      this.Value = 0;
      return;
    }
    decimal quantity = this.ProductId.WithCompany(this.CompanyId).QuantitySvl;
    if (Env.FloatUtils.IsZero(quantity, this.ProductId.UomId.Rounding)) {
      this.Value = 0;
      return;
    }
    this.Value = this.Quantity * this.ProductId.WithCompany(this.CompanyId).ValueSvl / quantity;
  }

  public bool ShouldExcludeForValuation() {
    return this.OwnerId != null && this.OwnerId != this.CompanyId.PartnerId;
  }

  public void ApplyInventory() {
    var accountingDateGroups = Env.Utils.GroupBy(this, q => q.AccountingDate);
    foreach (var accountingDateGroup in accountingDateGroups) {
      var inventories = Env.StockQuant.Concat(accountingDateGroup);
      if (accountingDateGroup.Key != null) {
        inventories.WithContext("force_period_date", accountingDateGroup.Key).ApplyInventory();
        inventories.AccountingDate = null;
      } else {
        inventories.ApplyInventory();
      }
    }
  }

  public Dictionary<string, object> GetInventoryMoveValues(decimal qty, Core.Location locationId, Core.Location locationDestId, Core.Package packageId = null, Core.Package packageDestId = null) {
    var resMove = base.GetInventoryMoveValues(qty, locationId, locationDestId, packageId, packageDestId);
    if (Env.Context.Get("inventory_name") == null) {
      if (Env.Context.Get("force_period_date") != null) {
        resMove["name"] += $" [Accounted on {Env.Context.Get("force_period_date")}]";
      }
    }
    return resMove;
  }

  public List<string> GetInventoryFieldsWrite() {
    var res = base.GetInventoryFieldsWrite();
    res.Add("AccountingDate");
    return res;
  }
}
