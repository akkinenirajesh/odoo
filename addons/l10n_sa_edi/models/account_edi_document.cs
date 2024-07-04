csharp
public partial class AccountEdiDocument {

    public virtual List<AccountEdiDocument> _PrepareJobs() {
        List<AccountEdiDocument> jobs = Env.CallMethod<List<AccountEdiDocument>>("AccountEdiDocument", "_PrepareJobs");
        if (jobs.Count > 1) {
            int moveFirstIndex = 0;
            for (int index = 0; index < jobs.Count; index++) {
                List<AccountEdiDocument> documents = jobs[index].MoveLineIds;
                if (documents.Any(d => d.EdiFormatId.Code == "sa_zatca" && d.State == "to_send" && d.MoveId.L10nSaChainIndex != null)) {
                    moveFirstIndex = index;
                    break;
                }
            }
            jobs = new List<AccountEdiDocument> {jobs[moveFirstIndex]}.Concat(jobs.Take(moveFirstIndex)).Concat(jobs.Skip(moveFirstIndex + 1)).ToList();
        }
        return jobs;
    }
}
