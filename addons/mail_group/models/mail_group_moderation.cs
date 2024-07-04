csharp
public partial class MailGroupModeration 
{
    public void Create(ref Dictionary<string, object> values)
    {
        string emailNormalized = Env.EmailNormalize(values["Email"]);
        if (string.IsNullOrEmpty(emailNormalized))
        {
            throw new Exception($"Invalid email address “{values["Email"]}”");
        }
        values["Email"] = emailNormalized;
    }

    public void Write(ref Dictionary<string, object> values)
    {
        if (values.ContainsKey("Email"))
        {
            string emailNormalized = Env.EmailNormalize(values["Email"]);
            if (string.IsNullOrEmpty(emailNormalized))
            {
                throw new Exception($"Invalid email address “{values["Email"]}”");
            }
            values["Email"] = emailNormalized;
        }
    }
}
