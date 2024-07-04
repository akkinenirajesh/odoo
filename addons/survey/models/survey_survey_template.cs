C#
public partial class SurveyTemplate
{
    public SurveyTemplate()
    {

    }

    public virtual SurveyType SurveyType { get; set; }
    public virtual string Title { get; set; }
    public virtual string Description { get; set; }
    public virtual string DescriptionDone { get; set; }
    public virtual ProgressionMode ProgressionMode { get; set; }
    public virtual ScoringType ScoringType { get; set; }
    public virtual QuestionsLayout QuestionsLayout { get; set; }
    public virtual QuestionsSelection QuestionsSelection { get; set; }
    public virtual bool Certification { get; set; }
    public virtual AccessMode AccessMode { get; set; }
    public virtual bool IsTimeLimited { get; set; }
    public virtual int TimeLimit { get; set; }
    public virtual bool IsAttemptsLimited { get; set; }
    public virtual int AttemptsLimit { get; set; }
    public virtual bool UsersCanGoBack { get; set; }
    public virtual bool SessionSpeedRating { get; set; }
    public virtual MailTemplate CertificationMailTemplate { get; set; }
    public virtual ICollection<SurveyQuestion> QuestionAndPageIds { get; set; }
    public virtual int RandomQuestionsCount { get; set; }
    public virtual ICollection<SurveyQuestionAnswer> SuggestedAnswerIds { get; set; }
    public virtual ICollection<SurveyQuestionAnswer> MatrixRowIds { get; set; }
    public virtual MatrixSubtype MatrixSubtype { get; set; }

    public virtual void ActionLoadSampleSurvey()
    {
        // implementation for action_load_sample_survey
    }

    public virtual void ActionLoadSampleAssessment()
    {
        // implementation for action_load_sample_assessment
    }

    public virtual void ActionLoadSampleLiveSession()
    {
        // implementation for action_load_sample_live_session
    }

    public virtual void ActionLoadSampleCustom()
    {
        // implementation for action_load_sample_custom
    }

    public virtual void ActionShowSample()
    {
        // implementation for action_show_sample
    }
}
