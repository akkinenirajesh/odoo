csharp
public partial class AccountEdiDocument
{
    public List<JobInfo> PrepareJobs()
    {
        var jobs = base.PrepareJobs();

        if (jobs.Count > 1)
        {
            int moveFirstIndex = 0;
            for (int index = 0; index < jobs.Count; index++)
            {
                var documents = jobs[index].Documents;
                if (documents.Any(d => 
                    d.EdiFormatId.Code == "es_tbai" && 
                    d.State == "to_send" && 
                    d.MoveId.L10nEsTbaiChainIndex != null))
                {
                    moveFirstIndex = index;
                    break;
                }
                
            }
            
            jobs = new List<JobInfo> { jobs[moveFirstIndex] }
                .Concat(jobs.Take(moveFirstIndex))
                .Concat(jobs.Skip(moveFirstIndex + 1))
                .ToList();
        }

        return jobs;
    }
}

public class JobInfo
{
    public List<AccountEdiDocument> Documents { get; set; }
    // Other properties...
}
