csharp
public partial class Website_ProductAttribute
{
    public virtual void PopulateFactories()
    {
        var factories = Env.Populate.GetFactories();
        factories.Add(new PopulateFactory<Website_ProductAttribute>
        {
            Field = "Visibility",
            Values = new [] { "visible", "hidden" },
            Probabilities = new [] { 6, 3 }
        });
    }
}
