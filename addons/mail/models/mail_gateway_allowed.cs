csharp
public partial class MailGatewayAllowed
{
    public void ComputeEmailNormalized()
    {
        this.EmailNormalized = Env.Tools.EmailNormalize(this.Email);
    }

    public static Markup GetEmptyListHelp(string helpMessage)
    {
        var getParam = Env.IrConfigParameter.GetParam;
        var loopMinutes = int.Parse(getParam("mail.gateway.loop.minutes", "120"));
        var loopThreshold = int.Parse(getParam("mail.gateway.loop.threshold", "20"));

        return Markup.Parse($@"
            <p class=""o_view_nocontent_smiling_face"">
                Add addresses to the Allowed List
            </p><p>
                To protect you from spam and reply loops, Odoo automatically blocks emails
                coming to your gateway past a threshold of <b>{loopThreshold}</b> emails every <b>{loopMinutes}</b>
                minutes. If there are some addresses from which you need to receive very frequent
                updates, you can however add them below and Odoo will let them go through.
            </p>
        ");
    }
}
