csharp
public partial class BaseDecimalPrecision
{
  public virtual int PrecisionGet(string application)
  {
    this.FlushModel("Name", "Digits");
    var result = Env.Cr.Execute("select digits from decimal_precision where name = @name", new { name = application });
    if (result.HasRows)
    {
      return Convert.ToInt32(result.GetValue(0, 0));
    }
    return 2;
  }

  public virtual void Create(Dictionary<string, object> vals)
  {
    var result = super(BaseDecimalPrecision, this).Create(vals);
    Env.Registry.ClearCache();
  }

  public virtual void Write(Dictionary<string, object> data)
  {
    var result = super(BaseDecimalPrecision, this).Write(data);
    Env.Registry.ClearCache();
  }

  public virtual void Unlink()
  {
    var result = super(BaseDecimalPrecision, this).Unlink();
    Env.Registry.ClearCache();
  }

  public virtual void OnChangeDigits(Dictionary<string, object> data)
  {
    if (data.ContainsKey("Digits") && Convert.ToInt32(data["Digits"]) < this.Digits)
    {
      var warningMessage = string.Format(
        "The precision has been reduced for {0}.\nNote that existing data WON'T be updated by this change.\n\nAs decimal precisions impact the whole system, this may cause critical issues.\nE.g. reducing the precision could disturb your financial balance.\n\nTherefore, changing decimal precisions in a running database is not recommended.", this.Name);

      var warning = new Dictionary<string, object> {
        { "title", string.Format("Warning for {0}", this.Name) },
        { "message", warningMessage }
      };

      Env.Context.Set("warning", warning);
    }
  }
}
