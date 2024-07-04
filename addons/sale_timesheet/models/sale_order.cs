csharp
public partial class SaleOrder {

    public void ComputeTimesheetCount() {
        var timesheetsPerSo = Env.Ref("Account.AnalyticLine").ReadGroup(
            new object[] { new object[] { "OrderId", "in", this.Id } },
            new string[] { "OrderId" },
            new string[] { "__count" }
        );

        this.TimesheetCount = timesheetsPerSo.FirstOrDefault(x => x["OrderId"] == this.Id)["__count"];
    }

    public void ComputeTimesheetTotalDuration() {
        var groupData = Env.Ref("Account.AnalyticLine").ReadGroup(
            new object[] { new object[] { "OrderId", "in", this.Id } },
            new string[] { "OrderId" },
            new string[] { "UnitAmount:sum" }
        );
        var timesheetUnitAmountDict = new Dictionary<object, double>();
        timesheetUnitAmountDict = groupData.ToDictionary(x => x["OrderId"], x => (double)x["UnitAmount:sum"]);

        this.TimesheetTotalDuration = (int)Math.Round(Env.Ref("Project.ProjectTimeMode").ComputeQuantity(
            timesheetUnitAmountDict[this.Id],
            this.TimesheetEncodeUomId,
            "HALF-UP"
        ));
    }

    public void ComputeShowHoursRecordedButton() {
        var showButtonIds = GetOrderWithValidServiceProduct();
        this.ShowHoursRecordedButton = this.TimesheetCount > 0 || this.ProjectCount > 0 && showButtonIds.Contains(this.Id);
    }

    private object[] GetOrderWithValidServiceProduct() {
        return Env.Ref("Sale.SaleOrderLine").ReadGroup(
            new object[] { 
                Env.Ref("Sale.SaleOrderLine")._domain_sale_line_service(),
                new object[] { "OrderId", "in", this.Id },
                new object[] { "|", new object[] { "ProductId.ServiceType", "not in", new object[] { "Milestones", "Manual" } }, new object[] { "ProductId.InvoicePolicy", "!=", "Delivery" } } 
            },
            new string[] { "OrderId:array_agg" }
        )[0][0];
    }
}
