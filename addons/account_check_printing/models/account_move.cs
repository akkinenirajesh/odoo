csharp
public partial class AccountMove
{
    public override void OnCreate()
    {
        base.OnCreate();
        ComputePreferredPaymentMethod();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (IsFieldChanged(nameof(Partner)))
        {
            ComputePreferredPaymentMethod();
        }
    }

    private void ComputePreferredPaymentMethod()
    {
        if (Partner != null)
        {
            // Assuming we have a method to get the partner with a specific company context
            var partnerWithCompany = Env.Partners.GetWithCompany(Partner, Company);
            PreferredPaymentMethod = partnerWithCompany.PropertyPaymentMethod;
        }
        else
        {
            PreferredPaymentMethod = null;
        }
    }
}
