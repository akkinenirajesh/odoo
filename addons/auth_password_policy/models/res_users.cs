csharp
public partial class ResUsers
{
    public Dictionary<string, object> GetPasswordPolicy()
    {
        var parameters = Env.Get<IrConfigParameter>().Sudo();
        return new Dictionary<string, object>
        {
            ["MinLength"] = int.Parse(parameters.GetParam("auth_password_policy.minlength", "0"))
        };
    }

    public void SetPassword()
    {
        CheckPasswordPolicy(this.Password);
        base.SetPassword();
    }

    private void CheckPasswordPolicy(string password)
    {
        var failures = new List<string>();
        var parameters = Env.Get<IrConfigParameter>().Sudo();

        int minLength = int.Parse(parameters.GetParam("auth_password_policy.minlength", "0"));

        if (!string.IsNullOrEmpty(password) && password.Length < minLength)
        {
            failures.Add($"Your password must contain at least {minLength} characters and only has {password.Length}.");
        }

        if (failures.Any())
        {
            throw new UserException(string.Join("\n\n", failures));
        }
    }
}
