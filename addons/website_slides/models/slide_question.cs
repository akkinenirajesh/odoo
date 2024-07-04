C#
public partial class SlideQuestion {

    public void CheckAnswersIntegrity() {
        var questionsToFix = Env.Context.Get<List<SlideQuestion>>()
            .Where(question => !string.IsNullOrEmpty(question.AnswersValidationError))
            .Select(question => $"- {question.SlideId.Name}: {question.Question}")
            .ToList();
        if (questionsToFix.Any()) {
            throw new Exception($"All questions must have at least one correct answer and one incorrect answer: \n{string.Join("\n", questionsToFix)}\n");
        }
    }

    public void ComputeStatistics() {
        var slidePartners = Env.Context.Get<List<SlideSlidePartner>>().Where(x => x.SlideId.Id == this.SlideId.Id).ToList();
        var attemptsCount = slidePartners.Sum(x => x.QuizAttemptsCount);
        var attemptsUnique = slidePartners.Count;
        var doneCount = slidePartners.Where(x => x.Completed).Count();
        this.AttemptsCount = attemptsCount;
        this.AttemptsAvg = attemptsUnique == 0 ? 0 : (double)attemptsCount / attemptsUnique;
        this.DoneCount = doneCount;
    }

    public void ComputeAnswersValidationError() {
        this.AnswersValidationError = this.AnswerIds.Where(x => x.IsCorrect).Count() == 0 || this.AnswerIds.Count == this.AnswerIds.Where(x => x.IsCorrect).Count()
            ? string.Empty 
            : "This question must have at least one correct answer and one incorrect answer.";
    }
}
