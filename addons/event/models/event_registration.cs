csharp
public partial class EventRegistration
{
    public override string ToString()
    {
        return Name ?? $"#{Id}";
    }

    public void ActionSetPreviousState()
    {
        if (State == EventRegistrationState.Open)
        {
            ActionSetDraft();
        }
        else if (State == EventRegistrationState.Done)
        {
            ActionConfirm();
        }
    }

    public void ActionSetDraft()
    {
        State = EventRegistrationState.Draft;
    }

    public void ActionConfirm()
    {
        State = EventRegistrationState.Open;
    }

    public void ActionSetDone()
    {
        State = EventRegistrationState.Done;
    }

    public void ActionCancel()
    {
        State = EventRegistrationState.Cancel;
    }

    public void ActionSendBadgeEmail()
    {
        // Logic to send badge email
        // This would typically involve using the Env to access email templates and sending functionality
    }

    public void UpdateMailSchedulers()
    {
        if (State != EventRegistrationState.Open)
        {
            return;
        }

        // Logic to update mail schedulers
        // This would typically involve using the Env to access and update related records
    }

    public string GetRandomBarcode()
    {
        // Logic to generate a random barcode
        // This would typically use a C# equivalent of the Python os.urandom() function
        return "";  // Placeholder return
    }
}
