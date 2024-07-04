csharp
public partial class ProjectTask {
    public ProjectTask() {
    }

    public virtual Project GetDefaultPartnerId(Project project, ProjectTask parent) {
        var res = Env.CallMethod(this, "super", "GetDefaultPartnerId", project, parent);

        if (res == null && project != null) {
            var relatedProject = project;
            if (Env.User.IsPortal() && !Env.User.IsInternal()) {
                relatedProject = relatedProject.Sudo();
            }

            if (relatedProject.PricingType == "employee_rate") {
                return relatedProject.SaleLineEmployeeIds.FirstOrDefault()?.SaleLineId.OrderPartnerId;
            }
        }
        return res;
    }

    public virtual void ComputeRemainingHoursSo() {
        var timesheets = this.TimesheetIds.Where(t => t.TaskId.SaleLineId == t.SoLine || t.TaskId.SaleLineId == t._Origin.SoLine && t.SoLine.RemainingHoursAvailable);

        var mappedRemainingHours = this.Select(task => new { task.Id, RemainingHours = task.SaleLineId?.RemainingHours ?? 0.0 }).ToDictionary(x => x.Id);

        var uomHour = Env.Ref<Uom>("uom.product_uom_hour");

        foreach (var timesheet in timesheets) {
            var delta = 0.0;
            if (timesheet._Origin.SoLine == timesheet.TaskId.SaleLineId) {
                delta += timesheet._Origin.UnitAmount;
            }
            if (timesheet.SoLine == timesheet.TaskId.SaleLineId) {
                delta -= timesheet.UnitAmount;
            }
            if (delta > 0) {
                mappedRemainingHours[timesheet.TaskId.Id] += timesheet.ProductUomId.ComputeQuantity(delta, uomHour);
            }
        }

        foreach (var task in this) {
            task.RemainingHoursSo = mappedRemainingHours[task.Id];
        }
    }

    public virtual List<object> SearchRemainingHoursSo(string operator, double value) {
        return new List<object> { new { SaleLineId = new { RemainingHours = operator, value } } };
    }

    public virtual void ComputeAnalyticAccountActive() {
        Env.CallMethod(this, "super", "ComputeAnalyticAccountActive");
        foreach (var task in this) {
            task.AnalyticAccountActive = task.AnalyticAccountActive || task.SoAnalyticAccountId.Active;
        }
    }

    public virtual void InversePartnerId() {
        Env.CallMethod(this, "super", "InversePartnerId");
        foreach (var task in this) {
            if (task.AllowBillable && task.SaleLineId == null) {
                task.SaleLineId = task.GetLastSolOfCustomer();
            }
        }
    }

    public virtual void ComputeSaleLine() {
        Env.CallMethod(this, "super", "ComputeSaleLine");
        foreach (var task in this) {
            if (task.AllowBillable && task.SaleLineId == null) {
                task.SaleLineId = task.GetLastSolOfCustomer();
            }
        }
    }

    public virtual void ComputeIsProjectMapEmpty() {
        foreach (var task in this) {
            task.IsProjectMapEmpty = !task.Sudo().ProjectId.SaleLineEmployeeIds.Any();
        }
    }

    public virtual void ComputeHasMultiSol() {
        foreach (var task in this) {
            task.HasMultiSol = task.TimesheetIds.Any() && task.TimesheetIds.Any(t => t.SoLine != task.SaleLineId);
        }
    }

    public virtual SaleOrderLine GetLastSolOfCustomer() {
        if (this.PartnerId.CommercialPartnerId == null || !this.AllowBillable) {
            return null;
        }
        var domain = new List<object>();
        domain.AddRange(Env.CallMethod("Sale.SaleOrderLine", "_domain_sale_line_service"));
        domain.AddRange(new List<object> {
            new { CompanyId = "=", this.CompanyId.Id },
            new { OrderPartnerId = "child_of", this.PartnerId.CommercialPartnerId.Id },
            new { RemainingHours = ">", 0.0 }
        });

        if (this.ProjectId.PricingType != "task_rate" && this.ProjectSaleOrderId != null && this.PartnerId.CommercialPartnerId == this.ProjectId.PartnerId.CommercialPartnerId) {
            domain.AddRange(new List<object> { new { OrderId = "=", this.ProjectSaleOrderId.Id } });
        }
        return Env.Search<SaleOrderLine>(domain, 1).FirstOrDefault();
    }

    public virtual List<Timesheet> GetTimesheet() {
        var timesheetIds = Env.CallMethod(this, "super", "GetTimesheet");
        return timesheetIds.Where(t => t.IsNotInvoiced()).ToList();
    }

    public virtual List<int> GetActionViewSoIds() {
        var soIds = this.SaleOrderId.Ids;
        soIds.AddRange(this.TimesheetIds.Select(t => t.SoLine.OrderId).Distinct());
        return soIds;
    }
}
