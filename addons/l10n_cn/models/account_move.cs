csharp
public partial class AccountMove
{
    public override string ToString()
    {
        // Custom string representation logic can be added here
        return base.ToString();
    }

    public void CheckFapiao()
    {
        if (!string.IsNullOrEmpty(this.Fapiao) && (this.Fapiao.Length != 8 || !this.Fapiao.All(char.IsDigit)))
        {
            throw new ValidationException("Fapiao number is an 8-digit number. Please enter a correct one.");
        }
    }

    public bool CheckCn2an()
    {
        // Implementation depends on how cn2an is integrated in C#
        // This is a placeholder method
        return true;
    }

    public string ConvertToAmountInWord(decimal number)
    {
        if (!CheckCn2an())
        {
            return null;
        }
        // Implementation depends on how cn2an is integrated in C#
        // This is a placeholder method
        return "Amount in words";
    }

    public int CountAttachments()
    {
        var domains = new List<List<(string, string, object)>>
        {
            new List<(string, string, object)> { ("ResModel", "=", "Account.AccountMove"), ("ResId", "=", this.Id) }
        };

        var statementIds = this.LineIds.Select(l => l.StatementId).Where(id => id != null).ToList();
        var paymentIds = this.LineIds.Select(l => l.PaymentId).Where(id => id != null).ToList();

        if (statementIds.Any())
        {
            domains.Add(new List<(string, string, object)> { ("ResModel", "=", "Account.BankStatement"), ("ResId", "in", statementIds) });
        }
        if (paymentIds.Any())
        {
            domains.Add(new List<(string, string, object)> { ("ResModel", "=", "Account.Payment"), ("ResId", "in", paymentIds) });
        }

        // Assuming Env is available to access other models
        return Env.IrAttachment.SearchCount(domains.SelectMany(d => d).ToList());
    }
}
