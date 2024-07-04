csharp
public partial class MailResUsersSettingsVolumes 
{
    public void Init()
    {
        Env.Cr.Execute($"CREATE UNIQUE INDEX IF NOT EXISTS res_users_settings_volumes_partner_unique ON {this._Table} (UserSettingId, PartnerId) WHERE PartnerId IS NOT NULL");
        Env.Cr.Execute($"CREATE UNIQUE INDEX IF NOT EXISTS res_users_settings_volumes_guest_unique ON {this._Table} (UserSettingId, GuestId) WHERE GuestId IS NOT NULL");
    }

    public string _Table => "res_users_settings_volumes";

    public string DisplayName 
    {
        get 
        {
            var user = Env.Ref<MailResUsersSettings>(this.UserSettingId).User;
            var partnerName = this.PartnerId != null ? Env.Ref<CorePartner>(this.PartnerId).Name : "";
            var guestName = this.GuestId != null ? Env.Ref<CorePartner>(this.GuestId).Name : "";
            return $"{user.Name} - {partnerName ?? guestName}";
        }
    }

    public List<Dictionary<string, object>> DiscussUsersSettingsVolumeFormat()
    {
        return this.Select(volumeSetting => new Dictionary<string, object>()
        {
            {"id", volumeSetting.Id},
            {"volume", volumeSetting.Volume},
            {"persona", new Dictionary<string, object>() 
                {
                    {"id", volumeSetting.PartnerId != null ? volumeSetting.PartnerId : volumeSetting.GuestId},
                    {"name", volumeSetting.PartnerId != null ? Env.Ref<CorePartner>(volumeSetting.PartnerId).Name : Env.Ref<CorePartner>(volumeSetting.GuestId).Name },
                    {"type", volumeSetting.PartnerId != null ? "partner" : "guest" }
                }
            },
            {"user_setting_id", new Dictionary<string, object>() 
                { 
                    {"id", volumeSetting.UserSettingId}
                }
            }
        }).ToList();
    }
}
