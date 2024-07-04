csharp
public partial class AccountJournal
{
    public void ComputeL10nLatamCompanyUseDocuments()
    {
        this.L10nLatamCompanyUseDocuments = this.Company.LocalizationUseDocuments();
    }

    public void OnChangeCompany()
    {
        this.L10nLatamUseDocuments = (this.Type == "sale" || this.Type == "purchase") && 
                                     this.L10nLatamCompanyUseDocuments;
    }

    public void ComputeHasSequenceHoles()
    {
        if (this.L10nLatamUseDocuments)
        {
            this.HasSequenceHoles = false;
        }
        else
        {
            // Call base implementation
            base.ComputeHasSequenceHoles();
        }
    }

    public void CheckUseDocument()
    {
        var moveCount = Env.Set<Account.AccountMove>()
            .Where(m => m.Journal == this && m.PostedBefore)
            .Take(1)
            .Count();

        if (moveCount > 0)
        {
            throw new ValidationException("You can not modify the field \"Use Documents?\" if there are validated invoices in this journal!");
        }
    }

    public void OnChangeType()
    {
        base.OnChangeType();
        if (this.L10nLatamUseDocuments)
        {
            this.RefundSequence = false;
        }
    }
}
