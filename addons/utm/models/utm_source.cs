csharp
public partial class UtmSource {
    public UtmSource(Buvi.Env env)
    {
        this.Env = env;
    }

    public Buvi.Env Env { get; }

    public virtual string Name { get; set; }

    public virtual UtmSource Create(string name)
    {
        var newNames = this.Env.UtmMixin.GetUniqueNames(this.Env.Model<UtmSource>(), new List<string> { name });
        return this.Env.Create<UtmSource>(new Dictionary<string, object> { { "Name", newNames.FirstOrDefault() } });
    }

    public virtual string GenerateName(Buvi.Model record, string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        content = content.Replace("\n", " ");
        if (content.Length >= 24)
        {
            content = $"{content.Substring(0, 20)}...";
        }

        var createDateString = this.Env.Date.Strftime(record.CreateDate, this.Env.Tools.DefaultServerDateFormat);
        var modelDescription = this.Env.IrModel.Get(record.ModelName).Name;
        return this.Env.Locale.Translate(
            $"%(content)s (%(model_description)s created on %(create_date)s)",
            new Dictionary<string, object> { { "content", content }, { "model_description", modelDescription }, { "create_date", createDateString } }
        );
    }
}

public partial class UtmSourceMixin {
    public UtmSourceMixin(Buvi.Env env)
    {
        this.Env = env;
    }

    public Buvi.Env Env { get; }

    public virtual string Name { get; set; }
    public virtual UtmSource SourceId { get; set; }

    public virtual UtmSourceMixin Create(string name, string recName, string sourceId = null)
    {
        var utmSources = this.Env.UtmSource.Create(
            this.Env.UtmSource.GenerateName(this, recName)
        );
        this.SourceId = utmSources;
        return this;
    }

    public virtual UtmSourceMixin Write(string recName)
    {
        if (!string.IsNullOrEmpty(recName))
        {
            this.Name = this.Env.UtmSource.GenerateName(this, recName);
            this.Name = this.Env.UtmMixin.GetUniqueNames("Utm.UtmSource", new List<string> { this.Name }).FirstOrDefault();
        }
        return this;
    }

    public virtual UtmSourceMixin CopyData(Dictionary<string, object> defaultValues = null)
    {
        defaultValues = defaultValues ?? new Dictionary<string, object>();
        var defaultName = defaultValues.ContainsKey("Name") ? defaultValues["Name"] as string : null;
        var valsList = this.CopyData(defaultValues);
        var uniqueNames = this.Env.UtmMixin.GetUniqueNames("Utm.UtmSource", new List<string> { defaultName ?? this.Name });
        foreach (var val in valsList)
        {
            val["Name"] = uniqueNames.FirstOrDefault();
        }
        return this;
    }
}
