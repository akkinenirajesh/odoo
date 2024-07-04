csharp
using System;
using System.Collections.Generic;

public partial class ResLang
{
    public LangDataDict GetFrontend()
    {
        // Return the available languages for current request
        return Env.GetActiveLangsByCode();
    }
}

// Assuming LangDataDict is defined elsewhere
public class LangDataDict : Dictionary<string, LangData>
{
    // Implementation details
}

public class LangData
{
    // Properties and methods for language data
}
