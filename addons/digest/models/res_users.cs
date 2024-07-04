csharp
public partial class ResUsers
{
    public override ResUsers Create(Dictionary<string, object> values)
    {
        ResUsers user = base.Create(values);

        bool defaultDigestEmails = Env.Get<IrConfigParameter>().GetParam<bool>("digest.default_digest_emails");
        int? defaultDigestId = Env.Get<IrConfigParameter>().GetParam<int?>("digest.default_digest_id");

        if (defaultDigestEmails && defaultDigestId.HasValue && !user.Share)
        {
            DigestDigest digest = Env.Get<DigestDigest>().Browse(defaultDigestId.Value);
            if (digest != null)
            {
                digest.UserIds = digest.UserIds.Concat(new[] { user }).ToList();
            }
        }

        return user;
    }
}
