csharp
public partial class Skill
{
    public override string ToString()
    {
        if (!Env.Context.GetValueOrDefault("from_skill_dropdown", false))
        {
            return base.ToString();
        }
        return $"{Name} ({SkillType.Name})";
    }
}
