csharp
public partial class Tour {

    public void Consume(List<string> tourNames) {
        if (!Env.User.IsInternal()) {
            return;
        }
        foreach (var name in tourNames) {
            Env.Create<Tour>(new { Name = name, UserId = Env.Uid });
        }
    }

    public List<string> GetConsumedTours() {
        return Env.Search<Tour>(new { UserId = Env.Uid }).Select(t => t.Name).ToList();
    }
}
