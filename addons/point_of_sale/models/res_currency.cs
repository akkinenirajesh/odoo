csharp
public partial class PointOfSale.ResCurrency {
    public virtual List<PointOfSale.ResCurrency> LoadPosData(dynamic data) {
        var configId = data["pos.config"]["data"][0]["currency_id"];
        return Env.Search<PointOfSale.ResCurrency>(new Dictionary<string, object> { {"Id", configId }});
    }
    public virtual List<string> LoadPosDataFields(dynamic configId) {
        return new List<string> {"Id", "Name", "Symbol", "Position", "Rounding", "Rate", "DecimalPlaces", "ISONumeric"};
    }
}
