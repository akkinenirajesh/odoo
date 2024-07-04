csharp
public partial class Transaction
{
    public void PostProcess()
    {
        base.PostProcess();
        if (State == "done" && IsDonation)
        {
            SendDonationEmail();
            var msg = new List<string> { "Payment received from donation with following details:" };
            var fields = new[] { "CompanyId", "PartnerId", "PartnerName", "PartnerCountryId", "PartnerEmail" };
            foreach (var field in fields)
            {
                var fieldName = Env.Fields[GetType().Name][field].Label;
                var value = GetType().GetProperty(field).GetValue(this);
                if (value != null)
                {
                    if (value.GetType().GetProperty("Name") != null)
                    {
                        value = value.GetType().GetProperty("Name").GetValue(value);
                    }
                    msg.Add($"<br/>- {fieldName}: {value}");
                }
            }
            PaymentId.MessageLog(string.Join("", msg));
        }
    }

    public void SendDonationEmail(bool isInternalNotification = false, string comment = null, string recipientEmail = null)
    {
        if (isInternalNotification || State == "done")
        {
            var subject = isInternalNotification ? "A donation has been made on your website" : "Donation confirmation";
            var body = Env.IrQweb.Render("website_payment.donation_mail_body", new
            {
                is_internal_notification = isInternalNotification,
                tx = this,
                comment = comment
            });

            Env.Ref("website_payment.mail_template_donation").SendMail(
                Id,
                emailLayoutXmlid: "mail.mail_notification_light",
                emailValues: new
                {
                    email_to = isInternalNotification ? recipientEmail : PartnerEmail,
                    email_from = CompanyId.EmailFormatted,
                    author_id = PartnerId.Id,
                    subject = subject,
                    body_html = body
                },
                forceSend: true
            );
        }
    }
}
