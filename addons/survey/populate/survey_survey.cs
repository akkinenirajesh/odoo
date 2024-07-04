csharp
public partial class Survey {
    public virtual int SessionCode { get; set; }
    public virtual string Title { get; set; }
    public virtual SurveyScoringType ScoringType { get; set; }

    public virtual List<SurveyQuestion> QuestionAndPageIds { get; set; }
    public virtual List<ResUsers> RestrictUserIds { get; set; }
    public virtual ResUsers UserId { get; set; }

    public virtual int GetSessionCode(int counter) {
        int highestCode = Env.ReadGroup<Survey>(null, null, "SessionCode:max")[0].SessionCode;
        return highestCode + counter + 1;
    }

    public virtual List<SurveyQuestion> GetQuestionAndPageIds() {
        Random random = new Random();
        List<SurveyQuestion> questions = new List<SurveyQuestion>();
        for (int questionIdx = 1; questionIdx <= random.Next(5, 21); questionIdx++) {
            List<SurveyQuestionAnswer> answers = new List<SurveyQuestionAnswer>();
            for (int answerIdx = 1; answerIdx <= random.Next(1, 5) + 1; answerIdx++) {
                answers.Add(new SurveyQuestionAnswer() { Value = $"Answer {answerIdx} of question {questionIdx} for {this.Title}" });
            }
            questions.Add(new SurveyQuestion() { Title = $"Question {questionIdx} for {this.Title}", QuestionType = SurveyQuestionType.MultipleChoice, SuggestedAnswerIds = answers });
        }
        return questions;
    }

    public virtual List<ResUsers> GetRestrictUserIds() {
        Random random = new Random();
        List<int> userIds = Env.ReadGroup<ResUsers>(null, null, "Id:max")[0].Id;
        List<ResUsers> activeUsers = Env.Browse<ResUsers>(userIds).Where(u => u.Active).ToList();
        int nbUsers = random.Next(9);
        int startIdx = random.Next(activeUsers.Count - nbUsers);
        return activeUsers.GetRange(startIdx, nbUsers);
    }

    public virtual ResUsers GetUserId() {
        return this.RestrictUserIds.Count > 0 ? this.RestrictUserIds[0] : Env.User;
    }
}
