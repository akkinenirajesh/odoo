csharp
public partial class EventRegistration
{
    public virtual Event Event { get; set; }
    public virtual Core.Partner Attendee { get; set; }

    public virtual string GetDefaultDomain(MassMailing.MassMailing mailing)
    {
        return "[('State', '!=', 'cancel')]";
    }
}
