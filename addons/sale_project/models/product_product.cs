csharp
public partial class SaleProject.ProductProduct 
{
    public void OnChangeServiceTracking()
    {
        if (this.ServiceTracking == "no")
        {
            this.ProjectID = null;
            this.ProjectTemplateID = null;
        }
        else if (this.ServiceTracking == "task_global_project")
        {
            this.ProjectTemplateID = null;
        }
        else if (this.ServiceTracking == "task_in_project" || this.ServiceTracking == "project_only")
        {
            this.ProjectID = null;
        }
    }

    public void InverseServicePolicy()
    {
        if (this.ServicePolicy != null)
        {
            var serviceToGeneral = Env.GetModel("SaleProject.ProductTemplate").GetServiceToGeneral(this.ServicePolicy);
            this.InvoicePolicy = serviceToGeneral.InvoicePolicy;
            this.ServiceType = serviceToGeneral.ServiceType;
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("Type") && vals["Type"] != "service")
        {
            vals["ServiceTracking"] = "no";
            vals["ProjectID"] = null;
        }

        Env.GetModel("SaleProject.ProductProduct").Write(this, vals);
    }
}
