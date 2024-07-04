csharp
public partial class MailingSubscription 
{
    public void ComputeOptOutDateTime()
    {
        if (!this.OptOut)
        {
            this.OptOutDateTime = null;
        }
        else
        {
            this.OptOutDateTime = Env.Now();
        }
    }
}
