C#
public partial class PosOrder
{
    public PosPaymentMethod OnlinePaymentMethodId { get; set; }
    public decimal NextOnlinePaymentAmount { get; set; }
    public decimal AmountUnpaid { get; set; }

    public void ComputeOnlinePaymentMethodId()
    {
        this.OnlinePaymentMethodId = Env.Get("Pos.Config").FirstOrDefault(c => c.Id == this.ConfigId).GetCashierOnlinePaymentMethod();
    }

    public void ComputeAmountUnpaid()
    {
        this.AmountUnpaid = Env.Get("Core.Currency").FirstOrDefault(c => c.Id == this.CurrencyId).Round(this.GetRoundedAmount(this.AmountTotal) - this.AmountPaid);
    }

    public decimal GetAmountUnpaid()
    {
        return this.AmountUnpaid;
    }

    public void CleanPaymentLines()
    {
        Env.Get("Pos.Payment").Where(p => p.PosOrderId == this.Id && string.IsNullOrEmpty(p.OnlineAccountPaymentId)).Delete();
    }

    public Dictionary<string, object> GetAndSetOnlinePaymentsData(decimal? nextOnlinePaymentAmount = null)
    {
        if (this.State == "paid" || this.State == "done" || this.State == "invoiced")
        {
            return new Dictionary<string, object>()
            {
                { "id", this.Id },
                { "paidOrder", this.Read(new string[] { }, false) }
            };
        }

        var onlinePayments = Env.Get("Pos.Payment").SearchRead(new string[] { "&", ("PosOrderId", "=", this.Id), ("OnlineAccountPaymentId", "!=", "") }, new string[] { "PaymentMethodId", "Amount" }, false);
        var returnData = new Dictionary<string, object>()
        {
            { "id", this.Id },
            { "onlinePayments", onlinePayments },
            { "amountUnpaid", this.GetAmountUnpaid() }
        };

        if (nextOnlinePaymentAmount.HasValue)
        {
            if (nextOnlinePaymentAmount == 0 && onlinePayments.Count() == 0 && this.State == "draft" && !this.ConfigId.ModulePosRestaurant && this.ConfigId.TrustedConfigIds.Count() == 0)
            {
                this.CleanPaymentLines();
                returnData["deleted"] = true;
            }
            else if (this.CheckNextOnlinePaymentAmount(nextOnlinePaymentAmount.Value))
            {
                this.NextOnlinePaymentAmount = nextOnlinePaymentAmount.Value;
            }
        }

        return returnData;
    }

    public bool CheckNextOnlinePaymentAmount(decimal amount)
    {
        return amount >= 0 && amount <= this.GetAmountUnpaid();
    }

    public decimal? GetCheckedNextOnlinePaymentAmount()
    {
        var amount = this.NextOnlinePaymentAmount;
        return this.CheckNextOnlinePaymentAmount(amount) ? amount : null;
    }

    private decimal GetRoundedAmount(decimal amount)
    {
        // Implement rounding logic based on currency precision
        return amount;
    }
}
