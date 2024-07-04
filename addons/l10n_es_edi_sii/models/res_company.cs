csharp
public partial class ResCompany
{
    public virtual void ComputeL10nEsEdiCertificate()
    {
        if (this.Country?.Code == "ES")
        {
            this.L10nEsEdiCertificateId = Env.Set<Company.L10nEsEdiCertificate>()
                .Search(c => c.Company == this)
                .OrderByDescending(c => c.DateEnd)
                .FirstOrDefault();
        }
        else
        {
            this.L10nEsEdiCertificateId = null;
        }
    }
}
