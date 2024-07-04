csharp
public partial class WebsiteSaleStock.ProductProduct 
{
    public bool HasStockNotification(res.partner partner)
    {
        return Env.Ref<WebsiteSaleStock.ProductProduct>(this.Id).StockNotificationPartnerIds.Contains(partner);
    }

    public int GetCartQuantity(Website website = null)
    {
        if (!this.AllowOutOfStockOrder)
        {
            website = website ?? Env.Ref<Website>().GetCurrentWebsite();
            SaleOrder cart = website.SaleGetOrder();
            if (cart != null)
            {
                return cart.GetCommonProductLines(this).Sum(x => x.ProductUomQty);
            }
        }
        return 0;
    }

    public bool IsSoldOut()
    {
        if (!this.IsStorable)
        {
            return false;
        }
        int freeQty = Env.Ref<Website>().GetCurrentWebsite().GetProductAvailableQuantity(this);
        return freeQty <= 0;
    }

    public bool WebsiteShowQuickAdd()
    {
        return (this.AllowOutOfStockOrder || !this.IsSoldOut()) && base.WebsiteShowQuickAdd();
    }

    public void SendAvailabilityEmail()
    {
        foreach (var product in Env.Ref<WebsiteSaleStock.ProductProduct>().Search(x => x.StockNotificationPartnerIds.Count > 0))
        {
            if (product.IsSoldOut())
            {
                continue;
            }
            foreach (var partner in product.StockNotificationPartnerIds)
            {
                var selfContext = Env.WithContext(lang: partner.Lang);
                var productContext = product.WithContext(lang: partner.Lang);
                string bodyHtml = selfContext.Ref<IrQweb>().Render("website_sale_stock.availability_email_body", new { product = productContext });
                MailMessage msg = selfContext.Ref<MailMessage>().New(new { body = bodyHtml, recordName = productContext.Name });
                string fullMail = selfContext.Ref<MailRenderMixin>().RenderEncapsulate("mail.mail_notification_light", bodyHtml, new { message = msg, modelDescription = "Product" });
                var context = new { lang = partner.Lang }; // Use partner lang to translate mail subject below
                var mailValues = new
                {
                    subject = $"The product '{productContext.Name}' is now available",
                    emailFrom = (product.CompanyId.PartnerId ?? Env.User).EmailFormatted,
                    emailTo = partner.EmailFormatted,
                    bodyHtml = fullMail,
                };
                MailMail mail = selfContext.Ref<MailMail>().Create(mailValues);
                mail.Send(false);
                product.StockNotificationPartnerIds.Remove(partner);
            }
        }
    }
}
