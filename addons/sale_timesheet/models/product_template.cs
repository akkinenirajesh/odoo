csharp
public partial class SaleTimesheetProductTemplate
{
    public void ComputeServiceUpsellThresholdRatio()
    {
        var productUomHour = Env.Ref<Uom.Uom>("uom.product_uom_hour");
        var uomUnit = Env.Ref<Uom.Uom>("uom.product_uom_unit");
        var companyUom = Env.Company.TimesheetEncodeUomId;

        if (this.UomId == null || this.UomId != uomUnit || productUomHour.Factor == this.UomId.Factor ||
            this.UomId.CategoryId != productUomHour.CategoryId && this.UomId.CategoryId != uomUnit.CategoryId)
        {
            this.ServiceUpsellThresholdRatio = null;
            return;
        }

        var timesheetEncodeUom = this.CompanyId.TimesheetEncodeUomId ?? companyUom;
        this.ServiceUpsellThresholdRatio = $"(1 {this.UomId.Name} = {timesheetEncodeUom.Factor / productUomHour.Factor:.2f} {timesheetEncodeUom.Name})";
    }

    public void ComputeVisibleExpensePolicy()
    {
        var visibility = Env.User.IsInGroup("project.group_project_user");
        if (!this.VisibleExpensePolicy)
        {
            this.VisibleExpensePolicy = visibility;
        }
    }

    public void OnChangeServiceFields()
    {
        if (this.Type == "service" && this.ServiceType == "timesheet" &&
            !this.ServicePolicy.Equals(this.Original.ServicePolicy))
        {
            this.UomId = Env.Ref<Uom.Uom>("uom.product_uom_hour");
        }
        else if (this.Original.UomId != null)
        {
            this.UomId = this.Original.UomId;
        }
        else
        {
            this.UomId = GetDefaultUomId();
        }
        this.UomPoId = this.UomId;
    }

    private Uom.Uom GetDefaultUomId()
    {
        // Implement logic to get default UomId
        return null;
    }

    public void OnChangeServicePolicy()
    {
        InverseServicePolicy();
        var vals = GetOnChangeServicePolicyUpdates(this.ServiceTracking, this.ServicePolicy, this.ProjectId, this.ProjectTemplateId);
        if (vals != null)
        {
            Update(vals);
        }
    }

    private void InverseServicePolicy()
    {
        // Implement logic to inverse ServicePolicy
    }

    private Dictionary<string, object> GetOnChangeServicePolicyUpdates(string serviceTracking, string servicePolicy, Sale.Project projectId, Sale.ProjectTemplate projectTemplateId)
    {
        var vals = new Dictionary<string, object>();
        if (serviceTracking != "no" && servicePolicy == "delivered_timesheet")
        {
            if (projectId != null && !projectId.AllowTimesheets)
            {
                vals["ProjectId"] = null;
            }
            else if (projectTemplateId != null && !projectTemplateId.AllowTimesheets)
            {
                vals["ProjectTemplateId"] = null;
            }
        }
        return vals;
    }

    public void UnlinkExceptMasterData()
    {
        var timeProduct = Env.Ref<SaleTimesheet.ProductProduct>("sale_timesheet.time_product");
        if (timeProduct.ProductTmplId == this)
        {
            throw new Exception($"The {timeProduct.Name} product is required by the Timesheets app and cannot be archived nor deleted.");
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        // timesheet product can't be archived
        if (vals.ContainsKey("Active") && !(bool)vals["Active"])
        {
            var timeProduct = Env.Ref<SaleTimesheet.ProductProduct>("sale_timesheet.time_product");
            if (timeProduct.ProductTmplId == this)
            {
                throw new Exception($"The {timeProduct.Name} product is required by the Timesheets app and cannot be archived nor deleted.");
            }
        }
        base.Write(vals);
    }
}
