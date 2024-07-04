csharp
public partial class Membership.Partner
{
    public virtual void ComputeMembershipState()
    {
        DateTime today = DateTime.Now;
        Membership.MembershipLine line = Env.Get<Membership.MembershipLine>().Search(x => x.Partner == this.AssociateMember.Id || x.Partner == this.Id && x.DateCancel == null).OrderBy(x => x.DateFrom).FirstOrDefault();
        this.MembershipStart = line?.DateFrom;
        line = Env.Get<Membership.MembershipLine>().Search(x => x.Partner == this.AssociateMember.Id || x.Partner == this.Id && x.DateCancel == null).OrderByDescending(x => x.DateTo).FirstOrDefault();
        this.MembershipStop = line?.DateTo;
        line = Env.Get<Membership.MembershipLine>().Search(x => x.Partner == this.Id).OrderBy(x => x.DateCancel).FirstOrDefault();
        this.MembershipCancel = line?.DateCancel;

        if (this.AssociateMember != null)
        {
            this.MembershipState = this.AssociateMember.MembershipState;
            return;
        }

        if (this.FreeMember && this.MembershipState != "paid")
        {
            this.MembershipState = "free";
            return;
        }

        foreach (Membership.MembershipLine mline in this.MemberLines)
        {
            if ((mline.DateTo ?? DateTime.MinValue) >= today && (mline.DateFrom ?? DateTime.MinValue) <= today)
            {
                this.MembershipState = mline.State;
                return;
            }
            else if ((mline.DateFrom ?? DateTime.MinValue) < today && (mline.DateTo ?? DateTime.MinValue) <= today && (mline.DateFrom ?? DateTime.MinValue) < (mline.DateTo ?? DateTime.MinValue))
            {
                if (mline.AccountInvoiceId != null && mline.AccountInvoiceId.PaymentState.IsIn("in_payment", "paid"))
                {
                    this.MembershipState = "old";
                }
                else if (mline.AccountInvoiceId != null && mline.AccountInvoiceId.State == "cancel")
                {
                    this.MembershipState = "canceled";
                }
                return;
            }
        }
        this.MembershipState = "none";
    }

    public virtual void CheckRecursionAssociateMember()
    {
        if (this.HasCycle("AssociateMember"))
        {
            throw new Exception("You cannot create recursive associated members.");
        }
    }

    public virtual void CronUpdateMembership()
    {
        var partners = Env.Get<Membership.Partner>().Search(x => x.MembershipState.IsIn("invoiced", "paid"));
        Env.AddToUpdate(Env.Get<Membership.Partner>().Fields["MembershipState"], partners);
    }

    public virtual Account.Move CreateMembershipInvoice(Product.Product product, double amount)
    {
        List<Account.Move> invoiceValsList = new List<Account.Move>();
        foreach (Membership.Partner partner in this)
        {
            var addr = partner.AddressGet(new List<string> { "invoice" });
            if (partner.FreeMember)
            {
                throw new Exception("Partner is a free Member.");
            }
            if (!addr.ContainsKey("invoice"))
            {
                throw new Exception("Partner doesn't have an address to make the invoice.");
            }
            invoiceValsList.Add(new Account.Move
            {
                MoveType = "out_invoice",
                PartnerId = partner.Id,
                InvoiceLineIds = new List<Account.MoveLine>
                {
                    new Account.MoveLine
                    {
                        ProductId = product.Id,
                        Quantity = 1,
                        PriceUnit = amount,
                        TaxIds = product.TaxesId.FilteredDomain(Env.Get<Account.Tax>().CheckCompanyDomain(Env.Company)).Ids
                    }
                }
            });
        }
        return Env.Get<Account.Move>().Create(invoiceValsList);
    }
}
