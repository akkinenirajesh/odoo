csharp
public partial class Device
{
    public void Unlink()
    {
        // Notify users when trusted devices are removed from their account.
        var removedDevicesByUser = ClassifyByUser();
        foreach (var (user, removedDevices) in removedDevicesByUser)
        {
            user.NotifySecuritySettingUpdate(
                "Security Update: Device Removed",
                $"A trusted device has just been removed from your account: {string.Join(", ", removedDevices.Select(d => d.Name))}"
            );
        }

        // Call the base Unlink method (assuming it exists in the base class or another partial definition)
        base.Unlink();
    }

    public static Device Generate(string scope, string name)
    {
        // Notify users when trusted devices are added to their account.
        var device = GenerateBase(scope, name);

        Env.User.NotifySecuritySettingUpdate(
            "Security Update: Device Added",
            $"A trusted device has just been added to your account: {name}"
        );

        return device;
    }

    private Dictionary<Core.User, List<Device>> ClassifyByUser()
    {
        var devicesByUser = new Dictionary<Core.User, List<Device>>();
        var devices = Env.Set<Device>().Search(new[] { this });

        foreach (var device in devices)
        {
            if (!devicesByUser.ContainsKey(device.User))
            {
                devicesByUser[device.User] = new List<Device>();
            }
            devicesByUser[device.User].Add(device);
        }

        return devicesByUser;
    }
}
