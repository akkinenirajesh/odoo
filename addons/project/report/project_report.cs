csharp
public partial class ProjectReportProjectTaskUser 
{
    public void Init() 
    {
        Env.DropViewIfExist(this._table);
        Env.Execute(String.Format(
            "CREATE view {0} as SELECT {1} FROM {2} WHERE {3} GROUP BY {4}",
            this._table,
            this._Select(),
            this._From(),
            this._Where(),
            this._GroupBy()
        ));
    }

    private string _Select() 
    {
        return @"
                (select 1) AS Nbr,
                t.id as Id,
                t.id as TaskId,
                t.active,
                t.create_date,
                t.date_assign,
                t.date_end,
                t.date_last_stage_update,
                t.date_deadline,
                t.project_id,
                t.priority,
                t.name as Name,
                t.company_id,
                t.partner_id,
                t.parent_id,
                t.stage_id,
                t.state,
                t.milestone_id,
                CASE WHEN t.state IN ('1_done', '1_canceled') THEN True ELSE False END AS IsClosed,
                CASE WHEN pm.id IS NOT NULL THEN true ELSE false END as HasLateAndUnreachedMilestone,
                t.description,
                NULLIF(t.rating_last_value, 0) as RatingLastValue,
                AVG(rt.rating) as RatingAvg,
                NULLIF(t.working_days_close, 0) as WorkingDaysClose,
                NULLIF(t.working_days_open, 0) as WorkingDaysOpen,
                NULLIF(t.working_hours_open, 0) as WorkingHoursOpen,
                NULLIF(t.working_hours_close, 0) as WorkingHoursClose,
                (extract('epoch' from (t.date_deadline-(now() at time zone 'UTC'))))/(3600*24) as DelayEndingsDays,
                COUNT(td.task_id) as DependentIdsCount
        ";
    }

    private string _GroupBy() 
    {
        return @"
                t.id,
                t.active,
                t.create_date,
                t.date_assign,
                t.date_end,
                t.date_last_stage_update,
                t.date_deadline,
                t.project_id,
                t.priority,
                t.name,
                t.company_id,
                t.partner_id,
                t.parent_id,
                t.stage_id,
                t.state,
                t.rating_last_value,
                t.working_days_close,
                t.working_days_open,
                t.working_hours_open,
                t.working_hours_close,
                t.milestone_id,
                pm.id,
                td.depends_on_id
        ";
    }

    private string _From()
    {
        return @"
                project_task t
                    LEFT JOIN rating_rating rt ON rt.res_id = t.id
                          AND rt.res_model = 'project.task'
                          AND rt.consumed = True
                          AND rt.rating >= {RATING_LIMIT_MIN}
                    LEFT JOIN project_milestone pm ON pm.id = t.milestone_id
                          AND pm.is_reached = False
                          AND pm.deadline <= CAST(now() AS DATE)
                    LEFT JOIN task_dependencies_rel td ON td.depends_on_id = t.id
        ";
    }

    private string _Where()
    {
        return @"
                t.project_id IS NOT NULL
        ";
    }
}
