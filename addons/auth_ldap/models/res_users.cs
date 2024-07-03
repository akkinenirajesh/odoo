csharp
public partial class ResUsers
{
    public static int? Login(string db, string login, string password, Dictionary<string, string> userAgentEnv)
    {
        try
        {
            // Call base implementation (assuming it's implemented elsewhere)
            return base.Login(db, login, password, userAgentEnv);
        }
        catch (AccessDeniedException e)
        {
            using (var cr = Registry.GetCursor(db))
            {
                cr.Execute("SELECT id FROM res_users WHERE lower(login)=@login", new { login = login.ToLower() });
                var res = cr.FetchOne();
                if (res != null)
                {
                    throw e;
                }

                var env = new Environment(cr, Constants.SUPERUSER_ID, new Dictionary<string, object>());
                var ldap = env.Get<ResCompanyLdap>();
                foreach (var conf in ldap.GetLdapDicts())
                {
                    var entry = ldap.Authenticate(conf, login, password);
                    if (entry != null)
                    {
                        return ldap.GetOrCreateUser(conf, login, entry);
                    }
                }
                throw e;
            }
        }
    }

    public void CheckCredentials(string password, Environment env)
    {
        try
        {
            // Call base implementation (assuming it's implemented elsewhere)
            base.CheckCredentials(password, env);
        }
        catch (AccessDeniedException)
        {
            bool passwdAllowed = env.Context.ContainsKey("interactive") || !this.RpcApiKeysOnly();
            if (passwdAllowed && this.Active)
            {
                var ldap = env.Get<ResCompanyLdap>();
                foreach (var conf in ldap.GetLdapDicts())
                {
                    if (ldap.Authenticate(conf, this.Login, password))
                    {
                        return;
                    }
                }
            }
            throw new AccessDeniedException();
        }
    }

    public bool ChangePassword(string oldPasswd, string newPasswd)
    {
        if (!string.IsNullOrEmpty(newPasswd))
        {
            var ldap = Env.Get<ResCompanyLdap>();
            foreach (var conf in ldap.GetLdapDicts())
            {
                bool changed = ldap.ChangePassword(conf, this.Login, oldPasswd, newPasswd);
                if (changed)
                {
                    this.SetEmptyPassword();
                    return true;
                }
            }
        }
        // Call base implementation (assuming it's implemented elsewhere)
        return base.ChangePassword(oldPasswd, newPasswd);
    }

    private void SetEmptyPassword()
    {
        // Assuming we have a way to flush and invalidate recordsets
        this.FlushRecordset(new[] { "Password" });
        Env.Cr.Execute("UPDATE res_users SET password=NULL WHERE id=@id", new { id = this.Id });
        this.InvalidateRecordset(new[] { "Password" });
    }

    private bool RpcApiKeysOnly()
    {
        // Implementation needed
        throw new NotImplementedException();
    }
}
