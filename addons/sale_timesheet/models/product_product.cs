csharp
public partial class SaleTimesheet.ProductProduct
{
    public bool IsDeliveredTimesheet()
    {
        if (this.Type == "service" && this.ServicePolicy == "delivered_timesheet")
        {
            return true;
        }
        return false;
    }
    
    public void OnChangeServiceFields()
    {
        if (this.Type == "service" && this.ServiceType == "timesheet")
        {
            if (this.ServicePolicy != Env.Ref("SaleTimesheet.ServicePolicy", "delivered_timesheet"))
            {
                this.UomId = Env.Ref("uom.product_uom_hour");
            }
        }
        else if (this.UomId != null)
        {
            this.UomId = this.UomId;
        }
        else
        {
            this.UomId = GetDefaultUomId();
        }
        this.UomPoId = this.UomId;
    }

    public void OnChangeServicePolicy()
    {
        InverseServicePolicy();
        var vals = this.ProductTemplateId.GetOnChangeServicePolicyUpdates(this.ServiceTracking, this.ServicePolicy, this.ProjectId, this.ProjectTemplateId);
        if (vals != null)
        {
            this.Update(vals);
        }
    }

    private Product.Uom GetDefaultUomId()
    {
        return Env.Ref("uom.product_uom_unit");
    }

    private void InverseServicePolicy()
    {
        // InverseServicePolicy implementation
    }

    public void UnlinkExceptMasterData()
    {
        var timeProduct = Env.Ref("sale_timesheet.time_product");
        if (this == timeProduct)
        {
            throw new ValidationException("The " + timeProduct.Name + " product is required by the Timesheets app and cannot be archived nor deleted.");
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        // timesheet product can't be archived
        if (!Env.IsTestMode() && vals.ContainsKey("Active") && !(bool)vals["Active"])
        {
            var timeProduct = Env.Ref("sale_timesheet.time_product");
            if (this == timeProduct)
            {
                throw new ValidationException("The " + timeProduct.Name + " product is required by the Timesheets app and cannot be archived nor deleted.");
            }
        }
        base.Write(vals);
    }
}
