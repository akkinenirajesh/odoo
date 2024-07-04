csharp
public partial class ResUsers
{
    public virtual int PartnerId { get; set; }
    public virtual string Login { get; set; }
    public virtual string Password { get; set; }
    public virtual string NewPassword { get; set; }
    public virtual string Signature { get; set; }
    public virtual bool Active { get; set; }
    public virtual bool ActivePartner { get; set; }
    public virtual int ActionId { get; set; }
    public virtual List<ResGroups> GroupsId { get; set; }
    public virtual List<ResUsersLog> LogIds { get; set; }
    public virtual DateTime LoginDate { get; set; }
    public virtual bool Share { get; set; }
    public virtual int CompaniesCount { get; set; }
    public virtual string TzOffset { get; set; }
    public virtual List<ResUsersSettings> ResUsersSettingsIds { get; set; }
    public virtual ResUsersSettings ResUsersSettingsId { get; set; }
    public virtual int CompanyId { get; set; }
    public virtual List<ResCompany> CompanyIds { get; set; }
    public virtual string Name { get; set; }
    public virtual string Email { get; set; }
    public virtual int AccessesCount { get; set; }
    public virtual int RulesCount { get; set; }
    public virtual int GroupsCount { get; set; }

    public virtual void ComputeAccessesCount()
    {
        this.AccessesCount = this.Env.Ref<ResGroups>("base.group_user").ModelAccess.Count;
        this.RulesCount = this.Env.Ref<ResGroups>("base.group_user").RuleGroups.Count;
        this.GroupsCount = this.Env.Ref<ResGroups>("base.group_user").Count;
    }

    // Other methods can be added here.
}

public partial class ResUsersLog
{
    public virtual int CreateUid { get; set; }
    public virtual DateTime CreateDate { get; set; }
}

public partial class ResUsersSettings
{
    public virtual int UserId { get; set; }
}

public partial class ResGroups
{
    public virtual string Name { get; set; }
    public virtual List<ResUsers> Users { get; set; }
    public virtual List<IrModelAccess> ModelAccess { get; set; }
    public virtual List<IrRule> RuleGroups { get; set; }
    public virtual List<IrUiMenu> MenuAccess { get; set; }
    public virtual List<IrUiView> ViewAccess { get; set; }
    public virtual string Comment { get; set; }
    public virtual int CategoryId { get; set; }
    public virtual int Color { get; set; }
    public virtual string FullName { get; set; }
    public virtual bool Share { get; set; }

    public virtual void ComputeFullName()
    {
        this.FullName = this.Env.Ref<IrModuleCategory>("base.module_category_user_type").Name + " / " + this.Name;
    }

    public virtual List<ResGroups> SearchFullName(string operator, string operand)
    {
        // Implement search logic for FullName
        return new List<ResGroups>();
    }

    // Other methods can be added here.
}

public partial class ResUsersDeletion
{
    public virtual int UserId { get; set; }
    public virtual string State { get; set; }
}

public partial class ChangePasswordWizard
{
    public virtual List<ChangePasswordUser> UserIds { get; set; }

    public virtual void ChangePasswordButton()
    {
        this.UserIds.ForEach(x => x.ChangePasswordButton());
        // Reload if the current user is among the users whose password is changed
        if (this.Env.User.Id == this.UserIds.FirstOrDefault(x => x.UserId == this.Env.User.Id)?.UserId)
        {
            // Reload the UI
        }
    }

    // Other methods can be added here.
}

public partial class ChangePasswordUser
{
    public virtual int WizardId { get; set; }
    public virtual int UserId { get; set; }
    public virtual string UserLogin { get; set; }
    public virtual string NewPasswd { get; set; }

    public virtual void ChangePasswordButton()
    {
        if (!string.IsNullOrEmpty(this.NewPasswd))
        {
            this.Env.Ref<ResUsers>(this.UserId).ChangePassword(this.NewPasswd);
            this.NewPasswd = null;
        }
    }

    // Other methods can be added here.
}

public partial class ChangePasswordOwn
{
    public virtual string NewPassword { get; set; }
    public virtual string ConfirmPassword { get; set; }

    public virtual void ChangePassword()
    {
        // Check if the password and confirmation match
        // ...

        // Change the password
        this.Env.User.ChangePassword(this.NewPassword);
        // Remove this record
        this.Unlink();
        // Reload the UI
    }

    // Other methods can be added here.
}

public partial class APIKeysUser
{
    public virtual List<APIKeys> APIKeysIds { get; set; }

    // Other methods can be added here.
}

public partial class APIKeys
{
    // Add necessary properties here based on the XML definition.
    // Methods can be added here as well.
}

public partial class APIKeyDescription
{
    public virtual string Name { get; set; }

    public virtual void MakeKey()
    {
        // Implement logic to create a new API key based on this.Name
        // ...

        // Redirect to APIKeyShow model with the generated key
        // ...
    }

    // Other methods can be added here.
}

public partial class APIKeyShow
{
    public virtual int Id { get; set; }
    public virtual string Key { get; set; }

    // Other methods can be added here.
}
