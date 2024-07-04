C#
public partial class MrpProduction {
    public virtual bool PreButtonMarkDone()
    {
        bool confirmExpiredLots = this.CheckExpiredLots();
        if (confirmExpiredLots)
        {
            return confirmExpiredLots;
        }
        return Env.CallMethod("super", "PreButtonMarkDone", this);
    }

    public virtual bool CheckExpiredLots()
    {
        // We use the 'skip_expired' context key to avoid to make the check when
        // user already confirmed the wizard about using expired lots.
        if (Env.Context.Get("skip_expired"))
        {
            return false;
        }
        var expiredLotIds = this.MoveRawIds.Where(x => x.MoveLineIds.Any(ml => ml.LotId.ProductExpiryAlert)).Select(x => x.LotId.Id).ToList();
        if (expiredLotIds.Count > 0)
        {
            return (bool)Env.Call("expiry.picking.confirmation", "CreateConfirmationAction", new { LotIds = expiredLotIds, ProductionIds = this.Id });
        }
        return false;
    }
}
