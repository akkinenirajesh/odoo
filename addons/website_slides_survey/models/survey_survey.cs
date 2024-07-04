csharp
public partial class Survey {
    public Survey(Buvi.Env env) : base(env) {}

    public void ComputeSlideChannelData() {
        foreach (var survey in this) {
            survey.SlideChannelIds = survey.SlideIds.Select(slide => slide.ChannelId).ToList();
            survey.SlideChannelCount = survey.SlideChannelIds.Count;
        }
    }

    public void UnlinkExceptLinkedToCourse() {
        var certifications = Env.Model<Slide.Slide>().Search(slide => slide.SurveyId == this.Id && slide.SlideType == "certification").Select(slide => slide.SurveyId).Where(survey => survey.Exists).ToList();
        if (certifications.Any()) {
            var certificationsCourseMapping = certifications.Select(certi => {
                var certificationTitle = certi.Title;
                var courses = certi.SlideChannelIds.Select(channel => channel.Name).ToList();
                return $"{certificationTitle} (Courses - {courses.StringJoin(",")})";
            });
            throw new Buvi.ValidationError($"Any Survey listed below is currently used as a Course Certification and cannot be deleted:\n{certificationsCourseMapping.StringJoin("\n")}");
        }
    }

    public Buvi.IAction ActionSurveyViewSlideChannels() {
        var action = Env.Model<Ir.Actions.Actions>().ForXmlId("website_slides.slide_channel_action_overview");
        action.DisplayName = "Courses";
        if (SlideChannelCount == 1) {
            action.Views = new List<Buvi.View> { new Buvi.View(false, "form") };
            action.ResId = SlideChannelIds[0].Id;
        } else {
            action.Views = new List<Buvi.View> { new Buvi.View(false, "tree"), new Buvi.View(false, "form") };
            action.Domain = new Buvi.Domain {
                new Buvi.DomainExpression("id", "in", SlideChannelIds.Select(id => id.Id).ToList())
            };
        }
        action.Context = new Buvi.Context {
            Create = false
        };
        return action;
    }

    public string PrepareChallengeCategory() {
        var slideSurvey = Env.Model<Slide.Slide>().Search(slide => slide.SurveyId == this.Id);
        return slideSurvey.Any() ? "slides" : "certification";
    }
}
