csharp
public partial class AccountPayment
{
    public override string ToString()
    {
        // Example string representation
        return $"Payment {Amount} {PaymentType} for {Partner}";
    }

    public void Validate()
    {
        if (Date < DateTime.Today)
        {
            Move?.ActionPost();
        }
    }

    private Core.Partner GetPartner(Random random)
    {
        var partners = Env.Set<Core.Partner>()
            .Where(p => p.Company == Company)
            .ToList();

        int partnerCount = partners.Count;
        int splitIndex = (int)Math.Ceiling(partnerCount * 0.4);

        if (PartnerType == PartnerType.Customer)
        {
            return partners.Take(splitIndex).RandomElement(random);
        }
        else
        {
            return partners.Skip(splitIndex).RandomElement(random);
        }
    }

    private Account.Journal GetJournal(Random random)
    {
        return Env.Set<Account.Journal>()
            .Where(j => j.Company == Company && (j.Type == JournalType.Cash || j.Type == JournalType.Bank))
            .RandomElement(random);
    }

    private Account.PaymentMethodLine GetPaymentMethodLine(Random random)
    {
        var blacklistedCodes = new[] { "sdd", "bacs_dd" };
        return Env.Set<Account.PaymentMethodLine>()
            .Where(pml => pml.Journal == Journal &&
                          pml.PaymentMethod.PaymentType == PaymentType &&
                          !blacklistedCodes.Contains(pml.Code))
            .RandomElement(random);
    }

    public static IEnumerable<AccountPayment> Populate(int size)
    {
        var random = new Random();
        var companies = Env.Set<Core.Company>().Where(c => c.ChartTemplate != null).ToList();

        for (int i = 0; i < size; i++)
        {
            var payment = new AccountPayment
            {
                Company = companies.RandomElement(random),
                PaymentType = random.Next(2) == 0 ? PaymentType.Inbound : PaymentType.Outbound,
                PartnerType = random.Next(2) == 0 ? PartnerType.Customer : PartnerType.Supplier,
                Amount = (decimal)random.NextDouble() * 1000,
                Date = DateTime.Now.AddYears(-4).AddDays(random.Next(1461))
            };

            payment.Journal = payment.GetJournal(random);
            payment.PaymentMethodLine = payment.GetPaymentMethodLine(random);
            payment.Partner = payment.GetPartner(random);

            yield return payment;
        }
    }
}
