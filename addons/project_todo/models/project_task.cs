C#
public partial class ProjectTask
{
    public virtual void EnsureOnboardingTodo()
    {
        if (!Env.User.IsInGroup("project_todo.group_onboarding_todo"))
        {
            GenerateOnboardingTodo(Env.User);
            var onboardingGroup = Env.Ref("project_todo.group_onboarding_todo").As<Res.Users.Group>().Sudo();
            onboardingGroup.Write(new Dictionary<string, object>() {
                { "Users", new List<object>() { Env.User.Id } }
            });
        }
    }

    public virtual void GenerateOnboardingTodo(Res.Users.User user)
    {
        user.EnsureOne();
        var body = this.WithContext(user.Lang ?? Env.User.Lang).Env.QWeb.Render(
            "project_todo.todo_user_onboarding",
            new Dictionary<string, object>() { { "object", user } },
            minimalQcontext: true,
            raiseIfNotFound: false
        );
        if (string.IsNullOrEmpty(body))
        {
            return;
        }

        var title = Env.Translate("Welcome {0}!", user.Name);
        this.Env.Create<ProjectTask>(new List<Dictionary<string, object>>() {
            new Dictionary<string, object>() {
                { "UserId", new List<object>() { user.Id } },
                { "Description", body },
                { "Name", title }
            }
        });
    }

    public virtual void ActionConvertToTask()
    {
        this.EnsureOne();
        this.CompanyId = this.ProjectId.CompanyId;
    }

    public virtual List<object> GetTodoViewsId()
    {
        return new List<object>() {
            new object[] { Env.Ref("project_todo.project_task_view_todo_kanban").Id, "kanban" },
            new object[] { Env.Ref("project_todo.project_task_view_todo_tree").Id, "list" },
            new object[] { Env.Ref("project_todo.project_task_view_todo_form").Id, "form" },
            new object[] { Env.Ref("project_todo.project_task_view_todo_activity").Id, "activity" },
        };
    }
}
