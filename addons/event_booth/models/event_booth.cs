csharp
public partial class EventBooth
{
    public void ComputeContactName()
    {
        if (string.IsNullOrEmpty(ContactName))
        {
            ContactName = PartnerId?.Name;
        }
    }

    public void ComputeContactEmail()
    {
        if (string.IsNullOrEmpty(ContactEmail))
        {
            ContactEmail = PartnerId?.Email;
        }
    }

    public void ComputeContactPhone()
    {
        if (string.IsNullOrEmpty(ContactPhone))
        {
            ContactPhone = PartnerId?.Phone ?? PartnerId?.Mobile;
        }
    }

    public void ComputeIsAvailable()
    {
        IsAvailable = State == EventBoothState.Available;
    }

    public IEnumerable<int> SearchIsAvailable(string @operator, object operand)
    {
        bool negative = NegativeTermOperators.Contains(@operator);
        if ((negative && (bool)operand) || !(bool)operand)
        {
            return Env.Query<EventBooth>().Where(b => b.State == EventBoothState.Unavailable).Select(b => b.Id);
        }
        return Env.Query<EventBooth>().Where(b => b.State == EventBoothState.Available).Select(b => b.Id);
    }

    public void PostConfirmationMessage()
    {
        EventId.MessagePostWithSource(
            "event_booth.event_booth_booked_template",
            new Dictionary<string, object> { { "booth", this } },
            "event_booth.mt_event_booth_booked"
        );
    }

    public void ActionConfirm(Dictionary<string, object> additionalValues = null)
    {
        var writeVals = new Dictionary<string, object> { { "State", EventBoothState.Unavailable } };
        if (additionalValues != null)
        {
            foreach (var kvp in additionalValues)
            {
                writeVals[kvp.Key] = kvp.Value;
            }
        }
        Write(writeVals);
    }

    public void ActionPostConfirm(Dictionary<string, object> writeVals)
    {
        PostConfirmationMessage();
    }
}
