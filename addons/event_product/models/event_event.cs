csharp
public partial class Event
{
    public override string ToString()
    {
        // Assuming there's a Name field in the Event model
        return Name;
    }

    // If you need to access the related currency, you might do something like this:
    public Currency GetCurrency()
    {
        return Env.Get<Currency>(CurrencyId);
    }
}
