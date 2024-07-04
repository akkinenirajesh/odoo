csharp
public partial class ProductExpiry.StockPicking
{
    public virtual List<ProductExpiry.StockMoveLine> MoveLineIds { get; set; }

    public virtual void PreActionDoneHook()
    {
        var res = base.PreActionDoneHook();

        if (res && !Env.Context.Get<bool>("skip_expired"))
        {
            var pickingsToWarnExpired = CheckExpiredLots();

            if (pickingsToWarnExpired != null)
            {
                pickingsToWarnExpired.ActionGenerateExpiredWizard();
            }
        }
    }

    private ProductExpiry.StockPicking CheckExpiredLots()
    {
        var expiredPickings = MoveLineIds.Where(ml => ml.LotId.ProductExpiryAlert).Select(ml => ml.PickingId).ToList();
        return expiredPickings.FirstOrDefault();
    }

    private void ActionGenerateExpiredWizard()
    {
        var expiredLotIds = MoveLineIds.Where(ml => ml.LotId.ProductExpiryAlert).Select(ml => ml.LotId.Id).ToList();
        var viewId = Env.Ref("product_expiry.confirm_expiry_view").Id;

        var context = new Dictionary<string, object>(Env.Context);

        context.Add("default_picking_ids", new List<object> { new List<object> { 6, 0, this.Id } });
        context.Add("default_lot_ids", new List<object> { new List<object> { 6, 0, expiredLotIds } });

        Env.Action(
            "Confirmation",
            "ir.actions.act_window",
            "expiry.picking.confirmation",
            "form",
            viewId,
            "new",
            context);
    }
}
