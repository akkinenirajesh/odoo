csharp
public partial class EventBoothRegistration
{
    public void ComputeContactName()
    {
        if (string.IsNullOrEmpty(ContactName))
        {
            ContactName = Partner?.Name ?? string.Empty;
        }
    }

    public void ComputeContactEmail()
    {
        if (string.IsNullOrEmpty(ContactEmail))
        {
            ContactEmail = Partner?.Email ?? string.Empty;
        }
    }

    public void ComputeContactPhone()
    {
        if (string.IsNullOrEmpty(ContactPhone))
        {
            ContactPhone = Partner?.Phone ?? Partner?.Mobile ?? string.Empty;
        }
    }

    public List<string> GetFieldsForBoothConfirmation()
    {
        return new List<string> { "SaleOrderLine", "Partner", "ContactName", "ContactEmail", "ContactPhone" };
    }

    public void ActionConfirm()
    {
        var values = GetFieldsForBoothConfirmation()
            .ToDictionary(field => field, field => 
                typeof(EventBoothRegistration).GetProperty(field).GetValue(this));

        EventBooth.ActionConfirm(values);
        CancelPendingRegistrations();
    }

    private void CancelPendingRegistrations()
    {
        var boothNames = string.Join("", EventBooth.Select(booth => $"<li>{booth.DisplayName}</li>"));
        var body = $"<p>Your order has been cancelled because the following booths have been reserved: <ul>{boothNames}</ul></p>";

        var otherRegistrations = Env.Query<EventBoothRegistration>()
            .Where(r => EventBooth.Contains(r.EventBooth) && r.Id != Id)
            .ToList();

        foreach (var order in otherRegistrations.Select(r => r.SaleOrderLine.Order).Distinct())
        {
            order.MessagePost(body: body, partnerIds: new[] { order.User.Partner.Id });
            order.ActionCancel();
        }

        Env.Remove(otherRegistrations);
    }
}
