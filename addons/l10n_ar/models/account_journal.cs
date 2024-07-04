csharp
public partial class AccountJournal
{
    public bool ComputeL10nArIsPos()
    {
        return CountryCode == "AR" && Type == "sale" && L10nLatamUseDocuments;
    }

    public Account.AfipPosSystem ComputeL10nArAfipPosSystem()
    {
        return L10nArIsPos ? L10nArAfipPosSystem : null;
    }

    public List<string> GetJournalLetter(Core.Partner counterpartPartner = null)
    {
        var lettersData = new Dictionary<string, Dictionary<string, List<string>>>
        {
            ["issued"] = new Dictionary<string, List<string>>
            {
                ["1"] = new List<string> { "A", "B", "E", "M" },
                ["3"] = new List<string> { },
                ["4"] = new List<string> { "C" },
                ["5"] = new List<string> { },
                ["6"] = new List<string> { "C", "E" },
                ["9"] = new List<string> { "I" },
                ["10"] = new List<string> { },
                ["13"] = new List<string> { "C", "E" },
                ["99"] = new List<string> { }
            },
            ["received"] = new Dictionary<string, List<string>>
            {
                ["1"] = new List<string> { "A", "B", "C", "E", "M", "I" },
                ["3"] = new List<string> { "B", "C", "I" },
                ["4"] = new List<string> { "B", "C", "I" },
                ["5"] = new List<string> { "B", "C", "I" },
                ["6"] = new List<string> { "A", "B", "C", "I" },
                ["9"] = new List<string> { "E" },
                ["10"] = new List<string> { "E" },
                ["13"] = new List<string> { "A", "B", "C", "I" },
                ["99"] = new List<string> { "B", "C", "I" }
            }
        };

        if (Company.L10nArAfipResponsibilityType == null)
        {
            throw new RedirectWarningException("Can not create chart of account until you configure your company AFIP Responsibility and VAT.", "base.action_res_company_form", "Go to Companies");
        }

        var letters = lettersData[L10nArIsPos ? "issued" : "received"][Company.L10nArAfipResponsibilityType.Code];

        if (counterpartPartner != null)
        {
            var counterpartLetters = lettersData[!L10nArIsPos ? "issued" : "received"].GetValueOrDefault(counterpartPartner.L10nArAfipResponsibilityType.Code, new List<string>());
            letters = letters.Intersect(counterpartLetters).ToList();
        }

        return letters;
    }

    public List<string> GetJournalCodesDomain()
    {
        return GetCodesPerJournalType(L10nArAfipPosSystem);
    }

    public List<string> GetCodesPerJournalType(Account.AfipPosSystem afipPosSystem)
    {
        var usualCodes = new List<string> { "1", "2", "3", "6", "7", "8", "11", "12", "13" };
        var mipymeCodes = new List<string> { "201", "202", "203", "206", "207", "208", "211", "212", "213" };
        var invoiceMCode = new List<string> { "51", "52", "53" };
        var receiptMCode = new List<string> { "54" };
        var receiptCodes = new List<string> { "4", "9", "15" };
        var expoCodes = new List<string> { "19", "20", "21" };
        var zetaCodes = new List<string> { "80", "83" };
        var codesIssuerIsSupplier = new List<string> { "23", "24", "25", "26", "27", "28", "33", "43", "45", "46", "48", "58", "60", "61", "150", "151", "157", "158", "161", "162", "164", "166", "167", "171", "172", "180", "182", "186", "188", "332" };

        List<string> codes = new List<string>();

        if ((Type == "sale" && !L10nArIsPos) || (Type == "purchase" && (afipPosSystem == Account.AfipPosSystem.II_IM || afipPosSystem == Account.AfipPosSystem.RLI_RLM)))
        {
            codes = codesIssuerIsSupplier;
        }
        else if (Type == "purchase" && afipPosSystem == Account.AfipPosSystem.RAW_MAW)
        {
            codes = new List<string> { "60", "61" };
        }
        else if (Type == "purchase")
        {
            return codesIssuerIsSupplier;
        }
        else if (afipPosSystem == Account.AfipPosSystem.II_IM)
        {
            codes = usualCodes.Concat(receiptCodes).Concat(expoCodes).Concat(invoiceMCode).Concat(receiptMCode).ToList();
        }
        else if (afipPosSystem == Account.AfipPosSystem.RAW_MAW)
        {
            codes = usualCodes.Concat(receiptCodes).Concat(invoiceMCode).Concat(receiptMCode).Concat(mipymeCodes).ToList();
        }
        else if (afipPosSystem == Account.AfipPosSystem.RLI_RLM)
        {
            codes = usualCodes.Concat(receiptCodes).Concat(invoiceMCode).Concat(receiptMCode).Concat(mipymeCodes).Concat(zetaCodes).ToList();
        }
        else if (afipPosSystem == Account.AfipPosSystem.CPERCEL || afipPosSystem == Account.AfipPosSystem.CPEWS)
        {
            codes = usualCodes.Concat(invoiceMCode).ToList();
        }
        else if (afipPosSystem == Account.AfipPosSystem.BFERCEL || afipPosSystem == Account.AfipPosSystem.BFEWS)
        {
            codes = usualCodes.Concat(mipymeCodes).ToList();
        }
        else if (afipPosSystem == Account.AfipPosSystem.FEERCEL || afipPosSystem == Account.AfipPosSystem.FEEWS || afipPosSystem == Account.AfipPosSystem.FEERCELP)
        {
            codes = expoCodes;
        }

        return codes;
    }

    public void CheckAfipPosSystem()
    {
        if (L10nArIsPos && Type == "purchase" && L10nArAfipPosSystem != Account.AfipPosSystem.II_IM && L10nArAfipPosSystem != Account.AfipPosSystem.RLI_RLM && L10nArAfipPosSystem != Account.AfipPosSystem.RAW_MAW)
        {
            throw new ValidationException($"The pos system {L10nArAfipPosSystem} can not be used on a purchase journal (id {Id})");
        }
    }

    public void CheckAfipPosNumber()
    {
        if (L10nArIsPos && L10nArAfipPosNumber == 0)
        {
            throw new ValidationException("Please define an AFIP POS number");
        }

        if (L10nArIsPos && L10nArAfipPosNumber > 99999)
        {
            throw new ValidationException("Please define a valid AFIP POS number (5 digits max)");
        }
    }

    public void OnchangeSetShortName()
    {
        if (Type == "sale" && L10nArAfipPosNumber.HasValue)
        {
            Code = L10nArAfipPosNumber.Value.ToString("D5");
        }
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        var protectedFields = new List<string> { "Type", "L10nArAfipPosSystem", "L10nArAfipPosNumber", "L10nLatamUseDocuments" };
        var fieldsToCheck = protectedFields.Where(field => vals.ContainsKey(field)).ToList();

        if (fieldsToCheck.Any())
        {
            var journalWithEntryIds = Env.Cr.Query<int>("SELECT DISTINCT(journal_id) FROM account_move WHERE posted_before = True").ToList();

            if (Company.AccountFiscalCountry.Code == "AR" && (Type == "sale" || Type == "purchase") && journalWithEntryIds.Contains(Id))
            {
                foreach (var field in fieldsToCheck)
                {
                    if (!object.Equals(vals[field], this.GetType().GetProperty(field).GetValue(this)))
                    {
                        throw new UserException($"You can not change {Name} journal's configuration if it already has validated invoices");
                    }
                }
            }
        }

        return base.Write(vals);
    }
}
