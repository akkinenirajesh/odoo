csharp
public partial class WebsiteEventCrm.EventRegistration
{
    public string _GetLeadDescriptionRegistration(string lineSuffix = "")
    {
        string regDescription = Env.Call("WebsiteEventCrm.EventRegistration", "_GetLeadDescriptionRegistration", this, lineSuffix);
        if (!this.RegistrationAnswerIds.Any())
        {
            return regDescription;
        }

        List<string> answerDescriptions = new List<string>();
        foreach (var answer in this.RegistrationAnswerIds)
        {
            string answerValue = answer.ValueAnswerId.Name;
            if (answer.QuestionType == "TextBox")
            {
                answerValue = answer.ValueTextBox;
            }

            answerValue = string.Join("<br/>", answerValue.Split('\n').Select(line => $"    {line}"));
            answerDescriptions.Add($"<br/>  - {answer.QuestionId.Title}<br/>{answerValue}");
        }

        return $"{regDescription}{Env.Translate("Questions")}<br/>{string.Join("<br/>", answerDescriptions)}";
    }

    public List<string> _GetLeadDescriptionFields()
    {
        List<string> res = Env.Call<List<string>>("WebsiteEventCrm.EventRegistration", "_GetLeadDescriptionFields", this);
        res.Add("RegistrationAnswerIds");
        return res;
    }

    public Dictionary<string, object> _GetLeadValues(object rule)
    {
        Dictionary<string, object> leadValues = Env.Call<Dictionary<string, object>>("WebsiteEventCrm.EventRegistration", "_GetLeadValues", this, rule);
        leadValues.Add("VisitorIds", this.VisitorId);
        leadValues.Add("LangId", this.VisitorId.LangId.Id);
        return leadValues;
    }
}
