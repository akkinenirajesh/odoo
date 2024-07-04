csharp
public partial class AccountFiscalPosition
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Note { get; set; }
    public virtual int CountryId { get; set; }
    public virtual bool AutoApply { get; set; }
    public virtual int Company { get; set; }
    public virtual List<AccountFiscalPositionTaxMap> MapTaxIds { get; set; }
    public virtual List<AccountFiscalPositionTax> ChargeTaxes { get; set; }
    public virtual int AccountTaxId { get; set; }
    public virtual int CashRoundingId { get; set; }
    public virtual AccountFiscalPositionApplyOn ApplyOn { get; set; }

    public virtual List<int> LoadPosDataDomain(dynamic data)
    {
        return Env.Context.Get("pos.config").Get("data")[0].Get("fiscal_position_ids");
    }

    public virtual List<string> LoadPosDataFields(int configId)
    {
        return new List<string>() { "Id", "Name", "DisplayName", "TaxMap" };
    }
}
