csharp
public partial class EventType
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
