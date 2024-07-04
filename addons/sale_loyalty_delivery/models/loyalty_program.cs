C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaleLoyaltyDelivery
{
    public partial class LoyaltyProgram
    {
        public virtual decimal OrderMinimum { get; set; }
        public virtual int PointsValidityDays { get; set; }
        public virtual int PointsEarned { get; set; }
        public virtual int PointsSpent { get; set; }
        public virtual int PointsUsed { get; set; }
        public virtual int PointsExpired { get; set; }
        public virtual int PointsBalance { get; set; }
        public virtual DateTime ProgramStartDate { get; set; }
        public virtual DateTime ProgramEndDate { get; set; }
        public virtual int ApplicableOrders { get; set; }
        public virtual int PointsEarnedOrders { get; set; }
        public virtual int PointsSpentOrders { get; set; }
        public virtual int PointsUsedOrders { get; set; }
        public virtual int PointsExpiredOrders { get; set; }
        public virtual int PointsBalanceOrders { get; set; }

        public virtual List<LoyaltyReward> RewardIds { get; set; }

        public virtual List<LoyaltyProgramType> ProgramType { get; set; }

        public virtual string Name { get; set; }

        public virtual bool Active { get; set; }

        public virtual List<object> GetProgramTemplates()
        {
            var res = Env.Call("SaleLoyaltyDelivery.LoyaltyProgram", "_get_program_templates");
            if (res.ContainsKey("promotion"))
            {
                res["promotion"]["description"] = Env.Translate("Automatic promotion: free shipping on orders higher than $50");
            }
            return res;
        }

        public virtual List<object> _GetTemplateValues()
        {
            var res = Env.Call("SaleLoyaltyDelivery.LoyaltyProgram", "_get_template_values");
            if (res.ContainsKey("promotion"))
            {
                res["promotion"]["reward_ids"] = new List<object> { (5, 0, 0), (0, 0, new { reward_type = "shipping" }) };
            }
            return res;
        }

        public virtual List<object> _ProgramTypeDefaultValues()
        {
            var res = Env.Call("SaleLoyaltyDelivery.LoyaltyProgram", "_program_type_default_values");
            if (res.ContainsKey("loyalty"))
            {
                res["loyalty"]["reward_ids"].Add(new { reward_type = "shipping", required_points = 100 });
            }
            return res;
        }
    }
}
