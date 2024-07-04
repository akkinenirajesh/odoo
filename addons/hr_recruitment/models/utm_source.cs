csharp
public partial class UtmSource
{
    [OnDelete(AtUninstall = false)]
    public void UnlinkExceptLinkedRecruitmentSources()
    {
        var linkedRecruitmentSources = Env.Get<HrRecruitmentSource>().Search(new[]
        {
            ("SourceId", "in", new[] { this.Id })
        });

        if (linkedRecruitmentSources.Any())
        {
            var jobNames = linkedRecruitmentSources.Select(s => s.JobId.Name).Distinct();
            var errorMessage = string.Format(
                "You cannot delete these UTM Sources as they are linked to the following recruitment sources in Recruitment:\n{0}",
                string.Join(", ", jobNames.Select(name => $"\"{name}\"")));
            
            throw new UserException(errorMessage);
        }
    }
}
