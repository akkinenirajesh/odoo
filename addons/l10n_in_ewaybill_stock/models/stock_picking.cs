csharp
public partial class Picking
{
    public ActionResult GetL10nInEwaybillFormAction()
    {
        return Env.Ref("l10n_in_ewaybill_stock.l10n_in_ewaybill_form_action").GetActionDict();
    }

    public ActionResult ActionL10nInEwaybillCreate()
    {
        var productWithNoHsn = MoveIds.Select(m => m.ProductId)
            .Where(product => string.IsNullOrEmpty(product.L10nInHsnCode))
            .ToList();

        if (productWithNoHsn.Any())
        {
            var productNames = string.Join("\n", productWithNoHsn.Select(p => p.Name));
            throw new UserError($"Please set HSN code in below products: \n{productNames}");
        }

        if (L10nInEwaybill.Any())
        {
            throw new UserError("Ewaybill already created for this picking.");
        }

        var action = GetL10nInEwaybillFormAction();
        var ewaybill = Env.GetModel<L10n.In.Ewaybill>().Create(new Dictionary<string, object>
        {
            { "PickingId", Id },
            { "TypeId", Env.Ref("l10n_in_ewaybill_stock.type_delivery_challan_sub_others").Id }
        });

        action["res_id"] = ewaybill.Id;
        return action;
    }

    public ActionResult ActionOpenL10nInEwaybill()
    {
        var action = GetL10nInEwaybillFormAction();
        action["res_id"] = L10nInEwaybill.First().Id;
        return action;
    }
}
