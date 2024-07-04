csharp
public partial class TestMailFull.MailTestPortal 
{
    public void ComputeAccessUrl()
    {
        if (this.Id != 0)
        {
            this.AccessUrl = $"/my/test_portal/{this.Id}";
        }
    }
}

public partial class TestMailFull.MailTestPortalNoPartner
{
    public void ComputeAccessUrl()
    {
        if (this.Id != 0)
        {
            this.AccessUrl = $"/my/test_portal_no_partner/{this.Id}";
        }
    }
}

public partial class TestMailFull.MailTestRating
{
    public void ComputeEmailFrom()
    {
        if (this.CustomerId.EmailNormalized != null)
        {
            this.EmailFrom = this.CustomerId.EmailNormalized;
        }
        else if (this.EmailFrom == null)
        {
            this.EmailFrom = null;
        }
    }

    public void ComputeMobileNbr()
    {
        if (this.CustomerId.Mobile != null)
        {
            this.MobileNbr = this.CustomerId.Mobile;
        }
        else if (this.MobileNbr == null)
        {
            this.MobileNbr = null;
        }
    }

    public void ComputePhoneNbr()
    {
        if (this.CustomerId.Phone != null)
        {
            this.PhoneNbr = this.CustomerId.Phone;
        }
        else if (this.PhoneNbr == null)
        {
            this.PhoneNbr = null;
        }
    }

    public List<string> MailGetPartnerFields(bool IntrospectFields)
    {
        return new List<string>() { "CustomerId" };
    }

    public List<string> PhoneGetNumberFields()
    {
        return new List<string>() { "PhoneNbr", "MobileNbr" };
    }

    public int RatingApplyGetDefaultSubtypeID()
    {
        return Env.Ref("test_mail_full.mt_mail_test_rating_rating_done");
    }

    public Res.Partner RatingGetPartner()
    {
        return this.CustomerId;
    }
}

public partial class TestMailFull.MailTestRatingThread 
{
    public List<string> MailGetPartnerFields(bool IntrospectFields)
    {
        return new List<string>() { "CustomerId" };
    }

    public Res.Partner RatingGetPartner()
    {
        return this.CustomerId ?? base.RatingGetPartner();
    }
}
