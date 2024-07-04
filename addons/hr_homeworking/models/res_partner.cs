csharp
public partial class ResPartner
{
    public void ComputeImStatus()
    {
        base.ComputeImStatus();
        foreach (var user in this.UserIds)
        {
            var dayField = Env.Get<HrEmployee>().GetCurrentDayLocationField();
            var locationType = user[dayField].LocationType;
            if (string.IsNullOrEmpty(locationType))
            {
                continue;
            }

            var imStatus = user.PartnerId.ImStatus;
            if (imStatus == "online" || imStatus == "away" || imStatus == "offline")
            {
                user.PartnerId.ImStatus = $"{locationType}_{imStatus}";
            }
        }
    }
}
