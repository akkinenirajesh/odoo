csharp
public partial class WebsiteEventTrack_TrackVisitor {
    // all the model methods are written here.
    public void ComputePartnerId() {
        if (this.VisitorId.PartnerId != null && this.PartnerId == null) {
            this.PartnerId = this.VisitorId.PartnerId;
        } else if (this.PartnerId == null) {
            this.PartnerId = null;
        }
    }
}
