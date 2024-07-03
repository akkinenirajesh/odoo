csharp
public partial class ResPartnerBank
{
    public override string ToString()
    {
        return AccNumber;
    }

    private void _ComputeDisplayAccountWarning()
    {
        if (AllowOutPayment || string.IsNullOrEmpty(SanitizedAccNumber) || AccType != "iban")
        {
            HasIbanWarning = false;
            HasMoneyTransferWarning = false;
            return;
        }

        string bankCountry = SanitizedAccNumber.Substring(0, 2);
        HasIbanWarning = Partner?.Country != null && bankCountry != Partner.Country.Code;

        string bankInstitutionCode = SanitizedAccNumber.Substring(4, 3);
        HasMoneyTransferWarning = GetMoneyTransferServices().ContainsKey(bankInstitutionCode);
    }

    private void _ComputeMoneyTransferServiceName()
    {
        if (!string.IsNullOrEmpty(SanitizedAccNumber))
        {
            string bankInstitutionCode = SanitizedAccNumber.Substring(4, 3);
            MoneyTransferService = GetMoneyTransferServices().TryGetValue(bankInstitutionCode, out string serviceName) ? serviceName : null;
        }
        else
        {
            MoneyTransferService = null;
        }
    }

    private Dictionary<string, string> GetMoneyTransferServices()
    {
        return new Dictionary<string, string>
        {
            { "967", "Wise" },
            { "977", "Paynovate" },
            { "974", "PPS EU SA" }
        };
    }

    private void _ComputeUserHasGroupValidateBankAccount()
    {
        UserHasGroupValidateBankAccount = Env.User.HasGroup("account.group_validate_bank_account");
    }

    private void _ComputeLockTrustFields()
    {
        if (!Id.HasValue || !AllowOutPayment)
        {
            LockTrustFields = false;
        }
        else if (Id.HasValue && AllowOutPayment)
        {
            LockTrustFields = true;
        }
    }

    // Other methods and properties can be added here as needed
}
