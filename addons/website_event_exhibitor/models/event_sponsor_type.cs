csharp
public partial class WebsiteEventExhibitor.EventSponsorType {
    public int _defaultSequence() {
        return Env.Search<WebsiteEventExhibitor.EventSponsorType>(null, "Sequence desc", 1).FirstOrDefault().Sequence + 1;
    }
}
