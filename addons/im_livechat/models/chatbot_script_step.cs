csharp
public partial class ChatbotScriptStep
{
    public override string ToString()
    {
        return Message;
    }

    public void ComputeIsForwardOperatorChild()
    {
        var parentSteps = ChatbotScript.ScriptSteps
            .Where(s => s.StepType == ChatbotStepType.ForwardOperator || s.StepType == ChatbotStepType.QuestionSelection)
            .OrderByDescending(s => s.Sequence)
            .Where(s => s.Sequence < this.Sequence)
            .ToList();

        var parent = this;
        while (true)
        {
            parent = GetParentStep(parent, parentSteps);
            if (parent == null || parent.StepType == ChatbotStepType.ForwardOperator)
                break;
        }

        IsForwardOperatorChild = parent != null && parent.StepType == ChatbotStepType.ForwardOperator;
    }

    public ChatbotScriptStep GetParentStep(ChatbotScriptStep step, List<ChatbotScriptStep> allParentSteps)
    {
        foreach (var parentStep in allParentSteps)
        {
            if (parentStep.Sequence >= step.Sequence)
                continue;

            if (step.TriggeringAnswers.Any())
            {
                if (!(step.TriggeringAnswers.All(a => parentStep.TriggeringAnswers.Contains(a)) ||
                    step.TriggeringAnswers.Any(a => parentStep.Answers.Contains(a))))
                    continue;
            }
            else if (parentStep.TriggeringAnswers.Any())
            {
                continue;
            }

            return parentStep;
        }

        return null;
    }

    public ChatbotScriptStep FetchNextStep(List<ChatbotScriptAnswer> selectedAnswerIds)
    {
        var steps = Env.Query<ChatbotScriptStep>()
            .Where(s => s.ChatbotScript == this.ChatbotScript && s.Sequence > this.Sequence)
            .Where(s => !s.TriggeringAnswers.Any() || s.TriggeringAnswers.Any(a => selectedAnswerIds.Contains(a)))
            .OrderBy(s => s.Sequence)
            .ToList();

        foreach (var step in steps)
        {
            if (!step.TriggeringAnswers.Any())
                return step;

            var answersByStep = step.TriggeringAnswers
                .GroupBy(a => a.ScriptStep)
                .ToDictionary(g => g.Key, g => g.ToList());

            if (answersByStep.All(kvp => selectedAnswerIds.Any(a => kvp.Value.Contains(a))))
                return step;
        }

        return null;
    }

    public bool IsLastStep(DiscussChannel discussChannel = null)
    {
        if (StepType != ChatbotStepType.QuestionSelection && 
            FetchNextStep(discussChannel?.ChatbotMessageIds.Select(m => m.UserScriptAnswer).ToList() ?? new List<ChatbotScriptAnswer>()) == null)
        {
            return true;
        }

        return false;
    }

    public Dictionary<string, object> FormatForFrontend()
    {
        return new Dictionary<string, object>
        {
            ["id"] = Id,
            ["answers"] = Answers.Select(a => new
            {
                id = a.Id,
                label = a.Name,
                redirectLink = a.RedirectLink
            }).ToList(),
            ["message"] = string.IsNullOrWhiteSpace(Message) ? null : Env.Html.PlainTextToHtml(Message),
            ["isLast"] = IsLastStep(),
            ["type"] = StepType.ToString()
        };
    }
}
