csharp
public partial class WebsiteHrRecruitment.Job
{
    public void ComputePublishedDate()
    {
        this.PublishedDate = this.WebsitePublished ? Env.GetDate().Today : null;
    }

    public void ComputeFullUrl()
    {
        this.FullUrl = Env.UrlJoin(this.GetBaseUrl(), (this.WebsiteUrl ?? "/jobs"));
    }

    public void OnChangeWebsitePublished()
    {
        if (this.WebsitePublished)
        {
            this.IsPublished = true;
        }
        else
        {
            this.IsPublished = false;
        }
    }

    public void SetOpen()
    {
        this.WebsitePublished = false;
        base.SetOpen();
    }

    public int GetBackendMenuId()
    {
        return Env.Ref("hr_recruitment.menu_hr_recruitment_root").Id;
    }

    public void ToggleActive()
    {
        if (this.Active)
        {
            this.WebsitePublished = false;
        }

        base.ToggleActive();
    }

    public void ComputeWebsiteUrl()
    {
        base.ComputeWebsiteUrl();

        this.WebsiteUrl = $"/jobs/{Env.Slug(this)}";
    }

    public Dictionary<string, object> SearchGetDetail(Website website, string order, Dictionary<string, object> options)
    {
        bool requiresSudo = false;
        bool withDescription = (bool)options["displayDescription"];
        int? countryId = (int?)options.Get("country_id");
        int? departmentId = (int?)options.Get("department_id");
        int? officeId = (int?)options.Get("office_id");
        int? contractTypeId = (int?)options.Get("contract_type_id");
        bool isRemote = (bool)options.Get("is_remote");
        bool isOtherDepartment = (bool)options.Get("is_other_department");
        bool isUntyped = (bool)options.Get("is_untyped");

        List<object> domain = new List<object>() { website.WebsiteDomain };

        if (countryId != null)
        {
            domain.Add(new object[] { "address_id.Country", "=", countryId });
            requiresSudo = true;
        }

        if (departmentId != null)
        {
            domain.Add(new object[] { "Department", "=", departmentId });
        }
        else if (isOtherDepartment)
        {
            domain.Add(new object[] { "Department", "=", null });
        }

        if (officeId != null)
        {
            domain.Add(new object[] { "Address", "=", officeId });
        }
        else if (isRemote)
        {
            domain.Add(new object[] { "Address", "=", null });
        }

        if (contractTypeId != null)
        {
            domain.Add(new object[] { "ContractType", "=", contractTypeId });
        }
        else if (isUntyped)
        {
            domain.Add(new object[] { "ContractType", "=", null });
        }

        if (requiresSudo && !Env.User.HasGroup("hr_recruitment.group_hr_recruitment_user"))
        {
            // Rule must be reinforced because of sudo.
            domain.Add(new object[] { "WebsitePublished", "=", true });
        }

        List<string> searchFields = new List<string>() { "Name" };
        List<string> fetchFields = new List<string>() { "Name", "WebsiteUrl" };
        Dictionary<string, object> mapping = new Dictionary<string, object>()
        {
            {
                "Name", new Dictionary<string, object>()
                {
                    { "name", "Name" },
                    { "type", "text" },
                    { "match", true }
                }
            },
            {
                "WebsiteUrl", new Dictionary<string, object>()
                {
                    { "name", "WebsiteUrl" },
                    { "type", "text" },
                    { "truncate", false }
                }
            }
        };

        if (withDescription)
        {
            searchFields.Add("Description");
            fetchFields.Add("Description");
            mapping.Add("Description", new Dictionary<string, object>()
            {
                { "name", "Description" },
                { "type", "text" },
                { "html", true },
                { "match", true }
            });
        }

        return new Dictionary<string, object>()
        {
            { "model", "WebsiteHrRecruitment.Job" },
            { "requiresSudo", requiresSudo },
            { "baseDomain", domain },
            { "searchFields", searchFields },
            { "fetchFields", fetchFields },
            { "mapping", mapping },
            { "icon", "fa-briefcase" }
        };
    }
}
