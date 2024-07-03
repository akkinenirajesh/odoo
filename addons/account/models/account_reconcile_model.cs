csharp
public partial class AccountReconcileModel
{
    public override string ToString()
    {
        return Name;
    }

    public IEnumerable<AccountMove> ActionReconcileStat()
    {
        // Implementation of action_reconcile_stat method
        var moves = Env.Set<AccountMoveLine>()
            .Where(aml => aml.ReconcileModel == this)
            .Select(aml => aml.Move)
            .Distinct();

        return moves;
    }

    private void ComputeNumberEntries()
    {
        // Implementation of _compute_number_entries method
        NumberEntries = Env.Set<AccountMoveLine>().Count(aml => aml.ReconcileModel == this);
    }

    private void ComputeShowDecimalSeparator()
    {
        // Implementation of _compute_show_decimal_separator method
        ShowDecimalSeparator = Lines.Any(l => l.AmountType == "Regex");
    }

    private void ComputePaymentToleranceParam()
    {
        // Implementation of _compute_payment_tolerance_param method
        if (PaymentToleranceType == ReconcileModelPaymentToleranceType.Percentage)
        {
            PaymentToleranceParam = Math.Min(100.0m, Math.Max(0.0m, PaymentToleranceParam));
        }
        else
        {
            PaymentToleranceParam = Math.Max(0.0m, PaymentToleranceParam);
        }
    }

    private void CheckPaymentToleranceParam()
    {
        // Implementation of _check_payment_tolerance_param method
        if (AllowPaymentTolerance)
        {
            if (PaymentToleranceType == ReconcileModelPaymentToleranceType.Percentage && 
                (PaymentToleranceParam < 0 || PaymentToleranceParam > 100))
            {
                throw new ValidationException("A payment tolerance defined as a percentage should always be between 0 and 100");
            }
            else if (PaymentToleranceType == ReconcileModelPaymentToleranceType.FixedAmount && 
                     PaymentToleranceParam < 0)
            {
                throw new ValidationException("A payment tolerance defined as an amount should always be higher than 0");
            }
        }
    }
}
