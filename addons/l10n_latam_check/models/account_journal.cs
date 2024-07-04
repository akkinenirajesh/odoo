csharp
public partial class AccountJournal
{
    public void CheckL10nLatamManualChecks()
    {
        // Protect from setting check_manual_sequencing (Manual Numbering) + Use electronic/deferred checks
        var recs = Env.AccountJournals.Where(x => x.CheckManualSequencing && x.L10nLatamManualChecks).ToList();
        if (recs.Any())
        {
            var journalNames = string.Join(",", recs.Select(r => r.Name));
            throw new UserException(
                "Manual checks (electronic/deferred) can't be used together with check manual sequencing (check printing functionality), " +
                "please choose one or the other. Journals: " + journalNames);
        }
    }
}
