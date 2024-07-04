csharp
public partial class WebsiteSlidesSurvey.SlidePartnerRelation {
    public bool SurveyScoringSuccess { get; set; }

    public void ComputeSurveyScoringSuccess() {
        var succeededUserInputs = Env.Search<WebsiteSlidesSurvey.SurveyUserInput>(x => x.SlidePartnerId == this.Id && x.ScoringSuccess);
        var succeededSlidePartners = succeededUserInputs.Select(x => x.SlidePartnerId);
        this.SurveyScoringSuccess = succeededSlidePartners.Contains(this.Id);
    }

    public void ComputeFieldValue(string fieldName) {
        if (fieldName == "SurveyScoringSuccess") {
            if (this.SurveyScoringSuccess) {
                this.Completed = true;
            }
        }
    }

    public void RecomputeCompletion() {
        if (this.SurveyScoringSuccess) {
            var certifiedChannelsDomain = Env.CreateDomain(
                new[] {
                    Env.CreateDomain(new [] {("PartnerId", "=", this.PartnerId), ("ChannelId", "=", this.ChannelId)}),
                }
            );
            var certifiedChannels = Env.Search<WebsiteSlidesSurvey.SlideChannelPartner>(x => !x.SurveyCertificationSuccess && certifiedChannelsDomain);
            certifiedChannels.SurveyCertificationSuccess = true;
        }
    }
}

public partial class WebsiteSlidesSurvey.Slide {
    public string Name { get; set; }
    public string SlideCategory { get; set; }
    public string SlideType { get; set; }
    public WebsiteSlidesSurvey.SurveySurvey SurveyId { get; set; }
    public int NbrCertification { get; set; }
    public bool IsPreview { get; set; }

    public void ComputeName() {
        if (string.IsNullOrEmpty(this.Name) && this.SurveyId != null) {
            this.Name = this.SurveyId.Title;
        }
    }

    public void ComputeMarkCompleteActions() {
        if (this.SlideCategory == "certification") {
            this.CanSelfMarkUncompleted = false;
            this.CanSelfMarkCompleted = false;
        }
        else {
            // Call super method
        }
    }

    public void ComputeIsPreview() {
        if (this.SlideCategory == "certification" || !this.IsPreview) {
            this.IsPreview = false;
        }
    }

    public void ComputeSlideIconClass() {
        if (this.SlideType == "certification") {
            this.SlideIconClass = "fa-trophy";
        }
        else {
            // Call super method
        }
    }

    public void ComputeSlideType() {
        if (this.SlideCategory == "certification") {
            this.SlideType = "certification";
        }
    }

    public void Create(Dictionary<string, object> values) {
        var slide = this;
        // Call super method
        if (values.ContainsKey("SurveyId") && values["SurveyId"] != null) {
            slide.SlideCategory = "certification";
            slide.EnsureChallengeCategory();
        }
    }

    public void Write(Dictionary<string, object> values) {
        // Call super method
        if (values.ContainsKey("SurveyId") && values["SurveyId"] != null) {
            var oldSurveys = Env.Search<WebsiteSlidesSurvey.SurveySurvey>(x => x.Id == this.SurveyId).ToList();
            slide.EnsureChallengeCategory(oldSurveys);
        }
    }

    public void Unlink() {
        // Call super method
        var oldSurveys = Env.Search<WebsiteSlidesSurvey.SurveySurvey>(x => x.Id == this.SurveyId).ToList();
        slide.EnsureChallengeCategory(oldSurveys, true);
    }

    public void EnsureChallengeCategory(List<WebsiteSlidesSurvey.SurveySurvey> oldSurveys = null, bool unlink = false) {
        if (oldSurveys != null) {
            var oldCertificationChallenges = oldSurveys.Select(x => x.CertificationBadgeId).SelectMany(x => x.ChallengeIds).ToList();
            oldCertificationChallenges.ForEach(x => x.ChallengeCategory = "certification");
        }
        if (!unlink) {
            var certificationChallenges = this.SurveyId.CertificationBadgeId.ChallengeIds.ToList();
            certificationChallenges.ForEach(x => x.ChallengeCategory = "slides");
        }
    }

    public Dictionary<int, string> GenerateCertificationUrl() {
        var certificationUrls = new Dictionary<int, string>();
        foreach (var slide in Env.Search<WebsiteSlidesSurvey.Slide>(x => x.SlideCategory == "certification" && x.SurveyId != null).ToList()) {
            if (slide.ChannelId.IsMember) {
                var userMembershipIdSudo = Env.Search<WebsiteSlidesSurvey.SlideChannelPartner>(x => x.Id == slide.UserMembershipId).FirstOrDefault();
                if (userMembershipIdSudo.UserInputIds.Any()) {
                    var lastUserInput = userMembershipIdSudo.UserInputIds.OrderByDescending(x => x.CreateDate).FirstOrDefault();
                    certificationUrls[slide.Id] = lastUserInput.GetStartUrl();
                }
                else {
                    var userMembershipId = Env.Search<WebsiteSlidesSurvey.SlideChannelPartner>(x => x.Id == slide.UserMembershipId).FirstOrDefault();
                    var userinput = slide.SurveyId.CreateAnswer(
                        partner: Env.User.PartnerId,
                        checkAttempts: false,
                        slideId: slide.Id,
                        slidePartnerId: userMembershipId.Id,
                        inviteToken: Env.Create<WebsiteSlidesSurvey.SurveyUserInput>().GenerateInviteToken()
                    );
                    certificationUrls[slide.Id] = userinput.GetStartUrl();
                }
            }
            else {
                var userinput = slide.SurveyId.CreateAnswer(
                    partner: Env.User.PartnerId,
                    checkAttempts: false,
                    testEntry: true,
                    slideId: slide.Id
                );
                certificationUrls[slide.Id] = userinput.GetStartUrl();
            }
        }
        return certificationUrls;
    }
}
