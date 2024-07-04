csharp
public partial class Sms.IrActionsServer 
{
    public virtual void ComputeAvailableModelIds()
    {
        if (this.State == "sms")
        {
            var mailModels = Env.Get("Core.IrModel").Search(x => x.IsMailThread == true && x.Transient == false);
            this.AvailableModelIds = mailModels.Ids;
        }
        else
        {
            base.ComputeAvailableModelIds();
        }
    }

    public virtual void ComputeSmsTemplateId()
    {
        if (this.State != "sms" || this.ModelId != this.SmsTemplateId.ModelId)
        {
            this.SmsTemplateId = null;
        }
    }

    public virtual void ComputeSmsMethod()
    {
        if (this.State != "sms")
        {
            this.SmsMethod = null;
        }
        else
        {
            this.SmsMethod = "sms";
        }
    }

    public virtual void CheckSmsModelCoherency()
    {
        if (this.State == "sms" && (this.ModelId.Transient || !this.ModelId.IsMailThread))
        {
            throw new Exception("Sending SMS can only be done on a mail.thread or a transient model");
        }
    }

    public virtual void CheckSmsTemplateModel()
    {
        if (this.State == "sms" && this.SmsTemplateId != null && this.SmsTemplateId.ModelId != this.ModelId)
        {
            throw new Exception($"SMS template model of {this.Name} does not match action model.");
        }
    }

    public virtual void RunActionSmsMulti(object evalContext)
    {
        if (this.SmsTemplateId == null || this.IsRecompute())
        {
            return;
        }

        var records = evalContext.GetProperty("records") ?? evalContext.GetProperty("record");
        if (records == null)
        {
            return;
        }

        var composer = Env.Get("Sms.SmsComposer").WithContext(
            new {
                default_res_model = records.Name,
                default_res_ids = records.Ids,
                default_composition_mode = this.SmsMethod == "comment" ? "comment" : "mass",
                default_template_id = this.SmsTemplateId.Id,
                default_mass_keep_log = this.SmsMethod == "note",
            }).Create({});

        composer.ActionSendSms();
    }
}
