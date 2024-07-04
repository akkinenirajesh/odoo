csharp
public partial class MailMessageSubtype {

    public MailMessageSubtype Create(Dictionary<string, object> valsList) {
        Env.Registry.ClearCache(); // _get_auto_subscription_subtypes
        return (MailMessageSubtype)base.Create(valsList);
    }

    public MailMessageSubtype Write(Dictionary<string, object> vals) {
        Env.Registry.ClearCache(); // _get_auto_subscription_subtypes
        return (MailMessageSubtype)base.Write(vals);
    }

    public void Unlink() {
        Env.Registry.ClearCache(); // _get_auto_subscription_subtypes
        base.Unlink();
    }

    public Tuple<List<MailMessageSubtype>, List<MailMessageSubtype>, List<MailMessageSubtype>, Dictionary<int, int>, Dictionary<string, List<string>>> GetAutoSubscriptionSubtypes(string modelName) {
        List<MailMessageSubtype> childIds = new List<MailMessageSubtype>();
        List<MailMessageSubtype> defIds = new List<MailMessageSubtype>();
        List<MailMessageSubtype> allIntIds = new List<MailMessageSubtype>();
        Dictionary<int, int> parent = new Dictionary<int, int>();
        Dictionary<string, List<string>> relation = new Dictionary<string, List<string>>();
        List<MailMessageSubtype> subtypes = this.Search(new List<Tuple<string, object>> { Tuple.Create("ResModel", (object)null), Tuple.Create("ResModel", modelName), Tuple.Create("Parent.ResModel", modelName) });
        foreach (MailMessageSubtype subtype in subtypes) {
            if (subtype.ResModel == null || subtype.ResModel == modelName) {
                childIds.Add(subtype);
                if (subtype.Default) {
                    defIds.Add(subtype);
                }
            } else if (!string.IsNullOrEmpty(subtype.RelationField)) {
                parent.Add(subtype.Id, subtype.Parent.Id);
                if (!relation.ContainsKey(subtype.ResModel)) {
                    relation.Add(subtype.ResModel, new List<string>());
                }
                relation[subtype.ResModel].Add(subtype.RelationField);
            }
            if (subtype.Internal) {
                allIntIds.Add(subtype);
            }
        }
        return Tuple.Create(childIds, defIds, allIntIds, parent, relation);
    }

    public Tuple<MailMessageSubtype, MailMessageSubtype, MailMessageSubtype> DefaultSubtypes(string modelName) {
        return this.DefaultSubtypesInternal(modelName);
    }

    private Tuple<MailMessageSubtype, MailMessageSubtype, MailMessageSubtype> DefaultSubtypesInternal(string modelName) {
        List<MailMessageSubtype> subtypes = this.Search(new List<Tuple<string, object>> {
            Tuple.Create("Default", (object)true),
            Tuple.Create("ResModel", modelName),
            Tuple.Create("ResModel", (object)null)
        });
        List<MailMessageSubtype> internal = subtypes.Where(x => x.Internal).ToList();
        return Tuple.Create(subtypes.First(), internal.First(), subtypes.Except(internal).First());
    }
}
