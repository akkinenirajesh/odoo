csharp
public partial class Badge
{
    public const int CAN_GRANT = 1;
    public const int NOBODY_CAN_GRANT = 2;
    public const int USER_NOT_VIP = 3;
    public const int BADGE_REQUIRED = 4;
    public const int TOO_MANY = 5;

    public int GrantedCount => OwnersInfo.GrantedCount;
    public int GrantedUsersCount => OwnersInfo.GrantedUsersCount;
    public List<Core.User> UniqueOwnerIds => OwnersInfo.UniqueOwnerIds;

    public int StatThisMonth => BadgeUserStats.StatThisMonth;
    public int StatMy => BadgeUserStats.StatMy;
    public int StatMyThisMonth => BadgeUserStats.StatMyThisMonth;
    public int StatMyMonthlySending => BadgeUserStats.StatMyMonthlySending;

    public int RemainingSending => CalculateRemainingSending();

    private OwnersInfo OwnersInfo => GetOwnersInfo();
    private BadgeUserStats BadgeUserStats => GetBadgeUserStats();

    public bool CheckGranting()
    {
        int statusCode = CanGrantBadge();
        switch (statusCode)
        {
            case CAN_GRANT:
                return true;
            case NOBODY_CAN_GRANT:
                throw new UserException("This badge cannot be sent by users.");
            case USER_NOT_VIP:
                throw new UserException("You are not in the user allowed list.");
            case BADGE_REQUIRED:
                throw new UserException("You do not have the required badges.");
            case TOO_MANY:
                throw new UserException("You have already sent this badge too many times this month.");
            default:
                Env.Logger.LogError($"Unknown badge status code: {statusCode}");
                return false;
        }
    }

    private int CanGrantBadge()
    {
        if (Env.IsAdmin())
            return CAN_GRANT;

        if (RuleAuth == BadgeRuleAuth.Nobody)
            return NOBODY_CAN_GRANT;
        else if (RuleAuth == BadgeRuleAuth.Users && !RuleAuthUserIds.Contains(Env.User))
            return USER_NOT_VIP;
        else if (RuleAuth == BadgeRuleAuth.Having)
        {
            var allUserBadges = Env.Set<BadgeUser>().Search(u => u.User == Env.User).Select(bu => bu.Badge).ToList();
            if (RuleAuthBadgeIds.Except(allUserBadges).Any())
                return BADGE_REQUIRED;
        }

        if (RuleMax && StatMyMonthlySending >= RuleMaxNumber)
            return TOO_MANY;

        return CAN_GRANT;
    }

    private int CalculateRemainingSending()
    {
        if (CanGrantBadge() != CAN_GRANT)
            return 0;
        if (!RuleMax)
            return -1;
        return RuleMaxNumber - StatMyMonthlySending;
    }

    private OwnersInfo GetOwnersInfo()
    {
        // Implementation to calculate owners info
        // This would replace the _get_owners_info method from the original Python code
        throw new NotImplementedException();
    }

    private BadgeUserStats GetBadgeUserStats()
    {
        // Implementation to calculate badge user stats
        // This would replace the _get_badge_user_stats method from the original Python code
        throw new NotImplementedException();
    }
}

public class OwnersInfo
{
    public int GrantedCount { get; set; }
    public int GrantedUsersCount { get; set; }
    public List<Core.User> UniqueOwnerIds { get; set; }
}

public class BadgeUserStats
{
    public int StatThisMonth { get; set; }
    public int StatMy { get; set; }
    public int StatMyThisMonth { get; set; }
    public int StatMyMonthlySending { get; set; }
}
