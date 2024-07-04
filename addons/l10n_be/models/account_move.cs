csharp
public partial class AccountMove
{
    public string GetInvoiceReferenceBEPartner()
    {
        string bbacomm = System.Text.RegularExpressions.Regex.Replace(this.Partner.Ref ?? "", @"\D", "");
        if (string.IsNullOrEmpty(bbacomm))
        {
            bbacomm = this.Partner.Id.ToString();
        }
        bbacomm = bbacomm.Right(10).PadLeft(10, '0');
        int baseNum = int.Parse(bbacomm);
        int mod = baseNum % 97;
        mod = mod == 0 ? 97 : mod;
        return $"+++{bbacomm.Substring(0, 3)}/{bbacomm.Substring(3, 4)}/{bbacomm.Substring(7)}{mod:D2}+++";
    }

    public string GetInvoiceReferenceBEInvoice()
    {
        string bbacomm = this.Id.ToString().PadLeft(10, '0');
        int baseNum = int.Parse(bbacomm);
        int mod = baseNum % 97;
        mod = mod == 0 ? 97 : mod;
        return $"+++{bbacomm.Substring(0, 3)}/{bbacomm.Substring(3, 4)}/{bbacomm.Substring(7)}{mod:D2}+++";
    }

    public override string ToString()
    {
        // Implement the string representation of AccountMove
        return $"AccountMove {this.Id}";
    }
}
