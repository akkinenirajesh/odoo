csharp
public partial class Snailmail.IrActionsReport 
{
    public object RetrieveAttachment(object record) 
    {
        // Override this method in order to force to re-render the pdf in case of
        // using snailmail
        if (Env.Context.Get("snailmail_layout") != null) 
        {
            return false;
        }
        return Env.Model("ir.actions.report").Call("retrieve_attachment", record);
    }

    public object GetPaperFormat() 
    {
        // force the right format (euro/A4) when sending letters, only if we are not using the l10n_DE layout
        object res = Env.Model("ir.actions.report").Call("get_paperformat");
        if (Env.Context.Get("snailmail_layout") != null && res != Env.Ref("l10n_de.paperformat_euro_din", false)) 
        {
            object paperformatId = Env.Ref("base.paperformat_euro");
            return paperformatId;
        } 
        else 
        {
            return res;
        }
    }
}
