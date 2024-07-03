csharp
public partial class IrConfigParameter
{
    public void Init(bool force = false)
    {
        base.Init(force);
        if (force)
        {
            var oauthOe = Env.Ref("auth_oauth.provider_openerp") as AuthProvider;
            if (oauthOe == null)
            {
                return;
            }
            var dbuuid = Env.Sudo().GetParam("database.uuid");
            oauthOe.Write(new { ClientId = dbuuid });
        }
    }

    public override string ToString()
    {
        return $"{Key}: {Value}";
    }
}
