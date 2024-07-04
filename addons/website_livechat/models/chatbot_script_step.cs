csharp
public partial class WebsiteChatbotScriptStep {
    public WebsiteChatbotScriptStep _chatbotPrepareCustomerValues(WebsiteDiscussChannel discussChannel, bool createPartner, bool updatePartner)
    {
        var values = Env.Call<WebsiteChatbotScriptStep>("_chatbot_prepare_customer_values", this, discussChannel, createPartner, updatePartner);

        // sudo - website.visitor: chat bot can access visitor information
        if (var visitorSudo = discussChannel.LivechatVisitorId.Sudo())
        {
            if (string.IsNullOrEmpty(values.Email) && !string.IsNullOrEmpty(visitorSudo.Email))
            {
                values.Email = visitorSudo.Email;
            }

            if (string.IsNullOrEmpty(values.Phone) && !string.IsNullOrEmpty(visitorSudo.Mobile))
            {
                values.Phone = visitorSudo.Mobile;
            }

            values.Country = visitorSudo.CountryId != null ? new CoreCountry { Id = visitorSudo.CountryId } : null;
        }

        return values;
    }
}
