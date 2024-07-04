csharp
public partial class Website {
    public void ComputeEventsAppName() {
        if (this.EventsAppName == null) {
            this.EventsAppName = Env.Translate("{0} Events", this.Name);
        }
    }

    public void CheckEventsAppName() {
        if (this.EventsAppName == null) {
            throw new Exception(Env.Translate("\"Events App Name\" field is required."));
        }
    }

    public void ComputeAppIcon() {
        var image = Env.GetImage(this.Favicon);
        if (image == null || image.IsEmpty) {
            this.AppIcon = null;
            return;
        }
        var width = image.Width;
        var height = image.Height;
        var squareSize = width > height ? width : height;
        image.CropResize(squareSize, squareSize);
        image.Resize(512, 512);
        this.AppIcon = Env.ImageToBase64(image, "PNG");
    }
}
