csharp
public partial class AccountFiscalPosition
{
    public override bool Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("TaxIds"))
        {
            var posOrderCount = Env.GetModel("Pos.PosOrder").Sudo().SearchCount(new[] { ("FiscalPositionId", "in", new[] { this.Id }) });
            if (posOrderCount > 0)
            {
                throw new UserException(
                    "You cannot modify a fiscal position used in a POS order. " +
                    "You should archive it and create a new one."
                );
            }
        }
        return base.Write(vals);
    }
}
