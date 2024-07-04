csharp
public partial class CalendarEvent
{
    public override string ToString()
    {
        // Implement string representation logic here
        return base.ToString();
    }

    public override IEnumerable<CalendarEvent> Create(IEnumerable<Dictionary<string, object>> valsList)
    {
        var events = base.Create(valsList);

        try
        {
            Env.Get<HrRecruitment.Applicant>().CheckAccessRights("read");
        }
        catch (AccessException)
        {
            return events;
        }

        if (Env.Context.ContainsKey("default_applicant_id"))
        {
            var applicantId = (int)Env.Context["default_applicant_id"];
            var applicantAttachments = Env.Get<HrRecruitment.Applicant>().Browse(applicantId).AttachmentIds;

            foreach (var evnt in events)
            {
                Env.Get<Core.IrAttachment>().Create(applicantAttachments.Select(att => new Dictionary<string, object>
                {
                    ["Name"] = att.Name,
                    ["Type"] = "binary",
                    ["Datas"] = att.Datas,
                    ["ResModel"] = evnt.GetType().Name,
                    ["ResId"] = evnt.Id
                }));
            }
        }

        return events;
    }

    public override Dictionary<string, object> DefaultGet(IEnumerable<string> fields)
    {
        if (Env.Context.ContainsKey("default_applicant_id"))
        {
            Env = Env.WithContext(new Dictionary<string, object>
            {
                ["default_res_model"] = "hr.applicant",
                ["default_res_model_id"] = Env.Ref("hr_recruitment.model_hr_applicant").Id,
                ["default_res_id"] = Env.Context["default_applicant_id"],
                ["default_partner_ids"] = Env.Context["default_partner_ids"],
                ["default_name"] = Env.Context["default_name"]
            });
        }

        var defaults = base.DefaultGet(fields);

        if (!defaults.ContainsKey("ApplicantId"))
        {
            var resModel = defaults.ContainsKey("ResModel") ? (string)defaults["ResModel"] : (string)Env.Context.GetValueOrDefault("default_res_model");
            var resModelId = defaults.ContainsKey("ResModelId") ? (int)defaults["ResModelId"] : (int?)Env.Context.GetValueOrDefault("default_res_model_id");

            if ((resModel != null && resModel == "hr.applicant") || (resModelId != null && Env.Get<Core.IrModel>().Sudo().Browse(resModelId.Value).Model == "hr.applicant"))
            {
                defaults["ApplicantId"] = defaults.ContainsKey("ResId") ? defaults["ResId"] : Env.Context.GetValueOrDefault("default_res_id");
            }
        }

        return defaults;
    }

    public void ComputeIsHighlighted()
    {
        base.ComputeIsHighlighted();

        var applicantId = (int?)Env.Context.GetValueOrDefault("active_id");
        if (Env.Context.GetValueOrDefault("active_model") as string == "hr.applicant" && applicantId.HasValue)
        {
            if (ApplicantId?.Id == applicantId.Value)
            {
                IsHighlighted = true;
            }
        }
    }
}
