csharp
public partial class EventQuestion
{
    public override string ToString()
    {
        return Title;
    }

    public void Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("QuestionType"))
        {
            var newType = (EventQuestionType)vals["QuestionType"];
            if (QuestionType != newType)
            {
                var answerCount = Env.Set<EventRegistrationAnswer>().Where(a => a.Question == this).Count();
                if (answerCount > 0)
                {
                    throw new UserError("You cannot change the question type of a question that already has answers!");
                }
            }
        }
        // Call base Write method or perform actual update logic here
    }

    public void Unlink()
    {
        var answerCount = Env.Set<EventRegistrationAnswer>().Where(a => a.Question == this).Count();
        if (answerCount > 0)
        {
            throw new UserError("You cannot delete a question that has already been answered by attendees.");
        }
        // Perform actual delete logic here
    }

    public ActionResult ActionViewQuestionAnswers()
    {
        var action = Env.Actions.ForXmlId("event.action_event_registration_report");
        action.Domain = new List<object> { new List<object> { "Question", "=", this.Id } };

        if (QuestionType == EventQuestionType.SimpleChoice)
        {
            action.Views = new List<object>
            {
                new List<object> { false, "graph" },
                new List<object> { false, "pivot" },
                new List<object> { false, "tree" }
            };
        }
        else if (QuestionType == EventQuestionType.TextBox)
        {
            action.Views = new List<object> { new List<object> { false, "tree" } };
        }

        return action;
    }
}
