csharp
public partial class RecruitmentSource 
{
    public string Url { get; set; }

    public RecruitmentSource SourceId { get; set; }

    public Job JobId { get; set; }

    public List<Medium> MediumId { get; set; }

    public void ComputeUrl() 
    {
        this.Url = Env.UrlJoin(this.JobId.GetBaseUrl(), $"{this.JobId.WebsiteUrl}?{Env.UrlEncode(new Dictionary<string, string>
        {{
            "utm_campaign", Env.Ref("hr_recruitment.utm_campaign_job").Name,
            "utm_medium", this.MediumId.FirstOrDefault()?.Name ?? Env["Utm.Medium"]._FetchOrCreateUtmMedium("website").Name,
            "utm_source", this.SourceId.Name
        }
        ))}}");
    }
}
