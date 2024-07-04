csharp
public partial class CrmLead {
    public virtual void _MergeGetFields() {
        List<string> fields = new List<string>() { "RevealIp", "RevealIapCredits", "RevealRuleId" };
        fields.AddRange(Env.Call("crm.lead", "_merge_get_fields"));
        Env.Return(fields);
    }
}
