csharp
public partial class ResPartner
{
    public void Init()
    {
        base.Init();
        if (!Env.Cr.ColumnExists(TableName, "SignupToken"))
        {
            Env.Cr.Execute("ALTER TABLE res_partner ADD COLUMN signup_token varchar");
        }
    }

    public bool ComputeSignupValid()
    {
        var dt = DateTime.Now;
        return !string.IsNullOrEmpty(SignupToken) && 
               (!SignupExpiration.HasValue || dt <= SignupExpiration.Value);
    }

    public string ComputeSignupUrl()
    {
        var result = GetSignupUrlForAction();
        if (UserIds.Any(u => u.IsInternal() && u != Env.User))
        {
            Env.Users.CheckAccessRights("write");
        }
        if (UserIds.Any(u => u.IsPortal() && u != Env.User))
        {
            Env.ResPartners.CheckAccessRights("write");
        }
        return result;
    }

    public string ComputeToken()
    {
        return Env.Cr.ExecuteScalar<string>("SELECT signup_token FROM res_partner WHERE id=@Id", new { Id = Id });
    }

    public void InverseToken()
    {
        Env.Cr.Execute("UPDATE res_partner SET signup_token = @Token WHERE id=@Id", new { Token = SignupToken, Id = Id });
    }

    public Dictionary<long, string> GetSignupUrlForAction(string url = null, string action = null, string viewType = null, int? menuId = null, long? resId = null, string model = null)
    {
        // Implementation of _get_signup_url_for_action logic
        // This method would need to be adapted to C# and the specific environment
        throw new NotImplementedException();
    }

    public void ActionSignupPrepare()
    {
        SignupPrepare();
    }

    public Dictionary<long, Dictionary<string, string>> SignupGetAuthParam()
    {
        if (!Env.User.IsInternal() && !Env.IsAdmin())
        {
            throw new AccessDeniedException();
        }

        var res = new Dictionary<long, Dictionary<string, string>>();
        var allowSignup = Env.Users.GetSignupInvitationScope() == "b2c";

        foreach (var partner in this)
        {
            var partnerData = new Dictionary<string, string>();
            if (allowSignup && !partner.UserIds.Any())
            {
                partner.SignupPrepare();
                partnerData["auth_signup_token"] = partner.SignupToken;
            }
            else if (partner.UserIds.Any())
            {
                partnerData["auth_login"] = partner.UserIds.First().Login;
            }
            res[partner.Id] = partnerData;
        }

        return res;
    }

    public void SignupCancel()
    {
        SignupToken = null;
        SignupType = null;
        SignupExpiration = null;
    }

    public void SignupPrepare(string signupType = "signup", DateTime? expiration = null)
    {
        if (expiration.HasValue || !SignupValid)
        {
            string token;
            do
            {
                token = GenerateRandomToken();
            } while (SignupRetrievePartner(token) != null);

            SignupToken = token;
            SignupType = signupType;
            SignupExpiration = expiration;
        }
    }

    public ResPartner SignupRetrievePartner(string token, bool checkValidity = false, bool raiseException = false)
    {
        var partner = Env.ResPartners.FirstOrDefault(p => p.SignupToken == token && p.Active);
        if (partner == null)
        {
            if (raiseException)
            {
                throw new UserException($"Signup token '{token}' is not valid");
            }
            return null;
        }
        if (checkValidity && !partner.SignupValid)
        {
            if (raiseException)
            {
                throw new UserException($"Signup token '{token}' is no longer valid");
            }
            return null;
        }
        return partner;
    }

    public Dictionary<string, object> SignupRetrieveInfo(string token)
    {
        var partner = SignupRetrievePartner(token, raiseException: true);
        var res = new Dictionary<string, object>
        {
            ["db"] = Env.Cr.DbName
        };

        if (partner.SignupValid)
        {
            res["token"] = token;
            res["name"] = partner.Name;
        }

        if (partner.UserIds.Any())
        {
            res["login"] = partner.UserIds.First().Login;
        }
        else
        {
            res["email"] = res["login"] = partner.Email ?? "";
        }

        return res;
    }

    private string GenerateRandomToken()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 20)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
