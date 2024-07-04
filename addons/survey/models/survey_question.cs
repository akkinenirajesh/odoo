csharp
public partial class SurveyQuestion 
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string QuestionPlaceholder { get; set; }
    public byte[] BackgroundImage { get; set; }
    public string BackgroundImageUrl { get; set; }
    public SurveySurvey Survey { get; set; }
    public string ScoringType { get; set; }
    public int Sequence { get; set; }
    public bool SessionAvailable { get; set; }
    public bool IsPage { get; set; }
    public ICollection<SurveyQuestion> QuestionIds { get; set; }
    public string QuestionsSelection { get; set; }
    public int RandomQuestionsCount { get; set; }
    public SurveyQuestion Page { get; set; }
    public string QuestionType { get; set; }
    public bool IsScoredQuestion { get; set; }
    public bool HasImageOnlySuggestedAnswer { get; set; }
    public double AnswerNumericalBox { get; set; }
    public DateTime AnswerDate { get; set; }
    public DateTime AnswerDatetime { get; set; }
    public double AnswerScore { get; set; }
    public bool SaveAsEmail { get; set; }
    public bool SaveAsNickname { get; set; }
    public ICollection<SurveyQuestionAnswer> SuggestedAnswerIds { get; set; }
    public string MatrixSubtype { get; set; }
    public ICollection<SurveyQuestionAnswer> MatrixRowIds { get; set; }
    public int ScaleMin { get; set; }
    public int ScaleMax { get; set; }
    public string ScaleMinLabel { get; set; }
    public string ScaleMidLabel { get; set; }
    public string ScaleMaxLabel { get; set; }
    public bool IsTimeLimited { get; set; }
    public bool IsTimeCustomized { get; set; }
    public int TimeLimit { get; set; }
    public bool CommentsAllowed { get; set; }
    public string CommentsMessage { get; set; }
    public bool CommentCountAsAnswer { get; set; }
    public bool ValidationRequired { get; set; }
    public bool ValidationEmail { get; set; }
    public int ValidationLengthMin { get; set; }
    public int ValidationLengthMax { get; set; }
    public double ValidationMinFloatValue { get; set; }
    public double ValidationMaxFloatValue { get; set; }
    public DateTime ValidationMinDate { get; set; }
    public DateTime ValidationMaxDate { get; set; }
    public DateTime ValidationMinDatetime { get; set; }
    public DateTime ValidationMaxDatetime { get; set; }
    public string ValidationErrorMsg { get; set; }
    public bool ConstrMandatory { get; set; }
    public string ConstrErrorMsg { get; set; }
    public ICollection<SurveyUserInputLine> UserInputLineIds { get; set; }
    public ICollection<SurveyQuestion> TriggeringQuestionIds { get; set; }
    public ICollection<SurveyQuestion> AllowedTriggeringQuestionIds { get; set; }
    public bool IsPlacedBeforeTrigger { get; set; }
    public ICollection<SurveyQuestionAnswer> TriggeringAnswerIds { get; set; }

    public virtual void ComputeQuestionPlaceholder() {
        // your C# code to compute QuestionPlaceholder
        // Use Env to access anything from outside
    }

    public virtual void ComputeBackgroundImage() {
        // your C# code to compute BackgroundImage
        // Use Env to access anything from outside
    }

    public virtual void ComputeBackgroundImageUrl() {
        // your C# code to compute BackgroundImageUrl
        // Use Env to access anything from outside
    }

    public virtual void ComputeQuestionIds() {
        // your C# code to compute QuestionIds
        // Use Env to access anything from outside
    }

    public virtual void ComputePageId() {
        // your C# code to compute Page
        // Use Env to access anything from outside
    }

    public virtual void ComputeQuestionType() {
        // your C# code to compute QuestionType
        // Use Env to access anything from outside
    }

    public virtual void ComputeIsScoredQuestion() {
        // your C# code to compute IsScoredQuestion
        // Use Env to access anything from outside
    }

    public virtual void ComputeHasImageOnlySuggestedAnswer() {
        // your C# code to compute HasImageOnlySuggestedAnswer
        // Use Env to access anything from outside
    }

    public virtual void ComputeSaveAsEmail() {
        // your C# code to compute SaveAsEmail
        // Use Env to access anything from outside
    }

    public virtual void ComputeSaveAsNickname() {
        // your C# code to compute SaveAsNickname
        // Use Env to access anything from outside
    }

    public virtual void ComputeValidationRequired() {
        // your C# code to compute ValidationRequired
        // Use Env to access anything from outside
    }

    public virtual void ComputeTriggeringQuestionIds() {
        // your C# code to compute TriggeringQuestionIds
        // Use Env to access anything from outside
    }

    public virtual void ComputeAllowedTriggeringQuestionIds() {
        // your C# code to compute AllowedTriggeringQuestionIds and IsPlacedBeforeTrigger
        // Use Env to access anything from outside
    }
}

public partial class SurveyQuestionAnswer 
{
    public SurveyQuestion Question { get; set; }
    public SurveyQuestion MatrixQuestion { get; set; }
    public string QuestionType { get; set; }
    public int Sequence { get; set; }
    public string ScoringType { get; set; }
    public string Value { get; set; }
    public byte[] ValueImage { get; set; }
    public string ValueImageFilename { get; set; }
    public string ValueLabel { get; set; }
    public bool IsCorrect { get; set; }
    public double AnswerScore { get; set; }

    public virtual void ComputeValueLabel() {
        // your C# code to compute ValueLabel
        // Use Env to access anything from outside
    }
}
