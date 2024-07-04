csharp
public partial class Company
{
    public override string ToString()
    {
        // You might want to return a meaningful string representation of the company
        return Env.GetString("Name");
    }

    public string GetL10nEsEdiFacturaeResidenceType()
    {
        // Assuming 'PartnerId' is a property that represents the related partner
        var partner = Env.Get<ResPartner.Partner>(PartnerId);
        return partner?.L10nEsEdiFacturaeResidenceType;
    }

    public IEnumerable<L10nEsEdiFacturaeCertificate> GetL10nEsEdiFacturaeCertificateId()
    {
        // Fetch related certificates
        return Env.Query<ResCompany.L10nEsEdiFacturaeCertificate>()
            .Where(c => c.Company == this)
            .ToList();
    }
}
