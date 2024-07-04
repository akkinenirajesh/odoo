csharp
public partial class MassMailing.MailingFilter 
{
    public void CheckMailingDomain() 
    {
        if (this.MailingDomain != "[]") 
        {
            try
            {
                Env.GetModel(this.MailingModelId.Model).SearchCount(literal_eval(this.MailingDomain));
            }
            catch (Exception ex)
            {
                throw new Exception("The filter domain is not valid for this recipients.", ex);
            }
        }
    }
}
