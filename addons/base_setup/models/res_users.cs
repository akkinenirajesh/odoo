csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ResUsers
{
    public bool WebCreateUsers(IEnumerable<string> emails)
    {
        // Reactivate already existing users if needed
        var deactivatedUsers = Env.Query<ResUsers>()
            .Where(u => !u.Active && (emails.Contains(u.Login) || emails.Contains(u.Email)))
            .ToList();

        foreach (var user in deactivatedUsers)
        {
            user.Active = true;
        }

        var newEmails = emails.Except(deactivatedUsers.Select(u => u.Email));

        // Process new email addresses: create new users
        foreach (var email in newEmails)
        {
            var defaultValues = new ResUsers
            {
                Login = email,
                Name = email.Split('@')[0],
                Email = email,
                Active = true
            };

            Env.Create(defaultValues, new { SignupValid = true });
        }

        return true;
    }

    public IEnumerable<Core.Group> DefaultGroups()
    {
        var defaultUserRights = Env.GetParam<bool>("base_setup.default_user_rights", false);

        if (!defaultUserRights)
        {
            var employeeGroup = Env.Ref<Core.Group>("base.group_user");
            return new[] { employeeGroup }.Concat(employeeGroup.TransImpliedIds);
        }

        return base.DefaultGroups();
    }

    public bool ApplyGroupsToExistingEmployees()
    {
        var defaultUserRights = Env.GetParam<bool>("base_setup.default_user_rights", false);

        if (!defaultUserRights)
        {
            return false;
        }

        return base.ApplyGroupsToExistingEmployees();
    }
}
