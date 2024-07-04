csharp
public partial class Job
{
    public override string ToString()
    {
        return Name;
    }

    private Res.Partner _DefaultAddressId()
    {
        var lastUsedAddress = Env.HrRecruitment.Job.Search(new[] { ("CompanyId", "in", Env.Companies.Ids) }, orderBy: "Id desc", limit: 1).FirstOrDefault();
        if (lastUsedAddress != null)
        {
            return lastUsedAddress.AddressId;
        }
        return Env.Company.PartnerId;
    }

    private IEnumerable<object> _AddressIdDomain()
    {
        return new[]
        {
            "|", "&", "&", ("Type", "!=", "contact"), ("Type", "!=", "private"),
            ("Id", "in", Env.Companies.PartnerId.ChildIds.Ids),
            ("Id", "in", Env.Companies.PartnerId.Ids)
        };
    }

    private IEnumerable<long> _GetDefaultFavoriteUserIds()
    {
        return new[] { Env.Uid };
    }

    private void _ComputeNoOfHiredEmployee()
    {
        var counts = Env.HrRecruitment.Applicant.ReadGroup(
            domain: new[]
            {
                ("JobId", "in", new[] { Id }),
                ("DateClosed", "!=", false),
                "|",
                    ("Active", "=", false),
                    ("Active", "=", true)
            },
            groupBy: new[] { "JobId" },
            aggregates: new[] { "__count" }
        );

        NoOfHiredEmployee = counts.FirstOrDefault()?.Count ?? 0;
    }

    // Add other compute methods and action methods here
}
