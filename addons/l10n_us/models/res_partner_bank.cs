csharp
public partial class ResPartnerBank
{
    public void CheckAbaRouting()
    {
        if (this.AbaRouting != null && !System.Text.RegularExpressions.Regex.IsMatch(this.AbaRouting, @"^\d{1,9}$"))
        {
            throw new System.Exception("ABA/Routing should only contains numbers (maximum 9 digits).");
        }
    }
}
