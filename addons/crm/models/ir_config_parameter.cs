csharp
public partial class IrConfigParameter
{
    public override bool Write(Dictionary<string, object> vals)
    {
        bool result = base.Write(vals);
        if (this.Key == "crm.pls_fields")
        {
            Env.FlushAll();
            Env.Registry.SetupModels(Env.Cr);
        }
        return result;
    }

    public static IrConfigParameter Create(Dictionary<string, object> vals)
    {
        IrConfigParameter record = base.Create(vals);
        if (record.Key == "crm.pls_fields")
        {
            Env.FlushAll();
            Env.Registry.SetupModels(Env.Cr);
        }
        return record;
    }

    public override bool Unlink()
    {
        bool plsEmptied = this.Key == "crm.pls_fields";
        bool result = base.Unlink();
        if (plsEmptied && !Env.Context.ContainsKey("MODULE_UNINSTALL_FLAG"))
        {
            Env.FlushAll();
            Env.Registry.SetupModels(Env.Cr);
        }
        return result;
    }
}
