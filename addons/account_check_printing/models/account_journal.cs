csharp
public partial class AccountJournal
{
    public string ComputeCheckNextNumber()
    {
        if (CheckSequence != null)
        {
            return CheckSequence.GetNextChar(CheckSequence.NumberNextActual);
        }
        return "1";
    }

    public void InverseCheckNextNumber()
    {
        if (!string.IsNullOrEmpty(CheckNextNumber) && !System.Text.RegularExpressions.Regex.IsMatch(CheckNextNumber, @"^[0-9]+$"))
        {
            throw new ValidationException("Next Check Number should only contains numbers.");
        }

        int nextNumber = int.Parse(CheckNextNumber);
        if (nextNumber < CheckSequence.NumberNextActual)
        {
            throw new ValidationException($"The last check number was {CheckSequence.NumberNextActual}. In order to avoid a check being rejected by the bank, you can only use a greater number.");
        }

        if (CheckSequence != null)
        {
            CheckSequence.NumberNextActual = nextNumber;
            CheckSequence.Padding = CheckNextNumber.Length;
        }
    }

    public void CreateCheckSequence()
    {
        if (CheckSequence == null)
        {
            CheckSequence = Env.Sequences.Create(new Core.Sequence
            {
                Name = $"{Name}: Check Number Sequence",
                Implementation = "no_gap",
                Padding = 5,
                NumberIncrement = 1,
                Company = Company
            });
        }
    }

    public Dictionary<string, object> GetJournalDashboardData()
    {
        var dashboardData = base.GetJournalDashboardData();
        dashboardData["NumChecksToPrint"] = Env.Payments.Count(p =>
            p.PaymentMethodLine.Code == "check_printing" &&
            p.State == "posted" &&
            !p.IsMoveSent
        );
        return dashboardData;
    }

    public ActionResult ActionChecksToPrint()
    {
        var paymentMethodLine = OutboundPaymentMethodLines.FirstOrDefault(l => l.Code == "check_printing");
        return new ActionResult
        {
            Name = "Checks to Print",
            Type = ActionType.ActWindow,
            ViewMode = "list,form,graph",
            ResModel = "Account.Payment",
            Context = new Dictionary<string, object>
            {
                ["SearchDefaultChecksToSend"] = 1,
                ["JournalId"] = Id,
                ["DefaultJournalId"] = Id,
                ["DefaultPaymentType"] = "outbound",
                ["DefaultPaymentMethodLineId"] = paymentMethodLine?.Id
            }
        };
    }
}
