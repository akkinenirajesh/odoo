csharp
public partial class PosOrder 
{
    public void ActionSentMessageOnSms(string phone, object _)
    {
        if (!(this != null && this.ConfigId.ModulePosSms && this.ConfigId.SmsReceiptTemplateId != null && !string.IsNullOrEmpty(phone)))
        {
            return;
        }
        var smsComposer = Env.Create<SmsComposer>().WithContext(new { active_id = this.Id }).Create(
            new
            {
                CompositionMode = "comment",
                Numbers = phone,
                RecipientSingleNumberItf = phone,
                TemplateId = this.ConfigId.SmsReceiptTemplateId.Id,
                ResModel = "Pos.PosOrder"
            }
        );
        smsComposer.ActionSendSms();
    }
}
