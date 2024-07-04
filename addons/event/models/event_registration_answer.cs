csharp
public partial class EventRegistrationAnswer
{
    public override string ToString()
    {
        return ComputeDisplayName();
    }

    private string ComputeDisplayName()
    {
        if (QuestionType == EventQuestionType.SimpleChoice)
        {
            return ValueAnswerId?.Name;
        }
        else
        {
            return ValueTextBox;
        }
    }
}
