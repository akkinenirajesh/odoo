csharp
public partial class AccountMove {
    public bool InvoiceValidateSendEmail() {
        if (Env.Su) {
            // sending mail in sudo was meant for it being sent from superuser
            this = this.WithUser(Env.SuperUserId);
        }
        foreach (var invoice in this.Where(x => x.MoveType == "out_invoice")) {
            // send template only on customer invoice
            // subscribe the partner to the invoice
            if (!invoice.MessagePartnerIds.Contains(invoice.PartnerId)) {
                invoice.MessageSubscribe(new List<int> { invoice.PartnerId.Id });
            }
            var commentSubtype = Env.Get("ir.model.data").GetResId("mail.mt_comment");
            foreach (var line in invoice.InvoiceLineIds) {
                if (line.ProductId.EmailTemplateId != null) {
                    invoice.MessagePostWithSource(
                        line.ProductId.EmailTemplateId,
                        email_layout_xmlid: "mail.mail_notification_light",
                        subtype_id: commentSubtype
                    );
                }
            }
        }
        return true;
    }

    public AccountMove Post(bool soft = true) {
        // OVERRIDE
        var posted = this.BasePost(soft);
        posted.InvoiceValidateSendEmail();
        return posted;
    }
}
