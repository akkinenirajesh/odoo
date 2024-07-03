csharp
public partial class AccountAnalyticAccount
{
    public void ComputeInvoiceCount()
    {
        var saleTypes = Env.Get<AccountMove>().GetSaleTypes(includeReceipts: true);
        var data = Env.Get<AccountMoveLine>().ReadGroup(
            new[]
            {
                ("ParentState", "=", "posted"),
                ("Move.MoveType", "in", saleTypes),
                ("AnalyticDistribution", "=", this.Id)
            },
            new[] { "AnalyticDistribution" },
            new[] { "__count" }
        );
        
        this.InvoiceCount = data.TryGetValue(this.Id, out var count) ? count : 0;
    }

    public void ComputeVendorBillCount()
    {
        var purchaseTypes = Env.Get<AccountMove>().GetPurchaseTypes(includeReceipts: true);
        var data = Env.Get<AccountMoveLine>().ReadGroup(
            new[]
            {
                ("ParentState", "=", "posted"),
                ("Move.MoveType", "in", purchaseTypes),
                ("AnalyticDistribution", "=", this.Id)
            },
            new[] { "AnalyticDistribution" },
            new[] { "__count" }
        );
        
        this.VendorBillCount = data.TryGetValue(this.Id, out var count) ? count : 0;
    }

    public ActionResult ActionViewInvoice()
    {
        var accountMoveLines = Env.Get<AccountMoveLine>().SearchFetch(
            new[]
            {
                ("Move.MoveType", "in", Env.Get<AccountMove>().GetSaleTypes()),
                ("AnalyticDistribution", "=", this.Id)
            },
            new[] { "Move" }
        );

        return new ActionResult
        {
            Type = "ir.actions.act_window",
            ResModel = "Account.Move",
            Domain = new[] { ("Id", "in", accountMoveLines.Select(l => l.Move.Id).ToArray()) },
            Context = new Dictionary<string, object>
            {
                { "create", false },
                { "default_move_type", "out_invoice" }
            },
            Name = "Customer Invoices",
            ViewMode = "tree,form"
        };
    }

    public ActionResult ActionViewVendorBill()
    {
        var accountMoveLines = Env.Get<AccountMoveLine>().SearchFetch(
            new[]
            {
                ("Move.MoveType", "in", Env.Get<AccountMove>().GetPurchaseTypes()),
                ("AnalyticDistribution", "=", this.Id)
            },
            new[] { "Move" }
        );

        return new ActionResult
        {
            Type = "ir.actions.act_window",
            ResModel = "Account.Move",
            Domain = new[] { ("Id", "in", accountMoveLines.Select(l => l.Move.Id).ToArray()) },
            Context = new Dictionary<string, object>
            {
                { "create", false },
                { "default_move_type", "in_invoice" }
            },
            Name = "Vendor Bills",
            ViewMode = "tree,form"
        };
    }
}
