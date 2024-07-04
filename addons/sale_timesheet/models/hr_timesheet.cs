csharp
public partial class SaleTimesheet.AccountAnalyticLine {

    public virtual void ComputeTimesheetInvoiceType() {
        if (this.ProjectId != null) {
            string invoiceType = null;
            if (this.SoLine == null) {
                invoiceType = this.ProjectId.BillingType != "manually" ? "non_billable" : "billable_manual";
            } else if (this.SoLine.ProductId.Type == "service") {
                if (this.SoLine.ProductId.InvoicePolicy == "delivery") {
                    if (this.SoLine.ProductId.ServiceType == "timesheet") {
                        invoiceType = this.Amount > 0 ? "timesheet_revenues" : "billable_time";
                    } else {
                        string serviceType = this.SoLine.ProductId.ServiceType;
                        invoiceType = serviceType == "milestones" || serviceType == "manual" ? $"billable_{serviceType}" : "billable_fixed";
                    }
                } else if (this.SoLine.ProductId.InvoicePolicy == "order") {
                    invoiceType = "billable_fixed";
                }
            }
            this.TimesheetInvoiceType = invoiceType;
        } else {
            if (this.Amount >= 0) {
                if (this.SoLine != null && this.SoLine.ProductId.Type == "service") {
                    this.TimesheetInvoiceType = "service_revenues";
                } else {
                    this.TimesheetInvoiceType = "other_revenues";
                }
            } else {
                this.TimesheetInvoiceType = "other_costs";
            }
        }
    }

    public virtual void ComputeCommercialPartner() {
        this.CommercialPartnerId = this.TaskId.PartnerId != null ? this.TaskId.PartnerId.CommercialPartnerId : this.ProjectId.PartnerId.CommercialPartnerId;
    }

    public virtual void ComputeSoLine() {
        if (!this.IsSoLineEdited && this.IsNotInBilled()) {
            this.SoLine = this.ProjectId.AllowBillable && this.DetermineSaleLine();
        }
    }

    public virtual Domain DomainSoLine() {
        return Env.Get<Sale.Order.Line>()._DomainSaleLineServiceStr($@"[
            ('qty_delivered_method', 'in', ['analytic', 'timesheet']),
            ('order_partner_id.commercial_partner_id', '=', commercial_partner_id)
        ]", false);
    }

    public virtual bool IsNotInBilled() {
        return this.TimesheetInvoiceId == null || this.TimesheetInvoiceId.State == "cancel";
    }

    public virtual Account.Move ActionInvoiceFromTimesheet() {
        return Env.Get<Account.Move>().Browse(this.TimesheetInvoiceId.Id);
    }

    public virtual Sale.Order ActionSaleOrderFromTimesheet() {
        return Env.Get<Sale.Order>().Browse(this.OrderId.Id);
    }

    public virtual bool _IsUpdatableTimesheet() {
        return this.IsNotInBilled();
    }

    private Sale.Order.Line DetermineSaleLine() {
        if (this.TaskId != null) {
            if (this.TaskId.AllowBillable && this.TaskId.SaleLineId != null) {
                if (this.TaskId.PricingType == "task_rate" || this.TaskId.PricingType == "fixed_rate") {
                    return Env.Get<Sale.Order.Line>().Browse(this.TaskId.SaleLineId.Id);
                } else {
                    var mapEntry = Env.Get<Project.Sale.Line.Employee.Map>().Search(x => x.ProjectId == this.ProjectId.Id && x.EmployeeId == (this.EmployeeId ?? Env.User.EmployeeId.Id) && x.SaleLineId.OrderPartnerId.CommercialPartnerId == this.TaskId.PartnerId.CommercialPartnerId);
                    if (mapEntry != null) {
                        return Env.Get<Sale.Order.Line>().Browse(mapEntry.SaleLineId.Id);
                    }
                    return Env.Get<Sale.Order.Line>().Browse(this.TaskId.SaleLineId.Id);
                }
            }
        } else if (this.ProjectId.PricingType == "employee_rate") {
            var mapEntry = this.GetEmployeeMappingEntry();
            if (mapEntry != null) {
                return Env.Get<Sale.Order.Line>().Browse(mapEntry.SaleLineId.Id);
            }
            if (this.ProjectId.SaleLineId != null) {
                return Env.Get<Sale.Order.Line>().Browse(this.ProjectId.SaleLineId.Id);
            }
        }
        return null;
    }

    private Project.Sale.Line.Employee.Map GetEmployeeMappingEntry() {
        return Env.Get<Project.Sale.Line.Employee.Map>().Search(x => x.ProjectId == this.ProjectId.Id && x.EmployeeId == (this.EmployeeId ?? Env.User.EmployeeId.Id));
    }

    private decimal _HourlyCost() {
        if (this.ProjectId.PricingType == "employee_rate") {
            var mappingEntry = this.GetEmployeeMappingEntry();
            if (mappingEntry != null) {
                return mappingEntry.Cost;
            }
        }
        return this.UnitAmount;
    }
}
