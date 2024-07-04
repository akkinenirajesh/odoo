csharp
public partial class Website.IrModelData 
{
    public Website.IrModelData ProcessEndUnlinkRecord(Website.IrModelData record)
    {
        if (record.Context.ContainsKey("module") && record.Context["module"].ToString().StartsWith("theme_"))
        {
            var themeRecords = Env.Get("ir.module.module")._ThemeModelNames.Values;
            if (themeRecords.Contains(record.Model))
            {
                var copyIds = record.WithContext(new Dictionary<string, object>() {
                    { "active_test", false },
                    { "MODULE_UNINSTALL_FLAG", true }
                }).CopyIds;
                if (Env.Request != null)
                {
                    var currentWebsite = Env.Get("website").GetCurrentWebsite();
                    copyIds = copyIds.Where(c => c.WebsiteId == currentWebsite).ToList();
                }

                Env.Logger.Info($"Deleting {copyIds.Select(c => c.Id).ToList()}@{record.Model} (theme `copy_ids`) for website {copyIds.Select(c => c.WebsiteId).ToList()}");
                copyIds.Unlink();
            }
        }
        return record;
    }
}
