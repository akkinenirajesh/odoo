csharp
public partial class L10nLatamIdentificationType {
    public virtual Core.Country Country { get; set; }
    public virtual string Code { get; set; }
    public virtual string L10nPeVatCode { get; set; }
    public virtual string Name { get; set; }
    public virtual bool Active { get; set; }

    public object LoadPosDataDomain(object data) {
        if (Env.Company.Country.Code == "PE") {
            return new object[] { new object[] { "L10nPeVatCode", "!=", false } };
        } else {
            return base.LoadPosDataDomain(data);
        }
    }

    public object LoadPosDataFields(object configId) {
        return new string[] { "Name" };
    }
}
