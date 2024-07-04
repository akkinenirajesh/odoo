C#
public partial class SaleOrder
{
    public void SendOrderNotificationMail(MailTemplate mailTemplate)
    {
        bool asyncSend = Env.ConfigParameters.GetBool("sale.async_emails");
        Cron cron = asyncSend ? Env.Ref("sale_async_emails.cron") : null;
        if (asyncSend && cron && !Env.Context.ContainsKey("is_async_email"))
        {
            this.PendingEmailTemplateId = mailTemplate;
            cron.Trigger();
        }
        else
        {
            base.SendOrderNotificationMail(mailTemplate);
        }
    }

    public void CronSendPendingEmails(bool autoCommit = true)
    {
        var pendingEmailOrders = Env.Search<SaleOrder>("PendingEmailTemplateId != null");
        foreach (SaleOrder order in pendingEmailOrders)
        {
            order.WithContext("is_async_email", true).SendOrderNotificationMail(order.PendingEmailTemplateId);
            order.PendingEmailTemplateId = null;
            if (autoCommit)
            {
                Env.Cr.Commit();
            }
        }
    }
}
