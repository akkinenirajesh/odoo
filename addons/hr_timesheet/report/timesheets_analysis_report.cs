csharp
public partial class TimesheetsAnalysisReport
{
    public override string ToString()
    {
        return Name;
    }

    public string TableQuery
    {
        get
        {
            return $"{Select()} {From()} {Where()}";
        }
    }

    private string Select()
    {
        return @"
            SELECT
                A.id AS id,
                A.name AS Name,
                A.user_id AS User,
                A.project_id AS Project,
                A.task_id AS Task,
                A.parent_task_id AS ParentTask,
                A.employee_id AS Employee,
                A.manager_id AS Manager,
                A.company_id AS Company,
                A.department_id AS Department,
                A.currency_id AS Currency,
                A.date AS Date,
                A.amount AS Amount,
                A.unit_amount AS UnitAmount,
                A.partner_id AS Partner
        ";
    }

    private string From()
    {
        return "FROM account_analytic_line A";
    }

    private string Where()
    {
        return "WHERE A.project_id IS NOT NULL";
    }

    public void Init()
    {
        string query = $"CREATE or REPLACE VIEW {Env.GetTableName(this)} as ({TableQuery})";
        Env.Cr.Execute(query);
    }
}
