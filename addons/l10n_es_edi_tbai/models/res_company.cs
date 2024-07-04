csharp
public partial class ResCompany
{
    public string ComputeL10nEsTbaiLicenseHtml()
    {
        var licenseDict = GetL10nEsTbaiLicenseDict();
        if (licenseDict.Count > 0)
        {
            licenseDict["TrNif"] = Env.T("Licence NIF");
            licenseDict["TrNumber"] = Env.T("Licence number");
            licenseDict["TrName"] = Env.T("Software name");
            licenseDict["TrVersion"] = Env.T("Software version");

            return $@"
<strong>{licenseDict["LicenseName"]}</strong><br/>
<p>
<strong>{licenseDict["TrNif"]}: </strong>{licenseDict["LicenseNif"]}<br/>
<strong>{licenseDict["TrNumber"]}: </strong>{licenseDict["LicenseNumber"]}<br/>
<strong>{licenseDict["TrName"]}: </strong>{licenseDict["SoftwareName"]}<br/>
<strong>{licenseDict["TrVersion"]}: </strong>{licenseDict["SoftwareVersion"]}<br/>
</p>";
        }
        else
        {
            return $"<strong>{Env.T("TicketBAI is not configured")}</strong>";
        }
    }

    public Dictionary<string, string> GetL10nEsTbaiLicenseDict()
    {
        if (CountryCode == "ES" && L10nEsTbaiTaxAgency != null)
        {
            string licenseKey = L10nEsEdiTestEnv ? L10nEsTbaiTaxAgency.ToString() : "Production";
            return Env.L10nEsTbaiLicenseDict[licenseKey];
        }
        return new Dictionary<string, string>();
    }

    public int GetL10nEsTbaiNextChainIndex()
    {
        if (L10nEsTbaiChainSequence == null)
        {
            var sequence = Env.IrSequence.Create(new Dictionary<string, object>
            {
                ["Name"] = $"TicketBAI account move sequence for {Name} (id: {Id})",
                ["Code"] = $"l10n_es.edi.tbai.account.move.{Id}",
                ["Implementation"] = "no_gap",
                ["Company"] = this
            });
            L10nEsTbaiChainSequence = sequence;
        }
        return L10nEsTbaiChainSequence.NextById();
    }

    public AccountMove GetL10nEsTbaiLastPostedInvoice(AccountMove beingPosted = null)
    {
        var domain = new List<object[]>
        {
            new object[] { "L10nEsTbaiChainIndex", "!=", 0 },
            new object[] { "Company", "=", this }
        };

        if (beingPosted != null)
        {
            domain.Add(new object[] { "L10nEsTbaiChainIndex", "!=", beingPosted.L10nEsTbaiChainIndex });
        }

        return Env.AccountMove.Search(domain, limit: 1, order: "L10nEsTbaiChainIndex desc").FirstOrDefault();
    }
}
