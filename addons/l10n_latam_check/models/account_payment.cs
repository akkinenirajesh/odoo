csharp
public partial class AccountPayment
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeL10nLatamCheckNumber()
    {
        L10nLatamCheckNumber = CheckNumber;
    }

    public void InverseL10nLatamCheckNumber()
    {
        CheckNumber = L10nLatamCheckNumber;
    }

    public void ComputeL10nLatamCheckBankId()
    {
        if (PaymentMethodLineId?.Code == "new_third_party_checks")
        {
            L10nLatamCheckBankId = PartnerId?.BankIds.FirstOrDefault()?.BankId;
        }
        else
        {
            L10nLatamCheckBankId = null;
        }
    }

    public void ComputeL10nLatamCheckIssuerVat()
    {
        if (PaymentMethodLineId?.Code == "new_third_party_checks")
        {
            L10nLatamCheckIssuerVat = PartnerId?.Vat;
        }
        else
        {
            L10nLatamCheckIssuerVat = null;
        }
    }

    public void CleanL10nLatamCheckIssuerVat()
    {
        if (!string.IsNullOrEmpty(L10nLatamCheckIssuerVat) && !string.IsNullOrEmpty(CompanyId?.CountryId?.Code))
        {
            // Implement VAT cleaning logic here
        }
    }

    public void ComputeL10nLatamCheckWarningMsg()
    {
        // Implement warning message computation logic here
    }

    public void ComputeL10nLatamCheckCurrentJournal()
    {
        // Implement current journal computation logic here
    }

    public string[] GetBlockingL10nLatamWarningMsg()
    {
        var msgs = new List<string>();
        // Implement warning message logic here
        return msgs.ToArray();
    }

    public void OnchangeCheck()
    {
        if (L10nLatamCheckId != null)
        {
            Amount = L10nLatamCheckId.Amount;
        }
    }

    public void OnchangeToResetCheckIds()
    {
        L10nLatamCheckId = null;
    }

    public void OnchangeCheckNumber()
    {
        if (JournalId?.CompanyId?.CountryId?.Code == "AR" && !string.IsNullOrEmpty(L10nLatamCheckNumber) && int.TryParse(L10nLatamCheckNumber, out int checkNumber))
        {
            L10nLatamCheckNumber = checkNumber.ToString("D8");
        }
    }

    public string[] GetPaymentMethodCodesToExclude()
    {
        var res = base.GetPaymentMethodCodesToExclude();
        if (IsInternalTransfer)
        {
            res.Add("new_third_party_checks");
        }
        return res;
    }

    public void ActionPost()
    {
        var msgs = GetBlockingL10nLatamWarningMsg();
        if (msgs.Length > 0)
        {
            throw new ValidationException(string.Join("\n", msgs));
        }

        base.ActionPost();

        // Mark own checks that are not printed as sent
        if (PaymentMethodLineId?.Code == "check_printing" && L10nLatamManualChecks)
        {
            IsMoveSent = true;
        }
    }

    // Implement other methods as needed
}
