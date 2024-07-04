csharp
public partial class PosPaymentMethod
{
    // All methods should go here

    public object GetLatestAdyenStatus()
    {
        if (!Env.User.HasGroup("point_of_sale.group_pos_user"))
        {
            throw new AccessDeniedException();
        }

        var latestResponse = Env.Database.GetModel("Pos.PosPaymentMethod").Get("AdyenLatestResponse");
        if (latestResponse != null)
        {
            return Json.Deserialize(latestResponse);
        }
        return null;
    }
}
