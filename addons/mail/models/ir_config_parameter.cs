csharp
using System;
using System.Linq;

public partial class IrConfigParameter
{
    public void SetParam(string key, string value)
    {
        if (key == "mail.restrict.template.rendering")
        {
            var groupUser = Env.Ref("Base.GroupUser");
            var groupMailTemplateEditor = Env.Ref("Mail.GroupMailTemplateEditor");

            if (string.IsNullOrEmpty(value) && !groupUser.ImpliedIds.Contains(groupMailTemplateEditor))
            {
                groupUser.ImpliedIds = groupUser.ImpliedIds.Concat(new[] { groupMailTemplateEditor }).ToList();
            }
            else if (!string.IsNullOrEmpty(value) && groupUser.ImpliedIds.Contains(groupMailTemplateEditor))
            {
                groupUser.RemoveGroup(groupMailTemplateEditor);
            }
        }
        else if (key == "mail.catchall.domain.allowed" && !string.IsNullOrEmpty(value))
        {
            value = Env.Get<MailAlias>().SanitizeAllowedDomains(value);
        }

        base.SetParam(key, value);
    }
}
