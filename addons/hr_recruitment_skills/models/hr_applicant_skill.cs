csharp
public partial class ApplicantSkill
{
    public override string ToString()
    {
        return Skill?.ToString() ?? string.Empty;
    }

    public void ComputeSkill()
    {
        if (Skill?.SkillType != SkillType)
        {
            Skill = null;
        }
    }

    public void ComputeSkillLevel()
    {
        if (Skill == null)
        {
            SkillLevel = null;
        }
        else
        {
            var skillLevels = SkillType.SkillLevels;
            SkillLevel = skillLevels.FirstOrDefault(l => l.DefaultLevel) ?? skillLevels.FirstOrDefault();
        }
    }

    public void CheckSkillType()
    {
        if (!SkillType.Skills.Contains(Skill))
        {
            throw new ValidationException($"The skill {Skill.Name} and skill type {SkillType.Name} doesn't match");
        }
    }

    public void CheckSkillLevel()
    {
        if (!SkillType.SkillLevels.Contains(SkillLevel))
        {
            throw new ValidationException($"The skill level {SkillLevel.Name} is not valid for skill type: {SkillType.Name}");
        }
    }
}
