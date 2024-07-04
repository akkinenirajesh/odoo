C#
public partial class MailTestSmsBl
{
    public void ComputeMobileNbr()
    {
        if (this.MobileNbr != null || this.CustomerId == null)
        {
            return;
        }

        this.MobileNbr = Env.Ref<ResPartner>(this.CustomerId).Mobile;
    }

    public void ComputePhoneNbr()
    {
        if (this.PhoneNbr != null || this.CustomerId == null)
        {
            return;
        }

        this.PhoneNbr = Env.Ref<ResPartner>(this.CustomerId).Phone;
    }

    public string[] PhoneGetNumberFields()
    {
        return new string[] { "PhoneNbr", "MobileNbr" };
    }

    public string[] MailGetPartnerFields(bool introspectFields)
    {
        return new string[] { "CustomerId" };
    }
}
