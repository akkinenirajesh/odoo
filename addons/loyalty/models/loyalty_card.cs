C#
public partial class LoyaltyCard 
{
    public virtual string GenerateCode()
    {
        // Implement logic to generate a unique code using UUID or other methods.
        return "044" + System.Guid.NewGuid().ToString()[7..-18];
    }

    public virtual void ComputePointsDisplay()
    {
        // Implement logic to format points based on the point_name and currency_id.
        this.PointsDisplay = this.FormatPoints(this.Points);
    }

    private string FormatPoints(double points)
    {
        if (this.PointName == Env.Get("Loyalty.LoyaltyProgram", this.ProgramId).Get("CurrencyId").Get("Symbol"))
        {
            return Env.FormatAmount(points, Env.Get("Res.Currency", this.CurrencyId));
        }
        if (points == (int)points)
        {
            return $"{points} {this.PointName ?? string.Empty}";
        }
        return $"{points:F2} {this.PointName ?? string.Empty}";
    }

    public virtual void ComputeUseCount()
    {
        // Implement logic to calculate the use count based on associated orders or other data.
        this.UseCount = 0;
    }

    public virtual bool HasSourceOrder()
    {
        // Implement logic to check if the loyalty card is associated with a source order.
        return false;
    }

    public virtual void SendCreationCommunication()
    {
        // Implement logic to send email communication when a loyalty card is created.
        if (Env.Context.Get("loyalty_no_mail") || Env.Context.Get("action_no_send_mail"))
        {
            return;
        }
        var createCommunications = Env.Get("Loyalty.LoyaltyProgram", this.ProgramId).Get("CommunicationPlanIds").Filter(c => c.Get("Trigger") == "create");
        if (createCommunications.Count > 0 && this.PartnerId != null)
        {
            foreach (var comm in createCommunications)
            {
                comm.Get("MailTemplateId").SendMail(this.Id);
            }
        }
    }

    public virtual void SendPointsReachCommunication(double oldPoints, double newPoints)
    {
        // Implement logic to send email communication when the points on a loyalty card reach a specific milestone.
        if (Env.Context.Get("loyalty_no_mail"))
        {
            return;
        }
        var milestones = Env.Get("Loyalty.LoyaltyProgram", this.ProgramId).Get("CommunicationPlanIds").Filter(c => c.Get("Trigger") == "points_reach").OrderByDescending(c => c.Get("Points"));
        if (milestones.Count > 0 && this.PartnerId != null && oldPoints < newPoints)
        {
            var thisMilestone = milestones.FirstOrDefault(m => oldPoints < m.Get("Points") && m.Get("Points") <= newPoints);
            if (thisMilestone != null)
            {
                thisMilestone.Get("MailTemplateId").SendMail(this.Id);
            }
        }
    }

    public virtual void ContrainsCode()
    {
        // Implement validation logic to ensure that the loyalty card code is not the same as any existing loyalty rule code.
        if (Env.Get("Loyalty.LoyaltyRule").SearchCount(c => c.Get("Mode") == "with_code" && c.Get("Code") == this.Code) > 0)
        {
            throw new Exception("A trigger with the same code as one of your coupon already exists.");
        }
    }
}
