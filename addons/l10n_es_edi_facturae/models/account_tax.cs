csharp
public partial class AccountTax 
{
    public override string ToString()
    {
        // Assuming there's a Name property in the AccountTax class
        return Name;
    }

    public string GetL10nEsEdiFacturaeTaxTypeDisplay()
    {
        return Env.GetOptionSetDisplay("Account.L10nEsEdiFacturaeTaxType", L10nEsEdiFacturaeTaxType);
    }
}
