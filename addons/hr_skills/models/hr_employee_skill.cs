csharp
public partial class EmployeeSkill
{
    public override string ToString()
    {
        return $"{Skill.Name}: {SkillLevel.Name}";
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

    public void ComputeSkill()
    {
        if (SkillType != null)
        {
            Skill = SkillType.Skills.FirstOrDefault();
        }
        else
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

    public void CreateLogs()
    {
        var today = DateTime.Today;
        var employeeSkills = Env.Find<Hr.EmployeeSkill>(s => s.Employee == Employee);
        var employeeSkillLogs = Env.Find<Hr.EmployeeSkillLog>(l => l.Employee == Employee);

        var skillsToCreate = new List<Hr.EmployeeSkillLog>();

        foreach (var employeeSkill in employeeSkills)
        {
            var existingLog = employeeSkillLogs.FirstOrDefault(l => 
                l.Department == employeeSkill.Employee.Department && 
                l.Skill == employeeSkill.Skill && 
                l.Date == today);

            if (existingLog != null)
            {
                existingLog.SkillLevel = employeeSkill.SkillLevel;
            }
            else
            {
                skillsToCreate.Add(new Hr.EmployeeSkillLog
                {
                    Employee = employeeSkill.Employee,
                    Skill = employeeSkill.Skill,
                    SkillLevel = employeeSkill.SkillLevel,
                    Department = employeeSkill.Employee.Department,
                    SkillType = employeeSkill.SkillType,
                    Date = today
                });
            }
        }

        if (skillsToCreate.Any())
        {
            Env.Create(skillsToCreate);
        }
    }
}
