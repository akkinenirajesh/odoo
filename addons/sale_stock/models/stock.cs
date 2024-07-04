csharp
public partial class SaleStock.StockMove
{
    public virtual void DefaultGet(ref Dictionary<string, object> defaults)
    {
        string model = Env.Context.Get("active_model");
        int soId = Env.Context.Get("active_id");
        if (model == "Sale.SaleOrder" && soId != 0)
        {
            defaults["GroupId"] = Env[model].Browse(soId)["ProcurementGroupId"];
        }
    }

    public virtual List<string> PrepareMergeMovesDistinctFields()
    {
        List<string> distinctFields = base.PrepareMergeMovesDistinctFields();
        distinctFields.Add("SaleLineId");
        return distinctFields;
    }

    public virtual List<object> GetRelatedInvoices()
    {
        List<object> rslt = base.GetRelatedInvoices();
        var invoices = this.Mapped("PickingId.SaleId.InvoiceIds").Filtered(x => x["State"] == "posted");
        rslt.AddRange(invoices);
        //rslt.AddRange(invoices.Mapped("ReverseEntryIds"));
        return rslt;
    }

    public virtual object GetSourceDocument()
    {
        object res = base.GetSourceDocument();
        return this.SaleLineId.FirstOrDefault().Order.FirstOrDefault() ?? res;
    }

    public virtual List<object> GetSaleOrderLines()
    {
        this.EnsureOne();
        return (this + this.Browse(this.RollupMoveOrigs() | this.RollupMoveDests())).SaleLineId;
    }

    public virtual void AssignPickingPostProcess(bool new = false)
    {
        base.AssignPickingPostProcess(new);
        if (new)
        {
            var pickingId = this.Mapped("PickingId");
            var saleOrderIds = this.Mapped("SaleLineId.Order");
            foreach (var saleOrderId in saleOrderIds)
            {
                pickingId.MessagePostWithSource("mail.message_origin_link",
                    renderValues: new Dictionary<string, object> { { "self", pickingId }, { "origin", saleOrderId } },
                    subtypeXmlid: "mail.mt_note");
            }
        }
    }

    public virtual List<object> GetAllRelatedSM(object product)
    {
        return base.GetAllRelatedSM(product) | this.Filtered(m => m.SaleLineId.FirstOrDefault().ProductId == product);
    }
}
