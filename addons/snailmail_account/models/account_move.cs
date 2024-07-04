csharp
public partial class AccountMove 
{
    public virtual AccountMove _get_pdf_and_send_invoice_vals(AccountTemplate template, params object[] kwargs) 
    {
        var vals = Env.Call("super", "_get_pdf_and_send_invoice_vals", template, kwargs);
        vals["CheckboxSendByPost"] = false;
        return vals;
    }
}
