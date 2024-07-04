csharp
public partial class PointOfSaleResCountryState
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Code { get; set; }
    public virtual CoreCountry Country { get; set; }

    public virtual List<int> LoadPosDataFields(int configId)
    {
        return new List<int>() { Id, Name, Code, Country.Id };
    }
}
