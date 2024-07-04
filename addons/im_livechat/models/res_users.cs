csharp
public partial class Users
{
    public string ComputeLivechatUsername()
    {
        return ResUsersSettingsId?.LivechatUsername;
    }

    public void InverseLivechatUsername()
    {
        if (ResUsersSettingsId == null)
        {
            Env.ResUsersSettings.FindOrCreateForUser(this);
        }
        ResUsersSettingsId.LivechatUsername = LivechatUsername;
    }

    public List<Lang> ComputeLivechatLangIds()
    {
        return ResUsersSettingsId?.LivechatLangIds ?? new List<Lang>();
    }

    public void InverseLivechatLangIds()
    {
        if (ResUsersSettingsId == null)
        {
            Env.ResUsersSettings.FindOrCreateForUser(this);
        }
        ResUsersSettingsId.LivechatLangIds = LivechatLangIds;
    }

    public bool ComputeHasAccessLivechat()
    {
        return Env.User.HasGroup("im_livechat.im_livechat_group_user");
    }

    public void InitStoreData(Store store)
    {
        base.InitStoreData(store);
        store.Add(new Dictionary<string, object> { { "HasAccessLivechat", Env.User.HasAccessLivechat } });
    }

    // Additional properties to represent the SELF_READABLE_FIELDS and SELF_WRITEABLE_FIELDS
    public static List<string> SelfReadableFields => base.SelfReadableFields.Concat(new[] { "LivechatUsername", "LivechatLangIds", "HasAccessLivechat" }).ToList();
    public static List<string> SelfWriteableFields => base.SelfWriteableFields.Concat(new[] { "LivechatUsername", "LivechatLangIds" }).ToList();
}
