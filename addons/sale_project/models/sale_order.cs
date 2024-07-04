csharp
public partial class SaleOrder 
{
    public void ComputeMilestoneCount()
    {
        this.MilestoneCount = Env.Model("project.milestone").ReadGroup(
            new List<object> { new List<object> { "SaleLineId", "in", this.OrderLines.Ids } },
            new List<object> { "SaleLineId" },
            new List<object> { "__count" }
        ).Sum(x => (int)x["__count"]);
    }

    public void ComputeIsProductMilestone()
    {
        this.IsProductMilestone = this.OrderLines.Any(x => x.ProductId.ServicePolicy == "delivered_milestones");
    }

    public void ComputeShowProjectAndTaskButton()
    {
        bool isProjectManager = Env.User.HasGroup("project.group_project_manager");
        var showButtonIds = Env.Model("sale.order.line").ReadGroup(
            new List<object> {
                new List<object> { "OrderId", "in", this.Ids },
                new List<object> { "OrderId.State", "not in", new List<object> { "draft", "sent" } },
                new List<object> { "ProductId.Type", "=", "service" }
            },
            new List<object> { "OrderId:array_agg" },
            new List<object>()
        ).FirstOrDefault();
        if (showButtonIds == null)
        {
            this.ShowProjectButton = false;
            this.ShowTaskButton = false;
            this.ShowCreateProjectButton = false;
            return;
        }
        this.ShowProjectButton = (bool)showButtonIds["OrderId"] && this.ProjectCount > 0;
        this.ShowTaskButton = this.ShowProjectButton || this.TasksCount > 0;
        this.ShowCreateProjectButton = isProjectManager && (bool)showButtonIds["OrderId"] && this.ProjectCount == 0;
    }

    public void ActionConfirm()
    {
        var result = base.ActionConfirm();
        if (this.CompanyIds.Count == 1)
        {
            this.OrderLines.WithCompany(this.CompanyIds.FirstOrDefault()).TimesheetServiceGeneration();
        }
        else
        {
            this.OrderLines.ForEach(orderLine => orderLine.WithCompany(orderLine.OrderId.CompanyIds.FirstOrDefault()).TimesheetServiceGeneration());
        }
        return result;
    }

    public void GenerateAnalyticAccount()
    {
        base.GenerateAnalyticAccount();
        if (!this.AnalyticAccountId && this.OrderLines.Any(sol => sol.IsService && !sol.ProjectId && sol.ProductId.ServiceTracking.IsIn("project_only", "task_in_project")))
        {
            var serviceSols = this.OrderLines.Where(sol => sol.IsService && !sol.ProjectId && sol.ProductId.ServiceTracking.IsIn("project_only", "task_in_project"));
            var serviceProducts = serviceSols.Select(sol => sol.ProductId).Where(p => p.ProjectTemplateId && p.ServiceTracking != "no" && !string.IsNullOrEmpty(p.DefaultCode)).ToList();
            if (serviceProducts.Count == 1)
            {
                _createAnalyticAccount(serviceProducts.First().DefaultCode);
            }
        }
    }

    public void ActionViewTask()
    {
        if (this.OrderLines.Count == 0)
        {
            return;
        }
        var listViewId = Env.Ref("project.view_task_tree2").Id;
        var formViewId = Env.Ref("project.view_task_form2").Id;
        var kanbanViewId = Env.Ref("project.view_task_kanban_inherit_view_default_project").Id;
        var action = Env.Model("ir.actions.actions")._for_xml_id("project.action_view_task");
        if (this.TasksCount > 1)
        {
            for (var i = 0; i < action.Views.Count; i++)
            {
                var view = action.Views[i];
                if (view.Second == "kanban")
                {
                    action.Views[i] = new Tuple<long, string>(kanbanViewId, "kanban");
                }
                else if (view.Second == "tree")
                {
                    action.Views[i] = new Tuple<long, string>(listViewId, "tree");
                }
                else if (view.Second == "form")
                {
                    action.Views[i] = new Tuple<long, string>(formViewId, "form");
                }
            }
        }
        else
        {
            action.Views = new List<Tuple<long, string>> { new Tuple<long, string>(formViewId, "form") };
            action.ResId = this.Tasks.FirstOrDefault().Id;
        }
        var defaultLine = this.OrderLines.FirstOrDefault(sol => sol.ProductId.Type == "service");
        var defaultProjectId = defaultLine.ProjectId?.Id ?? this.Projects.FirstOrDefault()?.Id ?? this.Tasks.FirstOrDefault()?.ProjectId?.Id;
        action.Context = new Dictionary<string, object>
        {
            { "default_sale_order_id", this.Id },
            { "default_sale_line_id", defaultLine.Id },
            { "default_partner_id", this.PartnerId.Id },
            { "default_project_id", defaultProjectId },
            { "default_user_ids", new List<long> { Env.Uid } }
        };
        action.Domain = new List<object>
        {
            "id", "in", this.Tasks.Ids
        };
        return action;
    }

    public void ActionCreateProject()
    {
        if (!this.ShowCreateProjectButton)
        {
            return;
        }
        var sortedLine = this.OrderLines.OrderBy(sol => sol.Sequence);
        var defaultSaleLine = sortedLine.FirstOrDefault(sol => sol.ProductId.Type == "service" && !sol.IsDownpayment);
        var action = Env.Model("ir.actions.actions")._for_xml_id("project.open_create_project");
        action.Context = new Dictionary<string, object>
        {
            { "default_sale_order_id", this.Id },
            { "default_sale_line_id", defaultSaleLine.Id },
            { "default_partner_id", this.PartnerId.Id },
            { "default_user_ids", new List<long> { Env.Uid } },
            { "default_allow_billable", 1 },
            { "hide_allow_billable", true },
            { "default_company_id", this.CompanyIds.FirstOrDefault().Id },
            { "generate_milestone", defaultSaleLine.ProductId.ServicePolicy == "delivered_milestones" }
        };
        return action;
    }

    public void ActionViewProjectIds()
    {
        if (this.OrderLines.Count == 0)
        {
            return;
        }
        var sortedLine = this.OrderLines.OrderBy(sol => sol.Sequence);
        var defaultSaleLine = sortedLine.FirstOrDefault(sol => sol.ProductId.Type == "service");
        var action = new Dictionary<string, object>
        {
            { "type", "ir.actions.act_window" },
            { "name", "Projects" },
            { "domain", new List<object> { "sale_order_id", "=", this.Id } },
            { "res_model", "project.project" },
            { "views", new List<Tuple<long, string>> { new Tuple<long, string>(0, "kanban"), new Tuple<long, string>(0, "tree"), new Tuple<long, string>(0, "form") } },
            { "view_mode", "kanban,tree,form" },
            { "context", new Dictionary<string, object>
                {
                    { "default_partner_id", this.PartnerId.Id },
                    { "default_sale_line_id", defaultSaleLine.Id },
                    { "default_allow_billable", 1 }
                }
            }
        };
        if (this.Projects.Count == 1)
        {
            action["views"] = new List<Tuple<long, string>> { new Tuple<long, string>(0, "form") };
            action["res_id"] = this.Projects.FirstOrDefault().Id;
        }
        return action;
    }

    public void ActionViewMilestone()
    {
        var defaultProject = this.Projects.FirstOrDefault();
        var sortedLine = this.OrderLines.OrderBy(sol => sol.Sequence);
        var defaultSaleLine = sortedLine.FirstOrDefault(sol => sol.IsService && sol.ProductId.ServicePolicy == "delivered_milestones");
        var action = new Dictionary<string, object>
        {
            { "type", "ir.actions.act_window" },
            { "name", "Milestones" },
            { "domain", new List<object> { "sale_line_id", "in", this.OrderLines.Ids } },
            { "res_model", "project.milestone" },
            { "views", new List<Tuple<long, string>> { new Tuple<long, string>(Env.Ref("sale_project.sale_project_milestone_view_tree").Id, "tree") } },
            { "view_mode", "tree" },
            { "help", "<p class=\"o_view_nocontent_smiling_face\">No milestones found. Let's create one!</p><p>Track major progress points that must be reached to achieve success.</p>" },
            { "context", new Dictionary<string, object>
                {
                    { "default_project_id", defaultProject.Id },
                    { "default_sale_line_id", defaultSaleLine.Id }
                }
            }
        };
        return action;
    }

    public void Create(List<object> valsList)
    {
        var createdRecords = base.Create(valsList);
        var project = Env.Model("project.project").Browse(Env.Context.Get("create_for_project_id"));
        if (project != null)
        {
            var serviceSol = createdRecords.OrderLines.FirstOrDefault(sol => sol.IsService);
            if (serviceSol == null && !Env.Context.ContainsKey("from_embedded_action"))
            {
                throw new Exception("This Sales Order must contain at least one product of type \"Service\".");
            }
            if (project.SaleLineId == null)
            {
                project.SaleLineId = serviceSol;
            }
        }
    }

    public void Write(Dictionary<string, object> values)
    {
        base.Write(values);
        if (values.ContainsKey("State") && (string)values["State"] == "cancel")
        {
            Env.Model("project.project").WithUser(Env.Uid).Search(new List<object> { new List<object> { "SaleLineId.OrderId", "=", this.Id } }).ForEach(x => x.SaleLineId = null);
        }
    }

    private void _createAnalyticAccount(string defaultCode)
    {
        var data = _prepareAnalyticAccountData();
        data["name"] = defaultCode;
        this.AnalyticAccountId = Env.Model("account.analytic.account").Create(data);
    }

    private Dictionary<string, object> _prepareAnalyticAccountData()
    {
        var result = base._prepareAnalyticAccountData();
        var projectPlan = Env.Model("account.analytic.plan")._get_all_plans().First();
        result["plan_id"] = projectPlan.Id ?? result["plan_id"];
        return result;
    }
}
