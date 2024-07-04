csharp
public partial class ResCompany
{
    public Uom.UnitOfMeasure DefaultProjectTimeModeId()
    {
        var uom = Env.Ref("uom.product_uom_hour", false);
        var wtime = Env.Ref("uom.uom_categ_wtime");
        if (uom == null)
        {
            uom = Env.UnitOfMeasures.FirstOrDefault(u => u.CategoryId == wtime.Id && u.UomType == "reference");
        }
        if (uom == null)
        {
            uom = Env.UnitOfMeasures.FirstOrDefault(u => u.CategoryId == wtime.Id);
        }
        return uom;
    }

    public Uom.UnitOfMeasure DefaultTimesheetEncodeUomId()
    {
        var uom = Env.Ref("uom.product_uom_hour", false);
        var wtime = Env.Ref("uom.uom_categ_wtime");
        if (uom == null)
        {
            uom = Env.UnitOfMeasures.FirstOrDefault(u => u.CategoryId == wtime.Id && u.UomType == "reference");
        }
        if (uom == null)
        {
            uom = Env.UnitOfMeasures.FirstOrDefault(u => u.CategoryId == wtime.Id);
        }
        return uom;
    }

    public void CheckInternalProjectIdCompany()
    {
        if (InternalProjectId != null && InternalProjectId.CompanyId != this)
        {
            throw new ValidationException("The Internal Project of a company should be in that company.");
        }
    }

    public void CreateInternalProjectTask()
    {
        var typeIdsRef = Env.Ref("hr_timesheet.internal_project_default_stage", false);
        var typeIds = typeIdsRef != null ? new List<int> { typeIdsRef.Id } : new List<int>();

        var result = new Dictionary<string, object>
        {
            { "Name", "Internal" },
            { "AllowTimesheets", true },
            { "CompanyId", Id },
            { "TypeIds", typeIds },
            { "TaskIds", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "Name", "Training" }, { "CompanyId", Id } },
                    new Dictionary<string, object> { { "Name", "Meeting" }, { "CompanyId", Id } }
                }
            }
        };

        var projectId = Env.Projects.Create(result);
        InternalProjectId = projectId;
    }
}
