C#
public partial class PosMercuryConfiguration
{
  public string Name { get; set; }
  public string MerchantId { get; set; }
  public string MerchantPwd { get; set; }

  public void ComputePrefixedCardNumber()
  {
    foreach (var line in this)
    {
      if (!string.IsNullOrEmpty(line.MercuryCardNumber))
      {
        line.MercuryPrefixedCardNumber = "********" + line.MercuryCardNumber;
      }
      else
      {
        line.MercuryPrefixedCardNumber = "";
      }
    }
  }

  public void OnChangeUsePaymentTerminal()
  {
    if (this.UsePaymentTerminal != "mercury")
    {
      this.PosMercuryConfigId = null;
    }
  }
}
