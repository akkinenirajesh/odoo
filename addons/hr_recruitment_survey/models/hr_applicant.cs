csharp
public partial class Applicant
{
    public void ActionPrintSurvey()
    {
        var sortedInterviews = ResponseIds
            .Where(i => i.SurveyId == this.SurveyId)
            .OrderByDescending(i => i.CreateDate)
            .ToList();

        if (sortedInterviews.Count == 0)
        {
            var action = this.SurveyId.ActionPrintSurvey();
            action["target"] = "new";
            return action;
        }

        var answeredInterviews = sortedInterviews.Where(i => i.State == "done").ToList();
        if (answeredInterviews.Any())
        {
            var action = this.SurveyId.ActionPrintSurvey(answer: answeredInterviews[0]);
            action["target"] = "new";
            return action;
        }

        var action = this.SurveyId.ActionPrintSurvey(answer: sortedInterviews[0]);
        action["target"] = "new";
        return action;
    }

    public void ActionSendSurvey()
    {
        if (this.PartnerId == null)
        {
            if (string.IsNullOrEmpty(this.PartnerName))
            {
                throw new UserException("Please provide an applicant name.");
            }

            this.PartnerId = Env.Get<ResPartner>().Sudo().Create(new Dictionary<string, object>
            {
                ["IsCompany"] = false,
                ["Name"] = this.PartnerName,
                ["Email"] = this.EmailFrom,
                ["Phone"] = this.PartnerPhone,
                ["Mobile"] = this.PartnerMobile
            });
        }

        this.SurveyId.CheckValidity();
        var template = Env.Ref("hr_recruitment_survey.mail_template_applicant_interview_invite", raiseIfNotFound: false);

        var localContext = new Dictionary<string, object>
        {
            ["default_applicant_id"] = this.Id,
            ["default_partner_ids"] = new[] { this.PartnerId.Id },
            ["default_survey_id"] = this.SurveyId.Id,
            ["default_use_template"] = template != null,
            ["default_template_id"] = template?.Id ?? false,
            ["default_email_layout_xmlid"] = "mail.mail_notification_light",
            ["default_deadline"] = DateTime.Now.AddDays(15)
        };

        return new Dictionary<string, object>
        {
            ["type"] = "ir.actions.act_window",
            ["name"] = "Send an interview",
            ["view_mode"] = "form",
            ["res_model"] = "survey.invite",
            ["target"] = "new",
            ["context"] = localContext,
        };
    }
}
