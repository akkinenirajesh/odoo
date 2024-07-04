csharp
public partial class TestTranslationImportModel1 
{
    public string GetCodeTranslation() 
    {
        return Env.Translate("slot"); 
    }

    public string GetCodeLazyTranslation()
    {
        return Env.Translate("Code Lazy, English");
    }

    public string GetCodePlaceholderTranslation(params object[] args)
    {
        return Env.Translate("Code, %s, English", args);
    }

    public string GetCodeNamedPlaceholderTranslation(params object[] args)
    {
        return Env.Translate("Code, %(num)s, %(symbol)s, English", args);
    }
}
