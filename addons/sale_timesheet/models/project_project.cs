C#
public partial class ProjectProject {

    public virtual SaleTimesheet.ProjectPricingType PricingType { get; set; }

    public virtual Sale.OrderLine SaleLineId { get; set; }

    public virtual Res.Partner PartnerId { get; set; }

    public virtual SaleTimesheet.ProjectBillingType BillingType { get; set; }

    public virtual bool AllowTimesheets { get; set; }

    public virtual bool AllowBillable { get; set; }

    public virtual Account.AnalyticAccount AnalyticAccountId { get; set; }

    public virtual bool WarningEmployeeRate { get; set; }

    public virtual float AllocatedHours { get; set; }

    public virtual Product.Product TimesheetProductId { get; set; }

    public virtual int SaleOrderCount { get; set; }

    public virtual int SaleOrderLineCount { get; set; }

    public virtual IEnumerable<SaleTimesheet.ProjectSaleLineEmployeeMap> SaleLineEmployeeIds { get; set; }

    public virtual void ComputePricingType() {
        if (this.AllowBillable) {
            if (this.SaleLineEmployeeIds.Any()) {
                this.PricingType = SaleTimesheet.ProjectPricingType.EmployeeRate;
            }
            else if (this.SaleLineId != null) {
                this.PricingType = SaleTimesheet.ProjectPricingType.FixedRate;
            }
            else {
                this.PricingType = SaleTimesheet.ProjectPricingType.TaskRate;
            }
        }
    }

    public virtual void ComputeTimesheetProductId() {
        if (!this.AllowTimesheets || !this.AllowBillable) {
            this.TimesheetProductId = null;
        }
        else if (this.TimesheetProductId == null) {
            this.TimesheetProductId = Env.Ref<Product.Product>("sale_timesheet.time_product");
        }
    }

    public virtual void ComputeWarningEmployeeRate() {
        if (this.AllowBillable && this.AllowTimesheets && this.PricingType == SaleTimesheet.ProjectPricingType.EmployeeRate) {
            var employees = Env.Model<Account.AnalyticLine>().ReadGroup(
                new[] { new SearchCriteria("task_id", SearchOperator.In, this.GetTasks().Select(x => x.Id)), new SearchCriteria("employee_id", SearchOperator.NotNull) },
                new[] { "project_id" },
                new[] { new GroupByField("employee_id", GroupByAggregation.ArrayAgg) }
            );
            var dictProjectEmployee = employees.ToDictionary(x => (long)x.ProjectId, x => (long[])x.EmployeeIds);
            this.WarningEmployeeRate = dictProjectEmployee.ContainsKey(this.Id) && dictProjectEmployee[this.Id].Any(x => !this.SaleLineEmployeeIds.Any(y => y.EmployeeId == x));
        }
    }

    public virtual void ComputePartnerId() {
        if (this.AllowBillable && this.AllowTimesheets && this.PricingType != SaleTimesheet.ProjectPricingType.TaskRate) {
            this.PartnerId = this.SaleLineId?.OrderPartnerId ?? this.SaleLineEmployeeIds.FirstOrDefault()?.SaleLineId?.OrderPartnerId;
        }
    }

    public virtual void ComputeSaleLineId() {
        if (this.SaleLineId == null && this.PartnerId != null && this.PricingType == SaleTimesheet.ProjectPricingType.EmployeeRate) {
            var saleOrderLine = Env.Model<Sale.OrderLine>().Search(
                new[] {
                    Env.Model<Sale.OrderLine>().GetDomainSaleLineService(),
                    new[] { new SearchCriteria("order_partner_id", SearchOperator.ChildOf, this.PartnerId.CommercialPartnerId.Id), new SearchCriteria("remaining_hours", SearchOperator.GreaterThan, 0) },
                },
                1
            ).FirstOrDefault();
            this.SaleLineId = saleOrderLine ?? this.SaleLineEmployeeIds.FirstOrDefault()?.SaleLineId;
        }
    }

    public virtual void ComputeSaleOrderCount() {
        if (this.AllowBillable) {
            // TODO: implement ComputeSaleOrderCount
        }
        else {
            this.SaleOrderLineCount = 0;
            this.SaleOrderCount = 0;
        }
    }

    public virtual void ComputeBillingType() {
        if ((!this.AllowBillable || !this.AllowTimesheets) && this.BillingType == SaleTimesheet.ProjectBillingType.Manually) {
            this.BillingType = SaleTimesheet.ProjectBillingType.NotBillable;
        }
    }

    private IEnumerable<Project.Task> GetTasks() {
        // TODO: implement GetTasks
        return Enumerable.Empty<Project.Task>();
    }
}
