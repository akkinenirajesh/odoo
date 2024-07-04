csharp
public partial class WebsiteEventTrackQuiz_Quiz
{
    // All model methods are written here.
    public void ComputeAwardedPoints()
    {
        foreach (var question in this.QuestionIds)
        {
            question.AwardedPoints = question.AnswerIds.Sum(answer => answer.AwardedPoints);
        }
    }

    public void ComputeCorrectAnswerId()
    {
        foreach (var question in this.QuestionIds)
        {
            question.CorrectAnswerId = question.AnswerIds.Where(e => e.IsCorrect).ToList();
        }
    }
}

public partial class WebsiteEventTrackQuiz_QuizQuestion
{
    // All model methods are written here.
    public void CheckAnswersIntegrity()
    {
        if (this.CorrectAnswerId.Count != 1)
        {
            throw new System.Exception("Question " + this.Name + " must have 1 correct answer to be valid.");
        }

        if (this.AnswerIds.Count < 2)
        {
            throw new System.Exception("Question " + this.Name + " must have 1 correct answer and at least 1 incorrect answer to be valid.");
        }
    }
}
