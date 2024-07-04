csharp
public partial class WebsiteEventTrackQuiz.EventTrack
{
    public void _ComputeQuizId()
    {
        this.QuizId = this.QuizIds.FirstOrDefault();
    }

    public void _ComputeQuizQuestionsCount()
    {
        this.QuizQuestionsCount = this.QuizId.QuestionIds.Count();
    }

    public void _ComputeQuizData()
    {
        if (this.QuizId == null)
        {
            this.IsQuizCompleted = false;
            this.QuizPoints = 0;
            return;
        }

        var currentVisitor = Env.Website.Visitor._GetVisitorFromRequest();

        if (Env.User._IsPublic() && currentVisitor == null)
        {
            this.IsQuizCompleted = false;
            this.QuizPoints = 0;
            return;
        }

        if (Env.User._IsPublic())
        {
            var domain = new List<string> { $"visitor_id = {currentVisitor.Id}" };

            var eventTrackVisitors = Env.EventTrackVisitor.SearchRead(domain, new List<string> { "track_id", "quiz_completed", "quiz_points" });

            var quizVisitorMap = eventTrackVisitors.ToDictionary(x => x.TrackId[0], x => new { quiz_completed = x.QuizCompleted, quiz_points = x.QuizPoints });

            this.IsQuizCompleted = quizVisitorMap.ContainsKey(this.Id) ? quizVisitorMap[this.Id].quiz_completed : false;
            this.QuizPoints = quizVisitorMap.ContainsKey(this.Id) ? quizVisitorMap[this.Id].quiz_points : 0;
        }
        else
        {
            var domain = new List<string>
            {
                $"partner_id = {Env.User.PartnerId.Id}",
                $"visitor_id = {currentVisitor.Id}"
            };

            var eventTrackVisitors = Env.EventTrackVisitor.SearchRead(domain, new List<string> { "track_id", "quiz_completed", "quiz_points" });

            var quizVisitorMap = eventTrackVisitors.ToDictionary(x => x.TrackId[0], x => new { quiz_completed = x.QuizCompleted, quiz_points = x.QuizPoints });

            this.IsQuizCompleted = quizVisitorMap.ContainsKey(this.Id) ? quizVisitorMap[this.Id].quiz_completed : false;
            this.QuizPoints = quizVisitorMap.ContainsKey(this.Id) ? quizVisitorMap[this.Id].quiz_points : 0;
        }
    }

    public void ActionAddQuiz()
    {
        var eventQuizForm = Env.Ref("website_event_track_quiz.event_quiz_view_form");
        var action = new Action()
        {
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "WebsiteEventTrackQuiz.EventQuiz",
            ViewId = eventQuizForm.Id,
            Context = new Dictionary<string, object>
            {
                { "default_event_track_id", this.Id },
                { "create", false }
            }
        };
    }

    public void ActionViewQuiz()
    {
        var eventQuizForm = Env.Ref("website_event_track_quiz.event_quiz_view_form");
        var action = new Action()
        {
            Type = "ir.actions.act_window",
            ViewMode = "form",
            ResModel = "WebsiteEventTrackQuiz.EventQuiz",
            ResId = this.QuizId.Id,
            ViewId = eventQuizForm.Id,
            Context = new Dictionary<string, object>
            {
                { "create", false }
            }
        };
    }
}
