csharp
public partial class SlideChannelPartner
{
    public void RecomputeCompletion()
    {
        // Implementation of _recompute_completion method
        var partnerHasCompleted = new Dictionary<long, Channel>();
        if (MemberStatus == "completed")
        {
            partnerHasCompleted[PartnerId.Id] = ChannelId;
        }

        var employees = Env.Find<Employee>().Search(e => partnerHasCompleted.ContainsKey(e.UserId.PartnerId.Id));

        if (employees.Any())
        {
            var hrResumeLine = Env.Find<ResumeLine>();
            var lineType = Env.Ref<ResumeLineType>("hr_skills_slides.resume_type_training");
            var lineTypeId = lineType?.Id;

            foreach (var employee in employees)
            {
                var channel = partnerHasCompleted[employee.UserId.PartnerId.Id];

                var alreadyAdded = hrResumeLine.Search(l =>
                    l.EmployeeId.Id == employee.Id &&
                    l.ChannelId.Id == channel.Id &&
                    l.LineTypeId.Id == lineTypeId &&
                    l.DisplayType == "course"
                );

                if (!alreadyAdded.Any())
                {
                    hrResumeLine.Create(new ResumeLine
                    {
                        EmployeeId = employee,
                        Name = channel.Name,
                        DateStart = DateTime.Today,
                        DateEnd = DateTime.Today,
                        Description = Html2Plaintext(channel.Description),
                        LineTypeId = lineType,
                        DisplayType = "course",
                        ChannelId = channel
                    });
                }
            }
        }
    }

    public void SendCompletedMail()
    {
        // Implementation of _send_completed_mail method
        if (Env.User.EmployeeIds.Any())
        {
            var msg = $"The employee has completed the course <a href=\"{ChannelId.WebsiteUrl}\">{ChannelId.Name}</a>";
            Env.User.EmployeeId.MessagePost(body: msg);
        }
    }
}

public partial class Channel
{
    public void ActionAddMembers(IEnumerable<Partner> targetPartners, string memberStatus = "joined", bool raiseOnAccess = false)
    {
        // Implementation of _action_add_members method
        if (memberStatus == "joined")
        {
            var msg = $"The employee subscribed to the course <a href=\"{WebsiteUrl}\">{Name}</a>";
            MessageEmployeeChatter(msg, targetPartners);
        }
    }

    public void RemoveMembership(IEnumerable<long> partnerIds)
    {
        // Implementation of _remove_membership method
        var partners = Env.Find<Partner>().Browse(partnerIds);
        var msg = $"The employee left the course <a href=\"{WebsiteUrl}\">{Name}</a>";
        MessageEmployeeChatter(msg, partners);
    }

    private void MessageEmployeeChatter(string msg, IEnumerable<Partner> partners)
    {
        foreach (var partner in partners)
        {
            var employee = partner.UserIds
                .Where(u => u.EmployeeId != null && (!partner.CompanyId.HasValue || u.EmployeeId.CompanyId == partner.CompanyId))
                .Select(u => u.EmployeeId)
                .FirstOrDefault();

            if (employee != null)
            {
                employee.MessagePost(body: msg);
            }
        }
    }
}
