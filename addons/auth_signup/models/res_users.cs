csharp
public partial class ResUsers
{
    public string ComputeState()
    {
        return LoginDate.HasValue ? "Active" : "New";
    }

    public IEnumerable<int> SearchState(string op, object value)
    {
        // Implement the search logic based on the operator and value
        // Return a list of matching user IDs
        throw new NotImplementedException();
    }

    public (string Login, string Password) Signup(Dictionary<string, object> values, string token = null)
    {
        // Implement signup logic
        throw new NotImplementedException();
    }

    public string GetSignupInvitationScope()
    {
        return Env.Config.GetParam("auth_signup.invitation_scope", "b2b");
    }

    public ResUsers SignupCreateUser(Dictionary<string, object> values)
    {
        // Implement user creation logic
        throw new NotImplementedException();
    }

    public static int Authenticate(string db, string login, string password, Dictionary<string, string> userAgentEnv)
    {
        // Implement authentication logic
        throw new NotImplementedException();
    }

    public void NotifyInviter()
    {
        // Implement inviter notification logic
        throw new NotImplementedException();
    }

    public ResUsers CreateUserFromTemplate(Dictionary<string, object> values)
    {
        // Implement user creation from template logic
        throw new NotImplementedException();
    }

    public void ResetPassword(string login)
    {
        // Implement password reset logic
        throw new NotImplementedException();
    }

    public void ActionResetPassword()
    {
        // Implement password reset action logic
        throw new NotImplementedException();
    }

    public void SendUnregisteredUserReminder(int afterDays = 5, int batchSize = 100)
    {
        // Implement unregistered user reminder logic
        throw new NotImplementedException();
    }

    public void AlertNewDevice()
    {
        // Implement new device alert logic
        throw new NotImplementedException();
    }

    public Dictionary<string, object> PrepareNewDeviceNoticeValues()
    {
        // Implement preparation of new device notice values
        throw new NotImplementedException();
    }

    public static IEnumerable<ResUsers> WebCreateUsers(IEnumerable<string> emails)
    {
        // Implement web user creation logic
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Name;
    }
}
