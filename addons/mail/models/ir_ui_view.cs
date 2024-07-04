csharp
public partial class MailIrUiView 
{
    public bool IsQwebBasedView(string viewType)
    {
        return viewType == "activity" || Env.Call("IrUiView", "_is_qweb_based_view", viewType);
    }
}
