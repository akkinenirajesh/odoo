csharp
public partial class WebsiteSlidesSurvey.SurveyUserInput {
    public WebsiteSlidesSurvey.SurveyUserInput Create(Dictionary<string, object> vals)
    {
        WebsiteSlidesSurvey.SurveyUserInput record = Env.Create<WebsiteSlidesSurvey.SurveyUserInput>(vals);
        record.CheckForFailedAttempt();
        return record;
    }
    public void Write(Dictionary<string, object> vals)
    {
        Env.Write(this, vals);
        if (vals.ContainsKey("State"))
        {
            CheckForFailedAttempt();
        }
    }
    public void CheckForFailedAttempt()
    {
        if (this.State == "done" && !this.ScoringSuccess && this.SlidePartnerId != null)
        {
            if (Env.Ref("website_slides_survey.mail_template_user_input_certification_failed").SendMail(this.Id, "mail.mail_notification_light"))
            {
                return;
            }

            if (!Env.Ref("website_slides_survey.mail_template_user_input_certification_failed").SendMail(this.Id, "mail.mail_notification_light"))
            {
                return;
            }
            var removedMembershipsPerPartner = new Dictionary<Res.Partner, Slide.Channel>();
            var userInputs = Env.Search<WebsiteSlidesSurvey.SurveyUserInput>(new List<Tuple<string, object>>{
                new Tuple<string, object>("Id", this.Id),
                new Tuple<string, object>("State", "done"),
                new Tuple<string, object>("ScoringSuccess", false),
                new Tuple<string, object>("SlidePartnerId", this.SlidePartnerId)
            });
            foreach (WebsiteSlidesSurvey.SurveyUserInput userInput in userInputs)
            {
                if (userInput.SurveyId.HasAttemptsLeft(userInput.PartnerId, userInput.Email, userInput.InviteToken))
                {
                    continue;
                }
                var removedMemberships = removedMembershipsPerPartner.ContainsKey(userInput.PartnerId) ? removedMembershipsPerPartner[userInput.PartnerId] : Env.Get<Slide.Channel>();
                removedMemberships |= userInput.SlidePartnerId.ChannelId;
                removedMembershipsPerPartner[userInput.PartnerId] = removedMemberships;
            }
            foreach (var pair in removedMembershipsPerPartner)
            {
                pair.Value.RemoveMembership(new List<int> { pair.Key.Id });
            }
        }
    }
}
