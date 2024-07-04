csharp
public partial class Digest
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionSubscribe()
    {
        if (Env.User.IsInternal() && !Users.Contains(Env.User))
        {
            ActionSubscribeUsers(new[] { Env.User });
        }
    }

    private void ActionSubscribeUsers(User[] users)
    {
        Users = Users.Concat(users).ToArray();
    }

    public void ActionUnsubscribe()
    {
        if (Env.User.IsInternal() && Users.Contains(Env.User))
        {
            ActionUnsubscribeUsers(new[] { Env.User });
        }
    }

    private void ActionUnsubscribeUsers(User[] users)
    {
        Users = Users.Except(users).ToArray();
    }

    public void ActionActivate()
    {
        State = DigestState.Activated;
    }

    public void ActionDeactivate()
    {
        State = DigestState.Deactivated;
    }

    public void ActionSetPeriodicity(DigestPeriodicity periodicity)
    {
        Periodicity = periodicity;
    }

    public void ActionSend()
    {
        ActionSendInternal(true);
    }

    public void ActionSendManual()
    {
        ActionSendInternal(false);
    }

    private void ActionSendInternal(bool updatePeriodicity)
    {
        // Implementation of sending digest emails
        // This would involve more complex logic, including checking daily logs,
        // computing KPIs, and sending emails to users
    }

    // Other methods like ComputeKpis, ComputeTips, etc. would be implemented here
}
