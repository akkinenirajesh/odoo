csharp
public partial class ResPartner
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base ResPartner model
        return Name;
    }

    public DeliveryCarrier GetDefaultDeliveryCarrier(Company company)
    {
        // This method simulates the company_dependent behavior
        // It should retrieve the default delivery carrier for the given company
        return Env.GetCompanyProperty<DeliveryCarrier>(this, "PropertyDeliveryCarrierId", company);
    }

    public void SetDefaultDeliveryCarrier(DeliveryCarrier carrier, Company company)
    {
        // This method simulates setting a company-dependent field
        Env.SetCompanyProperty(this, "PropertyDeliveryCarrierId", carrier, company);
    }
}
