csharp
public partial class IrModuleModule
{
    public ActionResult ActionOpenInstallRequest()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Target = "new",
            Name = $"Activation Request of \"{this.ShortDesc}\"",
            ViewMode = "form",
            ResModel = "Base.ModuleInstallRequest",
            Context = new Dictionary<string, object>
            {
                { "default_module_id", this.Id }
            }
        };
    }

    public override string ToString()
    {
        return ShortDesc;
    }
}
