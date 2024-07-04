csharp
public partial class ResPartner
{
    public void ComputeImStatus()
    {
        // Call the base implementation
        base.ComputeImStatus();

        var absentNow = GetOnLeaveIds();
        if (absentNow.Contains(this.Id))
        {
            switch (this.ImStatus)
            {
                case "online":
                    this.ImStatus = "leave_online";
                    break;
                case "away":
                    this.ImStatus = "leave_away";
                    break;
                default:
                    this.ImStatus = "leave_offline";
                    break;
            }
        }
    }

    public List<int> GetOnLeaveIds()
    {
        return Env.Get<Hr.User>().GetOnLeaveIds(partner: true);
    }

    public Dictionary<int, Dictionary<string, object>> MailPartnerFormat(List<string> fields = null)
    {
        var partnersFormat = base.MailPartnerFormat(fields);

        if (fields == null || fields.Contains("OutOfOfficeDateEnd"))
        {
            var dates = this.Users.Select(u => u.LeaveDateTo).Where(d => d.HasValue).ToList();
            var states = this.Users.Select(u => u.CurrentLeaveState).Where(s => !string.IsNullOrEmpty(s)).ToList();

            var date = dates.Any() && dates.All(d => d.HasValue) ? dates.Min() : (DateTime?)null;
            var state = states.Any() && states.All(s => !string.IsNullOrEmpty(s)) ? states.First() : null;

            if (partnersFormat.ContainsKey(this.Id))
            {
                partnersFormat[this.Id]["OutOfOfficeDateEnd"] = (state == "validate" && date.HasValue) ? date.Value.ToString("yyyy-MM-dd") : null;
            }
        }

        return partnersFormat;
    }
}
