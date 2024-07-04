C#
public partial class WebsiteSlides.KarmaTracking {
    public virtual int Points { get; set; }
    public virtual DateTime Date { get; set; }
    public virtual res.users User { get; set; }
    public virtual slide.slide Slide { get; set; }
    public virtual slide.channel Channel { get; set; }
    public virtual res.partner ResPartner { get; set; }
    public virtual res.company ResCompany { get; set; }
    public virtual slide.session Session { get; set; }
    public virtual slide.course Course { get; set; }

    public virtual string GetOriginSelectionValues() {
        var selectionValues = Env.OptionSets.WebsiteSlides.KarmaTrackingOriginSelection.GetOptions();
        var superValues = base.GetOriginSelectionValues();
        return string.Join(",", selectionValues.Concat(superValues));
    }
}
