csharp
public partial class Event
{
    public void ComputeEventBoothIds()
    {
        if (EventTypeId == null && EventBoothIds.Count == 0)
        {
            EventBoothIds.Clear();
            return;
        }

        var boothsToRemove = EventBoothIds.Where(booth => booth.IsAvailable).ToList();
        foreach (var booth in boothsToRemove)
        {
            EventBoothIds.Remove(booth);
        }

        if (EventTypeId?.EventTypeBoothIds != null)
        {
            foreach (var line in EventTypeId.EventTypeBoothIds)
            {
                var newBooth = Env.New<Event.Booth>();
                foreach (var field in Env.Get<Event.TypeBooth>().GetEventBoothFieldsWhitelist())
                {
                    newBooth[field] = line[field];
                }
                EventBoothIds.Add(newBooth);
            }
        }
    }

    public Dictionary<int, int> GetBoothStatCount()
    {
        var elements = Env.Get<Event.Booth>().ReadGroup(
            new[] { ("EventId", "in", new[] { this.Id }) },
            new[] { "EventId", "State" },
            new[] { "__count" }
        );

        var elementsTotalCount = new Dictionary<int, int>();
        var elementsAvailableCount = new Dictionary<int, int>();

        foreach (var element in elements)
        {
            var eventId = (int)element["EventId"];
            var state = (string)element["State"];
            var count = (int)element["__count"];

            if (state == "available")
            {
                elementsAvailableCount[eventId] = count;
            }

            if (!elementsTotalCount.ContainsKey(eventId))
            {
                elementsTotalCount[eventId] = 0;
            }
            elementsTotalCount[eventId] += count;
        }

        return (elementsAvailableCount, elementsTotalCount);
    }

    public void ComputeEventBoothCount()
    {
        if (Id != 0)
        {
            var (boothsAvailableCount, boothsTotalCount) = GetBoothStatCount();
            EventBoothCountAvailable = boothsAvailableCount.GetValueOrDefault(Id, 0);
            EventBoothCount = boothsTotalCount.GetValueOrDefault(Id, 0);
        }
        else
        {
            EventBoothCount = EventBoothIds.Count;
            EventBoothCountAvailable = EventBoothIds.Count(booth => booth.IsAvailable);
        }
    }

    public void ComputeEventBoothCategoryIds()
    {
        EventBoothCategoryIds = EventBoothIds.Select(booth => booth.BoothCategoryId).Distinct().ToList();
    }

    public void ComputeEventBoothCategoryAvailableIds()
    {
        EventBoothCategoryAvailableIds = EventBoothIds
            .Where(booth => booth.IsAvailable)
            .Select(booth => booth.BoothCategoryId)
            .Distinct()
            .ToList();
    }
}
