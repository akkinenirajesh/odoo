csharp
public partial class PhoneValidation.ResUsers 
{
    public void DeactivatePortalUser(Dictionary<string, object> post)
    {
        Dictionary<string, PhoneValidation.ResUsers> numbersToBlacklist = new Dictionary<string, PhoneValidation.ResUsers>();
        if (post.ContainsKey("requestBlacklist"))
        {
            foreach (PhoneValidation.ResUsers user in this)
            {
                foreach (string fname in GetPhoneNumberFields())
                {
                    string number = FormatPhone(fname);
                    if (!string.IsNullOrEmpty(number))
                    {
                        numbersToBlacklist.Add(number, user);
                    }
                }
            }
        }

        base.DeactivatePortalUser(post);

        if (numbersToBlacklist.Count > 0)
        {
            PhoneValidation.ResUsers currentUser = Env.User;
            List<PhoneValidation.PhoneBlacklist> blacklists = Env.GetModel("PhoneValidation.PhoneBlacklist")._Add(numbersToBlacklist.Keys.ToList());
            foreach (PhoneValidation.PhoneBlacklist blacklist in blacklists)
            {
                PhoneValidation.ResUsers user = numbersToBlacklist[blacklist.Number];
                blacklist.MessageLog(
                    body: $"Blocked by deletion of portal account {user.Name} by {currentUser.Name} (#{currentUser.Id})",
                    user: currentUser
                );
            }
        }
    }

    private List<string> GetPhoneNumberFields()
    {
        return new List<string> { "Phone", "Mobile", "Fax", "WorkPhone" };
    }

    private string FormatPhone(string fname)
    {
        return this.GetFieldValue<string>(fname);
    }

}
