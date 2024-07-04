csharp
public partial class MailResCompany
{
    public int AliasDomainId { get; set; }
    public string AliasDomainName { get; set; }
    public string BounceEmail { get; set; }
    public string BounceFormatted { get; set; }
    public string CatchallEmail { get; set; }
    public string CatchallFormatted { get; set; }
    public string DefaultFromEmail { get; set; }
    public string EmailFormatted { get; set; }
    public string EmailPrimaryColor { get; set; }
    public string EmailSecondaryColor { get; set; }

    public void ComputeBounce()
    {
        if (this.AliasDomainId != 0)
        {
            this.BounceEmail = Env.Get<MailAliasDomain>().Find(this.AliasDomainId).BounceEmail;
            this.BounceFormatted = Env.Get<Tools>().FormatAddr(this.Name, this.BounceEmail);
        }
        else
        {
            this.BounceEmail = "";
            this.BounceFormatted = "";
        }
    }

    public void ComputeCatchall()
    {
        if (this.AliasDomainId != 0)
        {
            this.CatchallEmail = Env.Get<MailAliasDomain>().Find(this.AliasDomainId).CatchallEmail;
            this.CatchallFormatted = Env.Get<Tools>().FormatAddr(this.Name, this.CatchallEmail);
        }
        else
        {
            this.CatchallEmail = "";
            this.CatchallFormatted = "";
        }
    }

    public void ComputeEmailFormatted()
    {
        if (this.PartnerId.EmailFormatted != null)
        {
            this.EmailFormatted = this.PartnerId.EmailFormatted;
        }
        else if (this.CatchallFormatted != null)
        {
            this.EmailFormatted = this.CatchallFormatted;
        }
        else
        {
            this.EmailFormatted = "";
        }
    }

    public void ComputeEmailPrimaryColor()
    {
        this.EmailPrimaryColor = this.PrimaryColor != null ? this.PrimaryColor : "#000000";
    }

    public void ComputeEmailSecondaryColor()
    {
        this.EmailSecondaryColor = this.SecondaryColor != null ? this.SecondaryColor : "#875A7B";
    }

    public int DefaultAliasDomainId()
    {
        return Env.Get<MailAliasDomain>().Find([], 1).Id;
    }
}
