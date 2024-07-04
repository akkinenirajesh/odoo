csharp
public partial class WebsiteEventBoothExhibitor.EventBoothCategory 
{
    public bool UseSponsor { get; set; }
    public WebsiteEventBoothExhibitor.EventSponsorType SponsorTypeId { get; set; }
    public string ExhibitorType { get; set; }

    public void OnChangeUseSponsor()
    {
        if (this.UseSponsor)
        {
            if (this.SponsorTypeId == null)
            {
                this.SponsorTypeId = Env.Get("WebsiteEventBoothExhibitor.EventSponsorType").Search(null, "Sequence DESC", 1).FirstOrDefault();
            }
            if (this.ExhibitorType == null)
            {
                this.ExhibitorType = Env.Get("WebsiteEventBoothExhibitor.EventSponsorType").Options.FirstOrDefault().Value;
            }
        }
    }
}
