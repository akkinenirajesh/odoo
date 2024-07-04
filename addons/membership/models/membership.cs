C#
public partial class MembershipLine 
{
    public void ComputeState()
    {
        // access 'this' for object properties
        // access Env for all external classes
        // Access methods like Env.Ref("product.product") for reference records

        if (this.AccountInvoiceId.State == "draft")
        {
            this.State = "Waiting";
        }
        else if (this.AccountInvoiceId.State == "posted")
        {
            if (this.AccountInvoiceId.PaymentState == "paid")
            {
                // check for reversed entry
                // replace with your own logic to check for reversed entry
                if (false)
                {
                    this.State = "Canceled";
                }
                else
                {
                    this.State = "Paid";
                }
            }
            else if (this.AccountInvoiceId.PaymentState == "in_payment")
            {
                this.State = "Paid";
            }
            else if (this.AccountInvoiceId.PaymentState == "not_paid" || this.AccountInvoiceId.PaymentState == "partial")
            {
                this.State = "Invoiced";
            }
        }
        else if (this.AccountInvoiceId.State == "cancel")
        {
            this.State = "Canceled";
        }
    }
}
