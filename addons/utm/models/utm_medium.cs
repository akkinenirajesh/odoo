csharp
public partial class UtmMedium {
    public UtmMedium() { }

    public void Create(List<Dictionary<string, object>> valsList) {
        List<string> newNames = Env.Get("Utm.UtmMixin")._GetUniqueNames("Utm.UtmMedium", valsList.Select(v => (string)v["Name"]).ToList());
        for (int i = 0; i < valsList.Count; i++) {
            valsList[i]["Name"] = newNames[i];
        }
        Env.Create("Utm.UtmMedium", valsList);
    }

    public void _UnlinkExceptUtmMediumRecord() {
        Dictionary<string, string> requiredMediums = new Dictionary<string, string>() {
            { "utm.utm_medium_email", "Email" },
            { "utm.utm_medium_direct", "Direct" },
            { "utm.utm_medium_website", "Website" },
            { "utm.utm_medium_twitter", "X" },
            { "utm.utm_medium_facebook", "Facebook" },
            { "utm.utm_medium_linkedin", "LinkedIn" }
        };

        foreach (string medium in requiredMediums.Keys) {
            var utmMedium = Env.Ref(medium);
            if (utmMedium != null && this.Contains(utmMedium)) {
                throw new Exception($"Oops, you can't delete the Medium '{utmMedium.Name}'.\nDoing so would be like tearing down a load-bearing wall — not the best idea.");
            }
        }
    }

    public object _FetchOrCreateUtmMedium(string name, string module = "utm") {
        try {
            return Env.Ref($"{module}.utm_medium_{name}");
        } catch (Exception) {
            var utmMedium = Env.Create("Utm.UtmMedium", new Dictionary<string, object>() {
                { "Name", requiredMediums.GetValueOrDefault($"{module}.utm_medium_{name}", name) }
            });
            Env.Create("Ir.ModelData", new Dictionary<string, object>() {
                { "Name", $"utm_medium_{name}" },
                { "Module", module },
                { "ResId", utmMedium.Id },
                { "Model", "Utm.UtmMedium" },
            });
            return utmMedium;
        }
    }

    private static Dictionary<string, string> requiredMediums = new Dictionary<string, string>() {
        { "utm.utm_medium_email", "Email" },
        { "utm.utm_medium_direct", "Direct" },
        { "utm.utm_medium_website", "Website" },
        { "utm.utm_medium_twitter", "X" },
        { "utm.utm_medium_facebook", "Facebook" },
        { "utm.utm_medium_linkedin", "LinkedIn" }
    };

    private bool Contains(object utmMedium) {
        // Replace this with the appropriate logic to check if the current object contains the given utmMedium
        return false;
    }
}
