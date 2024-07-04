csharp
public partial class SaleCrmTeam 
{
    public virtual void ComputeDashboardButtonName() 
    {
        Env.Call("Sale.CrmTeam", "_compute_dashboard_button_name");
        if (Env.GetContext("in_sales_app") && this.UseOpportunities)
        {
            this.DashboardButtonName = Env.Translate("Sales Analysis");
        }
    }

    public virtual object ActionPrimaryChannelButton() 
    {
        if (Env.GetContext("in_sales_app") && this.UseOpportunities)
        {
            return Env.Call("ir.actions.actions", "_for_xml_id", "sale.action_order_report_so_salesteam");
        }
        return Env.Call("Sale.CrmTeam", "action_primary_channel_button");
    }

    public virtual string GraphGetModel() 
    {
        if (this.UseOpportunities && Env.GetContext("in_sales_app"))
        {
            return "sale.report";
        }
        return Env.Call("Sale.CrmTeam", "_graph_get_model");
    }

    public virtual string GraphDateColumn() 
    {
        if (this.UseOpportunities && Env.GetContext("in_sales_app"))
        {
            return "date";
        }
        return Env.Call("Sale.CrmTeam", "_graph_date_column");
    }

    public virtual string GraphYQuery() 
    {
        if (this.UseOpportunities && Env.GetContext("in_sales_app"))
        {
            return "SUM(price_subtotal)";
        }
        return Env.Call("Sale.CrmTeam", "_graph_y_query");
    }

    public virtual Tuple<string, string> GraphTitleAndKey() 
    {
        if (this.UseOpportunities && Env.GetContext("in_sales_app"))
        {
            return Tuple.Create("", Env.Translate("Sales: Untaxed Total"));
        }
        return (Tuple<string, string>)Env.Call("Sale.CrmTeam", "_graph_title_and_key");
    }

    public virtual string ExtraSqlConditions() 
    {
        if (this.UseOpportunities && Env.GetContext("in_sales_app"))
        {
            return "AND state = 'sale'";
        }
        return Env.Call("Sale.CrmTeam", "_extra_sql_conditions");
    }
}
