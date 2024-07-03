csharp
public partial class AccountJournal
{
    public void PeppolGetNewDocuments()
    {
        var ediUsers = Env.Set<AccountEdiProxyClientUser>()
            .Where(u => u.Company.AccountPeppolProxyState == PeppolProxyState.Receiver)
            .Where(u => this.Company.Contains(u.Company))
            .ToList();

        foreach (var user in ediUsers)
        {
            user.PeppolGetNewDocuments();
        }
    }

    public void PeppolGetMessageStatus()
    {
        var canSend = AccountEdiProxyClientUser.GetCanSendDomain();
        var ediUsers = Env.Set<AccountEdiProxyClientUser>()
            .Where(u => canSend.Contains(u.Company.AccountPeppolProxyState))
            .Where(u => this.Company.Contains(u.Company))
            .ToList();

        foreach (var user in ediUsers)
        {
            user.PeppolGetMessageStatus();
        }
    }

    public ActionResult ActionPeppolReadyMoves()
    {
        return new ActionResult
        {
            Name = "Peppol Ready invoices",
            Type = ActionType.Window,
            ViewMode = "list,form",
            ResModel = "Account.AccountMove",
            Context = new Dictionary<string, object>
            {
                { "search_default_peppol_ready", 1 }
            }
        };
    }
}
