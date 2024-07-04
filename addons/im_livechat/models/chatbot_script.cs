csharp
public partial class ChatbotScript
{
    public override string ToString()
    {
        return Title;
    }

    private void _ComputeLivechatChannelCount()
    {
        var channelsData = Env.ImLivechatChannelRule.ReadGroup(
            new[] { ("ChatbotScriptId", "=", this.Id) },
            new[] { "ChatbotScriptId" },
            new[] { "ChannelId:count_distinct" }
        );

        var mappedChannels = channelsData.ToDictionary(
            data => data.ChatbotScriptId,
            data => data.CountDistinct
        );

        LivechatChannelCount = mappedChannels.TryGetValue(Id, out var count) ? count : 0;
    }

    private void _ComputeFirstStepWarning()
    {
        var allowedFirstStepTypes = new[]
        {
            "QuestionSelection",
            "QuestionEmail",
            "QuestionPhone",
            "FreeInputSingle",
            "FreeInputMulti"
        };

        var welcomeSteps = ScriptStepIds.Any() ? GetWelcomeSteps() : new List<ChatbotScriptStep>();

        if (welcomeSteps.Any() && welcomeSteps.Last().StepType == "ForwardOperator")
        {
            FirstStepWarning = FirstStepWarningType.FirstStepOperator;
        }
        else if (welcomeSteps.Any() && !allowedFirstStepTypes.Contains(welcomeSteps.Last().StepType))
        {
            FirstStepWarning = FirstStepWarningType.FirstStepInvalid;
        }
        else
        {
            FirstStepWarning = null;
        }
    }

    public IList<ChatbotScriptStep> GetWelcomeSteps()
    {
        var welcomeSteps = new List<ChatbotScriptStep>();
        foreach (var step in ScriptStepIds.OrderBy(s => s.Sequence))
        {
            welcomeSteps.Add(step);
            if (step.StepType != "Text")
            {
                break;
            }
        }
        return welcomeSteps;
    }

    public Dictionary<string, object> FormatForFrontend()
    {
        return new Dictionary<string, object>
        {
            ["Id"] = Id,
            ["Name"] = Title,
            ["Partner"] = new
            {
                Id = OperatorPartnerId.Id,
                Type = "partner",
                Name = OperatorPartnerId.Name
            },
            ["WelcomeSteps"] = GetWelcomeSteps().Select(step => step.FormatForFrontend()).ToList()
        };
    }

    public Dictionary<string, object> ValidateEmail(string emailAddress, DiscussChannel discussChannel)
    {
        emailAddress = HtmlUtils.Html2Plaintext(emailAddress);
        var emailNormalized = EmailUtils.EmailNormalize(emailAddress);

        string errorMessage = null;
        MailMessage postedMessage = null;

        if (string.IsNullOrEmpty(emailNormalized))
        {
            errorMessage = string.Format(
                "'{0}' does not look like a valid email. Can you please try again?",
                emailAddress
            );
            postedMessage = discussChannel.ChatbotPostMessage(this, HtmlUtils.Plaintext2Html(errorMessage));
        }

        return new Dictionary<string, object>
        {
            ["Success"] = !string.IsNullOrEmpty(emailNormalized),
            ["PostedMessage"] = postedMessage,
            ["ErrorMessage"] = errorMessage
        };
    }

    public string GetChatbotLanguage()
    {
        var frontendLang = HttpContext.Current?.Request.Cookies["frontend_lang"];
        return frontendLang ?? Env.User.Lang ?? Env.GetLang().Code;
    }
}
