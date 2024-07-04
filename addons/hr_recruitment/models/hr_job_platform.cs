csharp
public partial class JobPlatform
{
    public override string ToString()
    {
        return Name;
    }

    public static IEnumerable<JobPlatform> Create(IEnumerable<JobPlatform> platforms)
    {
        foreach (var platform in platforms)
        {
            if (!string.IsNullOrEmpty(platform.Email))
            {
                platform.Email = Env.Tools.EmailNormalize(platform.Email) ?? platform.Email;
            }
        }
        
        return Env.Create<JobPlatform>(platforms);
    }

    public void Write(JobPlatform updatedPlatform)
    {
        if (!string.IsNullOrEmpty(updatedPlatform.Email))
        {
            updatedPlatform.Email = Env.Tools.EmailNormalize(updatedPlatform.Email) ?? updatedPlatform.Email;
        }
        
        base.Write(updatedPlatform);
    }
}
