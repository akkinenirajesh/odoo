csharp
public partial class IrUiMenu
{
    public List<int> LoadMenusBlacklist()
    {
        var res = base.LoadMenusBlacklist();
        var isInterviewer = Env.User.HasGroup("HrRecruitment.GroupHrRecruitmentInterviewer");
        var isUser = Env.User.HasGroup("HrRecruitment.GroupHrRecruitmentUser");

        if (!isInterviewer)
        {
            res.Add(Env.Ref("Hr.MenuViewHrJob").Id);
        }
        else if (isInterviewer && !isUser)
        {
            res.Add(Env.Ref("HrRecruitment.MenuHrJobPosition").Id);
        }
        else
        {
            res.Add(Env.Ref("HrRecruitment.MenuHrJobPositionInterviewer").Id);
        }

        return res;
    }
}
