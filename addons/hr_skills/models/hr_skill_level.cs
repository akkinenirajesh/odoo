csharp
public partial class SkillLevel
{
    public override string ToString()
    {
        if (!Env.Context.GetValueOrDefault("from_skill_level_dropdown", false))
        {
            return base.ToString();
        }
        return $"{Name} ({LevelProgress}%)";
    }

    public override void OnCreate()
    {
        base.OnCreate();
        if (DefaultLevel)
        {
            UpdateDefaultLevel();
        }
    }

    public override void OnWrite()
    {
        base.OnWrite();
        if (DefaultLevel)
        {
            UpdateDefaultLevel();
        }
    }

    private void UpdateDefaultLevel()
    {
        var otherLevels = Env.Set<SkillLevel>()
            .Where(r => r.SkillTypeId == this.SkillTypeId && r.Id != this.Id);
        foreach (var level in otherLevels)
        {
            level.DefaultLevel = false;
        }
    }
}
