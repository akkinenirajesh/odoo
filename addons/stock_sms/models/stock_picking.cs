csharp
public partial class StockPicking
{
    public virtual bool PreActionDoneHook()
    {
        var res = base.PreActionDoneHook();
        if (res && !Env.Context.ContainsKey("skip_sms"))
        {
            var pickingsToWarnSms = CheckWarnSms();
            if (pickingsToWarnSms != null)
            {
                return pickingsToWarnSms.ActionGenerateWarnSmsWizard();
            }
        }
        return res;
    }

    public virtual StockPicking CheckWarnSms()
    {
        var warnSmsPickings = new StockPicking();
        foreach (var picking in this)
        {
            var isDelivery = picking.Company.StockMoveSmsValidation && picking.PickingTypeId.Code == "outgoing" && (!string.IsNullOrEmpty(picking.Partner.Mobile) || !string.IsNullOrEmpty(picking.Partner.Phone));
            if (isDelivery && !Thread.CurrentThread.Name.Equals("testing") && !Env.Registry.InTestMode() && !picking.Company.HasReceivedWarningStockSms && picking.Company.StockMoveSmsValidation)
            {
                warnSmsPickings |= picking;
            }
        }
        return warnSmsPickings;
    }

    public virtual ConfirmStockSms ActionGenerateWarnSmsWizard()
    {
        var view = Env.Ref("stock_sms.view_confirm_stock_sms");
        var wiz = Env.Create<ConfirmStockSms>(new { PickIds = this });
        return new ConfirmStockSms {
            Name = "SMS",
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "Confirm.StockSms",
            Views = new List<object> { new object[] { view.Id, "form" } },
            ViewId = view.Id,
            Target = "new",
            ResId = wiz.Id,
            Context = Env.Context
        };
    }

    public virtual void SendConfirmationEmail()
    {
        base.SendConfirmationEmail();
        if (!Env.Context.ContainsKey("skip_sms") && !Thread.CurrentThread.Name.Equals("testing") && !Env.Registry.InTestMode())
        {
            var pickings = this.Where(p => p.Company.StockMoveSmsValidation && p.PickingTypeId.Code == "outgoing" && (!string.IsNullOrEmpty(p.Partner.Mobile) || !string.IsNullOrEmpty(p.Partner.Phone))).ToList();
            foreach (var picking in pickings)
            {
                // Sudo as the user has not always the right to read this sms template.
                var template = picking.Company.StockSmsConfirmationTemplate;
                picking.MessageSmsWithTemplate(template, picking.Partner.Id, false);
            }
        }
    }

    public virtual void MessageSmsWithTemplate(StockSmsTemplate template, int partnerId, bool putInQueue)
    {
        // Implement this method based on your SMS integration logic
    }
}

public partial class ConfirmStockSms
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string ViewMode { get; set; }
    public string ResModel { get; set; }
    public List<object> Views { get; set; }
    public int ViewId { get; set; }
    public string Target { get; set; }
    public int ResId { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public List<StockPicking> PickIds { get; set; }
}

public partial class StockSmsTemplate
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}

public partial class ResCompany
{
    public int Id { get; set; }
    public bool StockMoveSmsValidation { get; set; }
    public bool HasReceivedWarningStockSms { get; set; }
    public StockSmsTemplate StockSmsConfirmationTemplate { get; set; }
}

public partial class StockPickingType
{
    public int Id { get; set; }
    public string Code { get; set; }
}

public partial class ResPartner
{
    public int Id { get; set; }
    public string Mobile { get; set; }
    public string Phone { get; set; }
}
