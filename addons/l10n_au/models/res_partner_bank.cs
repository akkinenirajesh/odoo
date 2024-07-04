csharp
public partial class ResPartnerBank
{
    public override string ToString()
    {
        return AccNumber;
    }

    public List<(string, string)> GetSupportedAccountTypes()
    {
        var rslt = base.GetSupportedAccountTypes();
        rslt.Add(("aba", "ABA"));
        return rslt;
    }

    public void ValidateAbaBsb()
    {
        if (!string.IsNullOrEmpty(AbaBsb))
        {
            var testBsb = System.Text.RegularExpressions.Regex.Replace(AbaBsb, @"( |-)", "");
            if (testBsb.Length != 6 || !long.TryParse(testBsb, out _))
            {
                throw new ValidationException("BSB is not valid (expected format is \"NNN-NNN\"). Please rectify.");
            }
        }
    }

    public void ComputeAccType()
    {
        base.ComputeAccType();
        if (AccType == "bank" && System.Text.RegularExpressions.Regex.IsMatch(AccNumber ?? "", @"^(?=.*[1-9])[ \-\d]{0,9}$"))
        {
            AccType = "aba";
        }
    }
}
