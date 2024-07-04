csharp
public partial class Challenge 
{
    public override string ToString()
    {
        return Name;
    }

    public void ActionStart()
    {
        State = ChallengeState.InProgress;
    }

    public void ActionCheck()
    {
        Env.Set("gamification.goal").Search(new object[] {
            new object[] { "challenge_id", "=", this.Id },
            new object[] { "state", "=", "inprogress" }
        }).Unlink();

        UpdateAll();
    }

    public void ActionReportProgress()
    {
        ReportProgress();
    }

    public Dictionary<string, object> ActionViewUsers()
    {
        var action = Env.Ref("base.action_res_users");
        action["domain"] = new object[] { new object[] { "id", "in", Users.Select(u => u.Id).ToArray() } };
        return action;
    }

    public void AcceptChallenge()
    {
        var user = Env.User;
        MessagePost(body: $"{user.Name} has joined the challenge");
        InvitedUsers = InvitedUsers.Where(u => u.Id != user.Id).ToList();
        Users.Add(user);
        GenerateGoalsFromChallenge();
    }

    public void DiscardChallenge()
    {
        var user = Env.User;
        MessagePost(body: $"{user.Name} has refused the challenge");
        InvitedUsers = InvitedUsers.Where(u => u.Id != user.Id).ToList();
    }

    private void RewardUser(Core.User user, Gamification.Badge badge)
    {
        Env.Set("gamification.badge.user").Create(new Dictionary<string, object>
        {
            { "UserId", user.Id },
            { "BadgeId", badge.Id },
            { "ChallengeId", this.Id }
        }).SendBadge();
    }

    // ... Other methods would be implemented similarly
}
