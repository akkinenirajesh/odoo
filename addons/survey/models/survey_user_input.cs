csharp
public partial class SurveyUserInput
{
    public SurveyUserInput()
    {
    }

    public int SurveyId { get; set; }
    public string ScoringType { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public DateTime? Deadline { get; set; }
    public string State { get; set; }
    public bool TestEntry { get; set; }
    public int LastDisplayedPageId { get; set; }
    public bool IsAttemptsLimited { get; set; }
    public int AttemptsLimit { get; set; }
    public int AttemptsCount { get; set; }
    public int AttemptsNumber { get; set; }
    public bool SurveyTimeLimitReached { get; set; }
    public string AccessToken { get; set; }
    public string InviteToken { get; set; }
    public int PartnerId { get; set; }
    public string Email { get; set; }
    public string Nickname { get; set; }
    public ICollection<SurveyUserInputLine> UserInputLineIds { get; set; }
    public ICollection<int> PredefinedQuestionIds { get; set; }
    public double ScoringPercentage { get; set; }
    public double ScoringTotal { get; set; }
    public bool ScoringSuccess { get; set; }
    public bool SurveyFirstSubmitted { get; set; }
    public bool IsSessionAnswer { get; set; }
    public bool QuestionTimeLimitReached { get; set; }

    public void ComputeAttemptsInfo()
    {
        if (State == "done" && !TestEntry && Env.Get("Survey.Survey").Browse(SurveyId).IsAttemptsLimited)
        {
            // C# code to compute AttemptsCount and AttemptsNumber
        }
    }

    public void ComputeSurveyTimeLimitReached()
    {
        if (!IsSessionAnswer && StartDateTime.HasValue)
        {
            SurveyTimeLimitReached = Env.Get("Survey.Survey").Browse(SurveyId).IsTimeLimited && DateTime.Now >= StartDateTime.Value.AddMinutes(Env.Get("Survey.Survey").Browse(SurveyId).TimeLimit);
        }
    }

    public void ComputeScoringValues()
    {
        // C# code to compute ScoringPercentage and ScoringTotal
    }

    public void ComputeScoringSuccess()
    {
        ScoringSuccess = ScoringPercentage >= Env.Get("Survey.Survey").Browse(SurveyId).ScoringSuccessMin;
    }

    public void ComputeQuestionTimeLimitReached()
    {
        if (IsSessionAnswer && Env.Get("Survey.Survey").Browse(SurveyId).SessionQuestionStartTime.HasValue)
        {
            QuestionTimeLimitReached = Env.Get("Survey.Survey").Browse(SurveyId).SessionQuestionId.IsTimeLimited && DateTime.Now >= Env.Get("Survey.Survey").Browse(SurveyId).SessionQuestionStartTime.Value.AddSeconds(Env.Get("Survey.Survey").Browse(SurveyId).SessionQuestionId.TimeLimit);
        }
    }

    public void ActionResend()
    {
        // C# code to resend the survey
    }

    public void ActionPrintAnswers()
    {
        // C# code to print the answers
    }

    public void ActionRedirectToAttempts()
    {
        // C# code to redirect to attempts
    }

    public void MarkInProgress()
    {
        // C# code to mark the state as "in_progress"
    }

    public void MarkDone()
    {
        // C# code to mark the state as "done"
    }

    public string GetStartUrl()
    {
        // C# code to get the start URL
        return "";
    }

    public string GetPrintUrl()
    {
        // C# code to get the print URL
        return "";
    }

    public void SaveLines(int questionId, object answer, string comment, bool overwriteExisting = true)
    {
        // C# code to save answers to questions
    }

    public void ClearInactiveConditionalAnswers()
    {
        // C# code to clear inactive conditional answers
    }

    public ICollection<int> GetSelectedSuggestedAnswers()
    {
        // C# code to get selected suggested answers
        return new List<int>();
    }

    public ICollection<int> GetInactiveConditionalQuestions()
    {
        // C# code to get inactive conditional questions
        return new List<int>();
    }

    public ICollection<int> GetPrintQuestions()
    {
        // C# code to get questions to display
        return new List<int>();
    }

    public int GetNextSkippedPageOrQuestion()
    {
        // C# code to get the next skipped page or question
        return 0;
    }

    public ICollection<int> GetSkippedQuestions()
    {
        // C# code to get skipped questions
        return new List<int>();
    }

    public bool IsLastSkippedPageOrQuestion(int pageOrQuestionId)
    {
        // C# code to check if the question or page is the last skipped one
        return false;
    }

    public void NotifyNewParticipationSubscribers()
    {
        // C# code to notify subscribers
    }
}
