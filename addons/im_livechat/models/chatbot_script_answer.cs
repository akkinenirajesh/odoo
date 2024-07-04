csharp
public partial class ChatbotScriptAnswer
{
    public override string ToString()
    {
        if (Env.Context.GetValueOrDefault("chatbot_script_answer_display_short_name", false))
        {
            return base.ToString();
        }

        if (ScriptStep != null)
        {
            string answerMessage = ScriptStep.Message.Replace("\n", " ");
            string shortenedMessage = TextWrapper.Shorten(answerMessage, 26, " [...]");
            return $"{shortenedMessage}: {Name}";
        }
        
        return Name;
    }

    public IEnumerable<ChatbotScriptAnswer> NameSearch(string name, IEnumerable<object> domain = null, string operatorValue = "ilike", int? limit = null, string order = null)
    {
        domain = domain ?? new List<object>();

        if (!string.IsNullOrEmpty(name) && operatorValue == "ilike")
        {
            var nameDomain = new List<object> { new List<object> { "Name", operatorValue, name } };
            var stepDomain = new List<object> { new List<object> { "ScriptStep.Message", operatorValue, name } };
            domain = Env.Expression.And(domain, Env.Expression.Or(nameDomain, stepDomain));
        }

        int? forceDomainChatbotScriptId = Env.Context.GetValueOrDefault("force_domain_chatbot_script_id", null) as int?;
        if (forceDomainChatbotScriptId.HasValue)
        {
            domain = Env.Expression.And(domain, new List<object> { new List<object> { "ChatbotScript", "=", forceDomainChatbotScriptId.Value } });
        }

        return Search(domain, limit: limit, order: order);
    }
}
