csharp
public partial class AccountPayment
{
    public string ComputeCheckAmountInWords()
    {
        if (Currency != null)
        {
            return Currency.AmountToText(Amount);
        }
        return null;
    }

    public void ComputeCheckNumber()
    {
        if (Journal.CheckManualSequencing && PaymentMethodLine?.Code == "check_printing")
        {
            var sequence = Journal.CheckSequence;
            CheckNumber = sequence.GetNextChar(sequence.NumberNextActual);
        }
        else
        {
            CheckNumber = null;
        }
    }

    public void InverseCheckNumber()
    {
        if (!string.IsNullOrEmpty(CheckNumber))
        {
            var sequence = Journal.CheckSequence;
            sequence.Padding = CheckNumber.Length;
        }
    }

    public bool ComputeShowCheckNumber()
    {
        return PaymentMethodLine?.Code == "check_printing" && !string.IsNullOrEmpty(CheckNumber);
    }

    public void ActionPost()
    {
        var paymentMethodCheck = Env.Ref<Account.PaymentMethod>("account_check_printing.account_payment_method_check");
        if (PaymentMethod == paymentMethodCheck && CheckManualSequencing)
        {
            var sequence = Journal.CheckSequence;
            CheckNumber = sequence.NextById();
        }
        // Call base ActionPost method
        base.ActionPost();
    }

    public object PrintChecks()
    {
        // Implementation of print_checks method
        // This is a simplified version, you may need to adjust it based on your specific requirements
        if (PaymentMethodLine?.Code != "check_printing" || State == "reconciled")
        {
            throw new UserException("Payments to print as checks must have 'Check' selected as payment method and not have already been reconciled");
        }

        if (!Journal.CheckManualSequencing)
        {
            // Logic for pre-numbered checks
            // You might need to implement a wizard for this in your C# application
        }
        else
        {
            if (State == "draft")
            {
                ActionPost();
            }
            return DoPrintChecks();
        }

        return null;
    }

    public object DoPrintChecks()
    {
        // Implementation of do_print_checks method
        // This would typically return a report action in Odoo
        // You'll need to adapt this to your C# reporting system
        IsMoveSent = true;
        // Return appropriate report action
        return null;
    }
}
