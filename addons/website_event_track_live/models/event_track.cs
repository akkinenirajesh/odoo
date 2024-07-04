csharp
public partial class WebsiteEventTrack.Track {

    public string YoutubeVideoUrl { get; set; }
    public string YoutubeVideoId { get; set; }
    public bool IsYoutubeReplay { get; set; }
    public bool IsYoutubeChatAvailable { get; set; }
    public string WebsiteImageUrl { get; set; }
    public DateTime Date { get; set; }
    public DateTime DateEnd { get; set; }
    public bool IsTrackDone { get; set; }
    public bool IsTrackUpcoming { get; set; }
    public bool IsTrackLive { get; set; }

    public void ComputeYoutubeVideoId() {
        if (!string.IsNullOrEmpty(this.YoutubeVideoUrl)) {
            var regex = new System.Text.RegularExpressions.Regex(r"^.*(youtu.be\/|v\/|u\/\w\/|embed\/|live\/|watch\?v=|&v=)([^#&?]*).*$");
            var match = regex.Match(this.YoutubeVideoUrl);
            if (match.Success && match.Groups.Count == 3 && match.Groups[2].Value.Length == 11) {
                this.YoutubeVideoId = match.Groups[2].Value;
            }
        }
        if (string.IsNullOrEmpty(this.YoutubeVideoId)) {
            this.YoutubeVideoId = null;
        }
    }

    public void ComputeWebsiteImageUrl() {
        if (!string.IsNullOrEmpty(this.YoutubeVideoId) && string.IsNullOrEmpty(this.WebsiteImageUrl)) {
            this.WebsiteImageUrl = $"https://img.youtube.com/vi/{this.YoutubeVideoId}/maxresdefault.jpg";
        }
    }

    public void ComputeIsYoutubeChatAvailable() {
        this.IsYoutubeChatAvailable = !string.IsNullOrEmpty(this.YoutubeVideoUrl) && !this.IsYoutubeReplay && (this.IsTrackUpcoming || this.IsTrackLive);
    }
}
