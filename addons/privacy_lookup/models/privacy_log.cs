csharp
public partial class PrivacyLog {
    public PrivacyLog() {
    }

    public void Create(List<Dictionary<string, object>> valsList) {
        foreach (Dictionary<string, object> vals in valsList) {
            vals["AnonymizedName"] = AnonymizeName((string)vals["AnonymizedName"]);
            vals["AnonymizedEmail"] = AnonymizeEmail((string)vals["AnonymizedEmail"]);
        }
        // Call base class create method
    }

    private string AnonymizeName(string label) {
        if (string.IsNullOrEmpty(label)) {
            return "";
        }
        if (label.Contains("@")) {
            return AnonymizeEmail(label);
        }
        return string.Join(" ", label.Split(' ').Select(e => e[0] + new string('*', e.Length - 1)));
    }

    private string AnonymizeEmail(string label) {
        if (string.IsNullOrEmpty(label) || !label.Contains("@")) {
            throw new Exception($"This email address is not valid ({label})");
        }
        string user = label.Split('@')[0];
        string domain = label.Split('@')[1];
        return $"{AnonymizeUser(user)}@{AnonymizeDomain(domain)}";
    }

    private string AnonymizeUser(string label) {
        return string.Join(".", label.Split('.').Select(e => e[0] + new string('*', e.Length - 1)));
    }

    private string AnonymizeDomain(string label) {
        if (label == "gmail.com" || label == "hotmail.com" || label == "yahoo.com") {
            return label;
        }
        string[] splitDomain = label.Split('.');
        return string.Join(".", splitDomain.Take(splitDomain.Length - 1).Select(e => e[0] + new string('*', e.Length - 1)).ToArray()) + "." + splitDomain.Last();
    }
}
