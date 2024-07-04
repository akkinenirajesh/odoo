csharp
public partial class OrderLine
{
    public void _ComputeL10nInHsnCode()
    {
        if (this.Company.AccountFiscalCountry.Code == "IN" && this.Product != null)
        {
            this.L10nInHsnCode = this.Product.L10nInHsnCode;
        }
        else
        {
            this.L10nInHsnCode = null;
        }
    }

    public List<string> _LoadPosDataFields(int configId)
    {
        var fields = base._LoadPosDataFields(configId);
        if (Env.Company.Country.Code == "IN")
        {
            fields.Add("L10nInHsnCode");
        }
        return fields;
    }
}
