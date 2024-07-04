csharp
public partial class EventTypeBooth
{
    public override string ToString()
    {
        // Implement a string representation of the EventTypeBooth
        return $"Event Type Booth: {ProductId}";
    }

    public List<string> GetEventBoothFieldsWhitelist()
    {
        var baseWhitelist = base.GetEventBoothFieldsWhitelist();
        baseWhitelist.AddRange(new[] { "ProductId", "Price" });
        return baseWhitelist;
    }
}
