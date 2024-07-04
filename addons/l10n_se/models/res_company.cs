csharp
public partial class ResCompany 
{
    public void ComputeOrgNumber() 
    {
        if (Env.Context.Get("company_id").AccountFiscalCountryId.Code == "SE" && this.Vat != null) 
        {
            string orgNumber = System.Text.RegularExpressions.Regex.Replace(this.Vat, @"[^\d]", "");
            orgNumber = orgNumber[..6] + "-" + orgNumber[6..];
            this.OrgNumber = orgNumber;
        }
        else 
        {
            this.OrgNumber = "";
        }
    }
}
