csharp
public partial class EventTagCategory
{
    public override string ToString()
    {
        return Name;
    }

    public int DefaultSequence()
    {
        var lastCategory = Env.Search<EventTagCategory>().OrderByDescending(c => c.Sequence).FirstOrDefault();
        return (lastCategory?.Sequence ?? 0) + 1;
    }
}

public partial class EventTag
{
    public override string ToString()
    {
        return Name;
    }

    public int DefaultColor()
    {
        return new Random().Next(1, 12);
    }
}
