csharp
public partial class TotpDevice
{
    public bool CheckCredentialsForUid(string scope, string key, int uid)
    {
        if (uid == 0)
        {
            throw new ArgumentException("uid is required");
        }
        return CheckCredentials(scope, key) == uid;
    }

    public static void GcDevice()
    {
        var trustedDeviceAge = 30 * 24 * 60 * 60; // 30 days in seconds, adjust as needed
        var expirationDate = DateTime.UtcNow.AddSeconds(-trustedDeviceAge);

        var expiredDevices = Env.Query<TotpDevice>()
            .Where(d => d.CreateDate < expirationDate)
            .ToList();

        foreach (var device in expiredDevices)
        {
            Env.Delete(device);
        }

        Env.Logger.Info($"GC'd {expiredDevices.Count} totp devices entries");
    }

    private int CheckCredentials(string scope, string key)
    {
        // Implement the logic to check credentials
        // This method should return the user ID if credentials are valid, or 0 otherwise
        throw new NotImplementedException();
    }
}
