csharp
public partial class Website.HrApplicant
{
    public Website.HrApplicant WebsiteFormInputFilter(dynamic request, dynamic values)
    {
        if (values.ContainsKey("PartnerName"))
        {
            var applicantJob = Env.Get("Hr.Job").Search(new { Id = values["JobId"] }).FirstOrDefault();
            var name = applicantJob != null ? $"{values["PartnerName"]} - {applicantJob.Name}" : $"{values["PartnerName"]}'s Application";
            values["Name"] = name;
        }

        if (values.ContainsKey("JobId"))
        {
            var job = Env.Get("Hr.Job").Browse(values["JobId"]);
            if (!job.Active)
            {
                throw new Exception("The job offer has been closed.");
            }

            var stage = Env.Get("Hr.Recruitment.Stage").Search(new { Fold = false, JobIds = values["JobId"] }, "sequence asc").FirstOrDefault();

            if (stage != null)
            {
                values["StageId"] = stage.Id;
            }
        }

        return this;
    }
}
