C#
public partial class AccountMove {
    public int TimesheetCount { get; set; }
    public int TimesheetTotalDuration { get; set; }

    public void ComputeTimesheetTotalDuration() {
        if (!Env.User.IsInGroup("hr_timesheet.group_hr_timesheet_user")) {
            TimesheetTotalDuration = 0;
            return;
        }
        var groupData = Env.Ref<AccountAnalyticLine>().ReadGroup(
            new[] { new Condition("TimesheetInvoiceId", "in", this.Id) },
            new[] { "TimesheetInvoiceId" },
            new[] { "UnitAmount:sum" }
        );
        var timesheetUnitAmountDict = new Dictionary<int, double>();
        foreach (var data in groupData) {
            timesheetUnitAmountDict.Add(data.GetValue<int>("TimesheetInvoiceId"), data.GetValue<double>("UnitAmount:sum"));
        }
        var totalTime = this.Company.ProjectTimeModeId.ComputeQuantity(
            timesheetUnitAmountDict[this.Id],
            this.TimesheetEncodeUomId,
            "HALF-UP"
        );
        TimesheetTotalDuration = (int)Math.Round(totalTime);
    }

    public void ComputeTimesheetCount() {
        var timesheetData = Env.Ref<AccountAnalyticLine>().ReadGroup(
            new[] { new Condition("TimesheetInvoiceId", "in", this.Id) },
            new[] { "TimesheetInvoiceId" },
            new[] { "__count" }
        );
        var mappedData = new Dictionary<int, int>();
        foreach (var data in timesheetData) {
            mappedData.Add(data.GetValue<int>("TimesheetInvoiceId"), data.GetValue<int>("__count"));
        }
        TimesheetCount = mappedData.GetValueOrDefault(this.Id, 0);
    }

    public Action ViewTimesheet() {
        return new Action() {
            Type = "ir.actions.act_window",
            Name = "Timesheets",
            Domain = new[] { new Condition("ProjectId", "!=", null) },
            ResModel = "Account.AnalyticLine",
            ViewMode = "tree,form",
            Help = """
                <p class="o_view_nocontent_smiling_face">
                    Record timesheets
                </p><p>
                    You can register and track your workings hours by project every
                    day. Every time spent on a project will become a cost and can be re-invoiced to
                    customers if required.
                </p>
            """,
            Limit = 80,
            Context = new {
                DefaultProjectId = this.Id,
                SearchDefaultProjectId = new[] { this.Id }
            }
        };
    }

    public void LinkTimesheetsToInvoice(DateTime? startDate = null, DateTime? endDate = null) {
        foreach (var line in this.Where(i => i.MoveType == "out_invoice" && i.State == "draft").InvoiceLineIds) {
            var saleLineDelivery = line.SaleLineIds.Where(sol => sol.ProductId.InvoicePolicy == "delivery" && sol.ProductId.ServiceType == "timesheet");
            if (saleLineDelivery.Any()) {
                var domain = line.TimesheetDomainGetInvoicedLines(saleLineDelivery);
                if (startDate != null) {
                    domain = domain.And(new[] { new Condition("Date", ">=", startDate) });
                }
                if (endDate != null) {
                    domain = domain.And(new[] { new Condition("Date", "<=", endDate) });
                }
                var timesheets = Env.Ref<AccountAnalyticLine>().Search(domain);
                timesheets.Write(new Dictionary<string, object>() { { "TimesheetInvoiceId", this.Id } });
            }
        }
    }
}
