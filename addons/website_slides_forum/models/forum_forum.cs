C#
public partial class WebsiteSlidesForum.Forum {
    public void ComputeSlideChannelId() {
        if (this.SlideChannelIds.Count > 0) {
            this.SlideChannelId = this.SlideChannelIds[0];
        } else {
            this.SlideChannelId = null;
        }
    }

    public void ComputeImage1920() {
        if (!this.Image1920 && this.SlideChannelId.Image1920) {
            this.Image1920 = this.SlideChannelId.Image1920;
        }
    }
}
