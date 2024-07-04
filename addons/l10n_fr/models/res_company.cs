csharp
public partial class ResCompany
{
    private static readonly string[] FranceCountryCodes = { "FR", "MF", "MQ", "NC", "PF", "RE", "GF", "GP", "TF" };

    public bool IsAccountingUnalterable()
    {
        if (string.IsNullOrEmpty(Vat) && Country == null)
        {
            return false;
        }
        return Country != null && FranceCountryCodes.Contains(Country.Code);
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (IsAccountingUnalterable())
        {
            CreateSecureSequence(new[] { "L10nFrClosingSequenceId" });
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (IsAccountingUnalterable())
        {
            CreateSecureSequence(new[] { "L10nFrClosingSequenceId" });
        }
    }

    private void CreateSecureSequence(string[] sequenceFields)
    {
        var valsWrite = new Dictionary<string, object>();
        foreach (var seqField in sequenceFields)
        {
            if (this.GetType().GetProperty(seqField).GetValue(this) == null)
            {
                var seq = Env.IrSequence.Create(new Dictionary<string, object>
                {
                    { "Name", $"Securisation of {seqField} - {Name}" },
                    { "Code", $"FRSECURE{Id}-{seqField}" },
                    { "Implementation", "no_gap" },
                    { "Prefix", "" },
                    { "Suffix", "" },
                    { "Padding", 0 },
                    { "Company", this }
                });
                valsWrite[seqField] = seq;
            }
        }
        if (valsWrite.Count > 0)
        {
            Write(valsWrite);
        }
    }
}
