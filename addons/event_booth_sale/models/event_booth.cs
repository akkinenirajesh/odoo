csharp
public partial class EventBooth
{
    public void UnlinkExceptLinkedSaleOrder()
    {
        var boothWithSo = this.Env.Sudo().Search<EventBooth>(b => b.SaleOrderId != null);
        if (boothWithSo.Any())
        {
            var boothNames = string.Join(", ", boothWithSo.Select(b => b.Name));
            throw new UserError($"You can't delete the following booths as they are linked to sales orders: {boothNames}");
        }
    }

    public void ActionSetPaid()
    {
        this.IsPaid = true;
        this.Env.SaveChanges();
    }

    public ActionResult ActionViewSaleOrder()
    {
        if (this.SaleOrderId == null)
        {
            throw new InvalidOperationException("No sale order associated with this booth.");
        }

        var action = this.Env.Ref<ActionWindow>("sale.action_orders");
        action.Views = new[] { (false, "form") };
        action.ResId = this.SaleOrderId.Id;
        return action;
    }

    public string GetBoothMultilineDescription()
    {
        return $"{this.EventId.DisplayName} : \n- {this.Name}";
    }
}
