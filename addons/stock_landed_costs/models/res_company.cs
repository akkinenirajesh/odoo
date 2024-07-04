C#
public partial class ResCompany {
    public Account.Journal LcJournal { get; set; }

    public void UpdateLcJournal(Account.Journal journal) {
        this.LcJournal = journal;
    }
}
