csharp
public partial class PointOfSaleBill {
    public virtual int Id { get; set; }

    public virtual string Name { get; set; }

    public virtual double Value { get; set; }

    public virtual bool ForAllConfig { get; set; }

    public virtual ICollection<PointOfSalePosConfig> PosConfigIds { get; set; }

    public virtual void NameCreate(string name) {
        if (!double.TryParse(name, out double value)) {
            throw new Exception("The name of the Coins/Bills must be a number.");
        }

        var result = Env.Create(new PointOfSaleBill { Name = name, Value = value });
        return result.Id;
    }

    public virtual IEnumerable<object> LoadPosDataDomain(object data) {
        return Enumerable.Empty<object>();
    }

    public virtual IEnumerable<object> LoadPosDataFields(int configId) {
        return new object[] { "Id", "Name", "Value" };
    }
}
