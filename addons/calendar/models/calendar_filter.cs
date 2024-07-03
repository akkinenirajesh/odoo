csharp
public partial class CalendarFilters
{
    public override string ToString()
    {
        // You might want to customize this based on your needs
        return $"Calendar Filter for {PartnerId}";
    }

    public static void UnlinkFromPartnerId(int partnerId)
    {
        var filtersToDelete = Env.Set<CalendarFilters>().Where(f => f.PartnerId.Id == partnerId);
        foreach (var filter in filtersToDelete)
        {
            Env.Delete(filter);
        }
    }
}
