csharp
public partial class MailResUsersSettings
{
    public void CleanupExpiredMutes()
    {
        var settings = Env.Search<MailResUsersSettings>(x => x.MuteUntilDt <= Env.Now);
        settings.Write(new { MuteUntilDt = (DateTime?)null });
        var notifications = new List<(long, string, object)>();
        foreach (var setting in settings)
        {
            notifications.Add((setting.UserId.PartnerId, "res.users.settings", new { MuteUntilDt = (DateTime?)null }));
        }
        Env.Bus.SendMany(notifications);
    }

    public object FormatSettings(List<string> fieldsToFormat)
    {
        var res = base.FormatSettings(fieldsToFormat);
        if (fieldsToFormat.Contains("VolumeSettingsIds"))
        {
            var volumeSettings = VolumeSettingsIds.DiscussUsersSettingsVolumeFormat();
            res.Remove("VolumeSettingsIds");
            res.Add("Volumes", new List<object> { new { Operation = "ADD", Values = volumeSettings } });
        }
        if (fieldsToFormat.Contains("MuteUntilDt"))
        {
            res.Add("MuteUntilDt", Env.ConvertDateTimeToString(MuteUntilDt));
        }
        return res;
    }

    public void Mute(int minutes)
    {
        if (minutes == -1)
        {
            MuteUntilDt = DateTime.MaxValue;
        }
        else if (minutes > 0)
        {
            MuteUntilDt = Env.Now.AddMinutes(minutes);
            Env.Ref("mail.ir_cron_discuss_users_settings_unmute").Trigger(MuteUntilDt);
        }
        else
        {
            MuteUntilDt = null;
        }
        Env.Bus.SendOne(UserId.PartnerId, "res.users.settings", new { MuteUntilDt = MuteUntilDt });
    }

    public object SetResUsersSettings(object newSettings)
    {
        var formated = base.SetResUsersSettings(newSettings);
        Env.Bus.SendOne(UserId.PartnerId, "res.users.settings", formated);
        return formated;
    }

    public void SetVolumeSetting(long partnerId, double volume, long? guestId = null)
    {
        var volumeSetting = Env.Search<MailResUsersSettingsVolumes>(x => x.UserSettingId == this.Id && x.PartnerId == partnerId && x.GuestId == guestId);
        if (volumeSetting.Any())
        {
            volumeSetting.First().Volume = volume;
        }
        else
        {
            volumeSetting = Env.Create<MailResUsersSettingsVolumes>(new { UserSettingId = this.Id, Volume = volume, PartnerId = partnerId, GuestId = guestId });
        }
        Env.Bus.SendOne(UserId.PartnerId, "res.users.settings.volumes", volumeSetting.First().DiscussUsersSettingsVolumeFormat());
    }
}
