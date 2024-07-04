csharp
public partial class TransifexCodeTranslation
{
    public string Source { get; set; }
    public string Value { get; set; }
    public string Module { get; set; }
    public string Lang { get; set; }
    public string TransifexUrl { get; set; }

    public List<Tuple<string, string>> GetLanguages()
    {
        return Env.Get("Res.Lang").GetInstalled();
    }

    public void ComputeTransifexUrl()
    {
        this.TransifexUrl = false;
        Env.Get("Transifex.Translation").UpdateTransifexUrl(this);
    }

    public bool LoadCodeTranslations(List<string> moduleNames = null, List<string> langs = null)
    {
        try
        {
            Env.Cr.Execute($"LOCK TABLE {this.GetTableName()} IN EXCLUSIVE MODE NOWAIT");

            if (moduleNames == null)
            {
                moduleNames = Env.Get("Ir.Module.Module").Search(new List<Tuple<string, object>> { new Tuple<string, object>("State", "installed") }).Select(x => x.Name).ToList();
            }

            if (langs == null)
            {
                langs = GetLanguages().Where(x => x.Item1 != "en_US").Select(x => x.Item1).ToList();
            }

            Env.Cr.Execute($"SELECT DISTINCT module, lang FROM {this.GetTableName()}");
            var loadedCodeTranslations = Env.Cr.FetchAll().Select(x => new Tuple<string, string>(x[0].ToString(), x[1].ToString())).ToHashSet();

            var createValueList = new List<TransifexCodeTranslation>();
            foreach (var moduleName in moduleNames)
            {
                foreach (var lang in langs)
                {
                    if (!loadedCodeTranslations.Contains(new Tuple<string, string>(moduleName, lang)))
                    {
                        foreach (var kvp in CodeTranslations.GetCodeTranslations(moduleName, lang, x => true))
                        {
                            createValueList.Add(new TransifexCodeTranslation
                            {
                                Source = kvp.Key,
                                Value = kvp.Value,
                                Module = moduleName,
                                Lang = lang
                            });
                        }
                    }
                }
            }

            Env.Get("Transifex.TransifexCodeTranslation").Create(createValueList);

            return true;
        }
        catch (Exception ex)
        {
            if (ex is PostgresException && ex.Message.Contains("LockNotAvailable"))
            {
                return false;
            }
            throw;
        }
    }

    public Dictionary<string, object> OpenCodeTranslations()
    {
        LoadCodeTranslations();
        return new Dictionary<string, object>
        {
            { "name", "Code Translations" },
            { "type", "ir.actions.act_window" },
            { "res_model", "Transifex.TransifexCodeTranslation" },
            { "view_mode", "list" }
        };
    }

    public bool Reload()
    {
        Env.Cr.Execute($"DELETE FROM {this.GetTableName()}");
        return LoadCodeTranslations();
    }

    // Helper Methods
    private string GetTableName()
    {
        return $"_{this.GetType().Name.Replace(".", "_")}";
    }
}
