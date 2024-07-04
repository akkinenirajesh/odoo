csharp
public partial class AccountJournal {
    public void CheckType() {
        var methods = Env.GetModel("Account.PaymentMethod").Search(new[] { new CSharp.SearchCondition("JournalId", "in", this.Ids) });
        if (methods.Count > 0) {
            throw new ValidationError(_("This journal is associated with a payment method. You cannot modify its type"));
        }
    }

    public List<Account.Account> GetJournalInboundOutstandingPaymentAccounts() {
        var res = base.GetJournalInboundOutstandingPaymentAccounts();
        var accountIds = new HashSet<long>(res.Ids);
        foreach (var paymentMethod in this.PosPaymentMethodIds.Where(x => x.JournalId == this.Id)) {
            accountIds.Add(paymentMethod.OutstandingAccountId.Id ?? this.CompanyId.AccountJournalPaymentDebitAccountId.Id);
        }
        return Env.GetModel("Account.Account").Browse(accountIds);
    }

    public AccountJournal EnsureCompanyAccountJournal() {
        var journal = this.Search(new[] { new CSharp.SearchCondition("Code", "=", "POSS"), new CSharp.SearchCondition("CompanyId", "=", this.Env.Company.Id) }, 1);
        if (journal.Count == 0) {
            journal = this.Create(new Dictionary<string, object> {
                {"Name", _("Point of Sale")},
                {"Code", "POSS"},
                {"Type", "general"},
                {"CompanyId", this.Env.Company.Id},
            });
        }
        return journal.FirstOrDefault();
    }
}
