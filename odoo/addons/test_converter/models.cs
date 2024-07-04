csharp
public partial class TestModel {
    public virtual object GbfM2O(object subs, object domain) {
        var subIds = Env.Search("TestConverter.TestModelSub", new Dictionary<string, object>());
        return Env.Browse("TestConverter.TestModelSub", subIds);
    }
}
