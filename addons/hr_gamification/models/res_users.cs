csharp
public partial class ResUsers
{
    public override string ToString()
    {
        // Assuming there's a Name field in the base ResUsers model
        return Name;
    }

    public IEnumerable<Gamification.Goal> GetGoals()
    {
        return Env.Set<Gamification.Goal>().Where(g => g.UserId == this.Id);
    }

    public IEnumerable<Gamification.BadgeUser> GetBadges()
    {
        return Env.Set<Gamification.BadgeUser>().Where(b => b.UserId == this.Id);
    }
}
