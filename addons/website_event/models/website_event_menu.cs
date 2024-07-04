csharp
public partial class WebsiteEventMenu
{
    public void Unlink()
    {
        Env.GetModel("Ir.Ui.View").Call("Unlink", new object[] { this.ViewId });
        Env.GetModel("Website.EventMenu").Call("Unlink", new object[] { this });
    }
}
