csharp
public partial class PointOfSale.ResUsers
{
    public virtual object LoadPosData(object data)
    {
        var domain = this.LoadPosDataDomain(data);
        var fields = this.LoadPosDataFields(data["pos.config"]["data"][0]["id"]);
        var user = Env.SearchRead(this, domain, fields, false);
        user[0]["role"] = (data["pos.config"]["data"][0]["group_pos_manager_id"] as List<object>).Contains(user[0]["groups_id"]) ? "manager" : "cashier";
        (user[0] as Dictionary<string, object>).Remove("groups_id");
        return new { data = user, fields = fields };
    }

    public virtual object LoadPosDataDomain(object data)
    {
        return new List<object> { new { id = Env.Uid } };
    }

    public virtual object LoadPosDataFields(object configId)
    {
        return new List<object> { "id", "name", "partner_id", "groups_id" };
    }
}
