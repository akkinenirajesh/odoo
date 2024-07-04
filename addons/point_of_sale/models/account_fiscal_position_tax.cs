C#
public partial class AccountFiscalPositionTax {
    public void LoadPosDataDomain(dynamic data) {
        var fiscalPositions = data["account.fiscal.position"]["data"];
        var positionIds = new List<int>();
        foreach (var fiscalPosition in fiscalPositions) {
            positionIds.Add(fiscalPosition["id"]);
        }

        var domain = new List<dynamic>();
        domain.Add(new { field = "PositionId", operator = "in", value = positionIds });

        Env.Context["default_domain"] = domain;
    }
}
