csharp
public partial class ResLang 
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual string Code { get; set; }
    public virtual bool Active { get; set; }
    public virtual string Direction { get; set; }
    public virtual string Date { get; set; }
    public virtual string Time { get; set; }
    public virtual int WeekStart { get; set; }
    public virtual string DecimalPoint { get; set; }
    public virtual string ThousandsSep { get; set; }
    public virtual string CurrencySymbol { get; set; }
    public virtual string CurrencyPosition { get; set; }
    public virtual string MonetarySeparator { get; set; }
    public virtual int Group { get; set; }
    public virtual List<int> Groups { get; set; }
    public virtual List<int> Countries { get; set; }
    public virtual int Parent { get; set; }
    public virtual List<int> Languages { get; set; }

    public virtual List<string> LoadPosDataFields(int configId)
    {
        return new List<string> { "Id", "Name", "Code" };
    }
}
