C#
public partial class LunchSupplier
{
    public void SyncCron()
    {
        // Implement logic to sync cron based on fields like SendBy, AutomaticEmailTime, etc.
    }

    public void ComputeDisplayName()
    {
        // Implement logic to compute DisplayName based on Name and Phone fields.
    }

    public void Create(List<Dictionary<string, object>> valsList)
    {
        // Implement logic to create new LunchSupplier records with appropriate cron configuration.
    }

    public void Write(Dictionary<string, object> values)
    {
        // Implement logic to update existing LunchSupplier records and handle cron adjustments.
    }

    public void Unlink()
    {
        // Implement logic to delete existing LunchSupplier records and associated crons.
    }

    public void ToggleActive()
    {
        // Implement logic to toggle Active state and update related LunchProduct records accordingly.
    }

    public void CancelFutureDays(List<string> weekdays)
    {
        // Implement logic to cancel orders for specific weekdays based on given weekday names.
    }

    public List<LunchOrder> GetCurrentOrders(string state = "ordered")
    {
        // Implement logic to retrieve orders based on current date and specified state.
    }

    public void SendAutoEmail()
    {
        // Implement logic to send email with order details to supplier if SendBy is 'Mail' and availableToday is true.
    }

    public void ComputeAvailableToday()
    {
        // Implement logic to compute AvailableToday based on RecurrencyEndDate and weekday fields.
    }

    public bool AvailableOnDate(DateTime date)
    {
        // Implement logic to check if supplier is available on a specific date.
    }

    public void ComputeOrderDeadlinePassed()
    {
        // Implement logic to compute OrderDeadlinePassed based on AutomaticEmailTime, SendBy, and AvailableToday.
    }

    public List<LunchSupplier> SearchAvailableToday(string operator, object value)
    {
        // Implement logic to filter records based on available today status.
    }

    public void ComputeButtons()
    {
        // Implement logic to compute ShowOrderButton and ShowConfirmButton based on order status.
    }

    public void ActionSendOrders()
    {
        // Implement logic to send orders for suppliers with SendBy as 'Mail' and trigger action_send for other orders.
    }

    public void ActionConfirmOrders()
    {
        // Implement logic to confirm orders with status 'sent'.
    }
}
