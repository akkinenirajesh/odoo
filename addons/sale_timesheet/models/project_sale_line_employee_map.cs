C#
public partial class SaleTimesheet.ProjectSaleLineEmployeeMap
{
    public void ComputeExistingEmployeeIds()
    {
        if (Env.Context.Contains("company"))
        {
            this.ExistingEmployeeIds = Env.Ref("Project.Project").Fetch(this.ProjectId.Id).SaleLineEmployeeIds.EmployeeId;
        }
        else
        {
            this.ExistingEmployeeIds = Env.Ref("Project.Project").Fetch(this.ProjectId.Id).SaleLineEmployeeIds.EmployeeId;
        }
    }

    public void ComputeSaleLineId()
    {
        if (this.SaleLineId != null && this.PartnerId != null && this.SaleLineId.OrderPartnerId.CommercialPartnerId != this.PartnerId.CommercialPartnerId)
        {
            this.SaleLineId = null;
        }
    }

    public void ComputePriceUnit()
    {
        if (this.SaleLineId != null)
        {
            this.PriceUnit = this.SaleLineId.PriceUnit;
        }
        else
        {
            this.PriceUnit = 0;
        }
    }

    public void ComputeCurrencyId()
    {
        if (this.SaleLineId != null)
        {
            this.CurrencyId = this.SaleLineId.CurrencyId;
        }
    }

    public void ComputeCost()
    {
        if (!this.IsCostChanged)
        {
            this.Cost = this.EmployeeId.HourlyCost;
        }
    }

    public void ComputeDisplayCost()
    {
        var isUomDay = Env.Ref("Uom.ProductUomDay").Id == Env.Company.TimesheetEncodeUomId;
        var resourceCalendarPerHours = GetWorkingHoursPerCalendar(isUomDay);

        if (isUomDay)
        {
            this.DisplayCost = this.Cost * resourceCalendarPerHours.GetValueOrDefault(this.EmployeeId.ResourceCalendarId.Id, 1);
        }
        else
        {
            this.DisplayCost = this.Cost;
        }
    }

    public void InverseDisplayCost()
    {
        var isUomDay = Env.Ref("Uom.ProductUomDay").Id == Env.Company.TimesheetEncodeUomId;
        var resourceCalendarPerHours = GetWorkingHoursPerCalendar(isUomDay);

        if (isUomDay)
        {
            this.Cost = this.DisplayCost / resourceCalendarPerHours.GetValueOrDefault(this.EmployeeId.ResourceCalendarId.Id, 1);
        }
        else
        {
            this.Cost = this.DisplayCost;
        }
    }

    public void ComputeIsCostChanged()
    {
        if (this.EmployeeId != null)
        {
            this.IsCostChanged = this.Cost != this.EmployeeId.HourlyCost;
        }
    }

    private Dictionary<int, double> GetWorkingHoursPerCalendar(bool isUomDay)
    {
        var resourceCalendarPerHours = new Dictionary<int, double>();

        if (!isUomDay)
        {
            return resourceCalendarPerHours;
        }

        var readGroupData = Env.Ref("Resource.Calendar").ReadGroup(new[] { ("Id", "in", this.EmployeeId.ResourceCalendarId.Ids) }, new[] { "HoursPerDay" }, new[] { "Id:array_agg" });
        foreach (var hoursPerDay in readGroupData)
        {
            foreach (var calendarId in (int[])hoursPerDay["Id:array_agg"])
            {
                resourceCalendarPerHours[calendarId] = (double)hoursPerDay["HoursPerDay"];
            }
        }

        return resourceCalendarPerHours;
    }

    public void Create(Dictionary<string, object> values)
    {
        var maps = Env.Ref("SaleTimesheet.ProjectSaleLineEmployeeMap").Create(values);
        maps.UpdateProjectTimesheet();
    }

    public void Write(Dictionary<string, object> values)
    {
        Env.Ref("SaleTimesheet.ProjectSaleLineEmployeeMap").Write(values);
        UpdateProjectTimesheet();
    }

    public void UpdateProjectTimesheet()
    {
        if (this.SaleLineId != null)
        {
            Env.Ref("Project.Project").Fetch(this.ProjectId.Id).UpdateTimesheetsSaleLineId();
        }
    }
}
