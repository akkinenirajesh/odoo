csharp
public partial class WebsiteEventRegistration
{
    public WebsiteEventRegistration()
    {
    }

    public virtual WebsiteEvent EventID { get; set; }
    public virtual ResPartner PartnerID { get; set; }
    public virtual WebsiteEventTicket EventTicketID { get; set; }
    public virtual WebsiteVisitor VisitorID { get; set; }

    public string GetWebsiteRegistrationAllowedFields()
    {
        return "Name, Phone, Email, CompanyName, EventID, PartnerID, EventTicketID";
    }
}
