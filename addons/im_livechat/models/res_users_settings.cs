csharp
public partial class ResUsersSettings
{
    public override string ToString()
    {
        return LivechatUsername ?? base.ToString();
    }

    public void AddLivechatLanguage(Core.ResLang lang)
    {
        if (!LivechatLangIds.Contains(lang))
        {
            var updatedLangs = new List<Core.ResLang>(LivechatLangIds) { lang };
            LivechatLangIds = updatedLangs.ToArray();
        }
    }

    public void RemoveLivechatLanguage(Core.ResLang lang)
    {
        if (LivechatLangIds.Contains(lang))
        {
            var updatedLangs = new List<Core.ResLang>(LivechatLangIds);
            updatedLangs.Remove(lang);
            LivechatLangIds = updatedLangs.ToArray();
        }
    }
}
