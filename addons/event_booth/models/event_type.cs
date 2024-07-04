csharp
public partial class EventType
{
    public override string ToString()
    {
        // Assuming there's a Name field in the EventType model
        return Name;
    }

    public void AddBooth(EventTypeBooth booth)
    {
        if (booth != null)
        {
            booth.EventTypeId = this;
            EventTypeBoothIds = EventTypeBoothIds.Append(booth).ToArray();
        }
    }

    public void RemoveBooth(EventTypeBooth booth)
    {
        if (booth != null)
        {
            EventTypeBoothIds = EventTypeBoothIds.Where(b => b != booth).ToArray();
            booth.EventTypeId = null;
        }
    }
}
