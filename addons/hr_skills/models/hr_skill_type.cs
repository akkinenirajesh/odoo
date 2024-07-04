csharp
using System;

namespace HrSkills
{
    public partial class SkillType
    {
        public override string ToString()
        {
            return Name;
        }

        private int GetDefaultColor()
        {
            return new Random().Next(1, 12);
        }

        public SkillType()
        {
            Color = GetDefaultColor();
        }
    }
}
