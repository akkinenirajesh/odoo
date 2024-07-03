csharp
public partial class PaymentProvider
{
    public override string ToString()
    {
        return Name;
    }

    private void EnsurePaymentMethodLine(bool allowCreate = true)
    {
        if (!Id.HasValue)
            return;

        var payMethodLine = Env.Set<Account.PaymentMethodLine>()
            .Where(p => p.PaymentProviderId == Id && p.JournalId != null)
            .FirstOrDefault();

        if (JournalId == null)
        {
            if (payMethodLine != null)
            {
                payMethodLine.Delete();
                return;
            }
        }

        if (payMethodLine == null)
        {
            payMethodLine = Env.Set<Account.PaymentMethodLine>()
                .Where(p => p.CompanyId == CompanyId && p.Code == GetCode() && p.PaymentProviderId == null && p.JournalId != null)
                .FirstOrDefault();
        }

        if (payMethodLine != null)
        {
            payMethodLine.PaymentProviderId = Id;
            payMethodLine.JournalId = JournalId;
            payMethodLine.Name = Name;
        }
        else if (allowCreate)
        {
            var defaultPaymentMethod = GetProviderPaymentMethod(GetCode());
            if (defaultPaymentMethod != null)
            {
                Env.Set<Account.PaymentMethodLine>().Create(new Account.PaymentMethodLine
                {
                    Name = Name,
                    PaymentMethodId = defaultPaymentMethod.Id,
                    JournalId = JournalId,
                    PaymentProviderId = Id
                });
            }
        }
    }

    public void ComputeJournalId()
    {
        var payMethodLine = Env.Set<Account.PaymentMethodLine>()
            .Where(p => p.PaymentProviderId == Id && p.JournalId != null)
            .FirstOrDefault();

        if (payMethodLine != null)
        {
            JournalId = payMethodLine.JournalId;
        }
        else if (State == "enabled" || State == "test")
        {
            JournalId = Env.Set<Account.Journal>()
                .Where(j => j.CompanyId == CompanyId && j.Type == "bank")
                .FirstOrDefault();

            if (Id.HasValue)
            {
                EnsurePaymentMethodLine();
            }
        }
    }

    public void InverseJournalId()
    {
        EnsurePaymentMethodLine();
    }

    public static int GetDefaultPaymentMethodId(string code)
    {
        var providerPaymentMethod = GetProviderPaymentMethod(code);
        if (providerPaymentMethod != null)
        {
            return providerPaymentMethod.Id;
        }
        return Env.Ref<Account.PaymentMethod>("account.account_payment_method_manual_in").Id;
    }

    public static Account.PaymentMethod GetProviderPaymentMethod(string code)
    {
        return Env.Set<Account.PaymentMethod>().Where(m => m.Code == code).FirstOrDefault();
    }

    public static void SetupProvider(string code)
    {
        // Base implementation not provided
        SetupPaymentMethod(code);
    }

    public static void SetupPaymentMethod(string code)
    {
        if (code != "none" && code != "custom" && GetProviderPaymentMethod(code) == null)
        {
            var providersDescription = GetProvidersDescription();
            Env.Set<Account.PaymentMethod>().Create(new Account.PaymentMethod
            {
                Name = providersDescription[code],
                Code = code,
                PaymentType = "inbound"
            });
        }
    }

    public bool CheckExistingPayment(Account.PaymentMethod paymentMethod)
    {
        return Env.Set<Account.Payment>().Any(p => p.PaymentMethodId == paymentMethod.Id);
    }

    public static void RemoveProvider(string code)
    {
        var paymentMethod = GetProviderPaymentMethod(code);
        var provider = new PaymentProvider();
        if (provider.CheckExistingPayment(paymentMethod))
        {
            throw new UserException("You cannot uninstall this module as payments using this payment method already exist.");
        }
        // Base implementation for removing provider not provided
        paymentMethod.Delete();
    }

    // Helper method to get providers description (implementation not provided)
    private static Dictionary<string, string> GetProvidersDescription()
    {
        // Implementation needed
        return new Dictionary<string, string>();
    }

    // Helper method to get code (implementation not provided)
    private string GetCode()
    {
        // Implementation needed
        return "";
    }
}
