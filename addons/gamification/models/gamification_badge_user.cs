csharp
public partial class BadgeUser
{
    public override string ToString()
    {
        return BadgeName;
    }

    public bool SendBadge()
    {
        var template = Env.Ref("Gamification.EmailTemplateBadgeReceived");
        if (template == null)
        {
            return false;
        }

        template.SendMail(this.Id);
        return true;
    }

    public static BadgeUser Create(BadgeUserCreateDto dto)
    {
        Env.Get<Gamification.Badge>(dto.BadgeId).CheckGranting();
        // Assuming there's a base Create method that handles the actual creation
        return base.Create(dto);
    }
}

public class BadgeUserCreateDto
{
    public int BadgeId { get; set; }
    // Add other properties as needed
}
