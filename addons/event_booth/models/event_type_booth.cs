csharp
public partial class TypeBooth
{
    public override string ToString()
    {
        return Name;
    }

    public Event.BoothCategory GetDefaultBoothCategory()
    {
        var categoryId = Env.Search<Event.BoothCategory>();
        if (categoryId != null && categoryId.Count() == 1)
        {
            return categoryId.First();
        }
        return null;
    }

    public List<string> GetEventBoothFieldsWhitelist()
    {
        return new List<string> { "Name", "BoothCategory" };
    }
}
