csharp
public partial class IrMailServer
{
    public List<Mailing> ActiveMailingIds { get; set; }

    public List<string> ActiveUsagesCompute()
    {
        List<string> usagesSuper = Env.CallMethod<List<string>>(this, "_active_usages_compute");
        int defaultMailServerId = Env.CallMethod<int>("mailing.mailing", "_get_default_mail_server_id");
        List<string> usages = new List<string>();
        if (defaultMailServerId == this.Id)
        {
            usages.Add(Env.Translate("Email Marketing uses it as its default mail server to send mass mailings"));
        }
        foreach (Mailing mailing in this.ActiveMailingIds)
        {
            string baseText = Env.Translate("Mass Mailing \"{0}\"", mailing.DisplayName);
            string detailsText = string.Empty;
            if (mailing.ScheduleDate != null)
            {
                detailsText = Env.Translate("(scheduled for {0})", Env.FormatDate(mailing.ScheduleDate));
            }
            string formattedUsage = string.Format("{0} {1}", baseText, detailsText);
            usages.Add(formattedUsage);
        }
        if (usages.Count > 0)
        {
            usagesSuper.AddRange(usages);
        }
        return usagesSuper;
    }
}
