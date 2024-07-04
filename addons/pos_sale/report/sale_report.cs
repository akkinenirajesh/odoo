csharp
public partial class PosSale.SaleReport
{
    public virtual void GetDoneStates()
    {
        var doneStates = Env.Call("sale.report", "_get_done_states");
        doneStates.AddRange(new string[] { "paid", "invoiced", "done" });
        Env.Return(doneStates);
    }

    public virtual string SelectPos()
    {
        var select_ = @"
            -MIN(l.id) AS id,
            l.product_id AS product_id,
            NULL AS line_invoice_status,
            t.uom_id AS product_uom,
            SUM(l.qty) AS product_uom_qty,
            SUM(l.qty_delivered) AS qty_delivered,
            SUM(l.qty - l.qty_delivered) AS qty_to_deliver,
            CASE WHEN pos.state = 'invoiced' THEN SUM(l.qty) ELSE 0 END AS qty_invoiced,
            CASE WHEN pos.state != 'invoiced' THEN SUM(l.qty) ELSE 0 END AS qty_to_invoice,
            SUM(l.price_unit)
                / MIN({this.CaseValueOrOne("pos.currency_rate")})
                * {this.CaseValueOrOne("currency_table.rate")}
            AS price_unit,
            SUM(l.price_subtotal_incl)
                / MIN({this.CaseValueOrOne("pos.currency_rate")})
                * {this.CaseValueOrOne("currency_table.rate")}
            AS price_total,
            SUM(l.price_subtotal)
                / MIN({this.CaseValueOrOne("pos.currency_rate")})
                * {this.CaseValueOrOne("currency_table.rate")}
            AS price_subtotal,
            (CASE WHEN pos.state != 'invoiced' THEN SUM(l.price_subtotal) ELSE 0 END)
                / MIN({this.CaseValueOrOne("pos.currency_rate")})
                * {this.CaseValueOrOne("currency_table.rate")}
            AS amount_to_invoice,
            (CASE WHEN pos.state = 'invoiced' THEN SUM(l.price_subtotal) ELSE 0 END)
                / MIN({this.CaseValueOrOne("pos.currency_rate")})
                * {this.CaseValueOrOne("currency_table.rate")}
            AS amount_invoiced,
            count(*) AS nbr,
            pos.name AS name,
            pos.date_order AS date,
            (CASE WHEN pos.state = 'done' THEN 'sale' ELSE pos.state END) AS state,
            NULL as invoice_status,
            pos.partner_id AS partner_id,
            pos.user_id AS user_id,
            pos.company_id AS company_id,
            NULL AS campaign_id,
            NULL AS medium_id,
            NULL AS source_id,
            t.categ_id AS categ_id,
            pos.pricelist_id AS pricelist_id,
            NULL AS analytic_account_id,
            pos.crm_team_id AS team_id,
            p.product_tmpl_id,
            partner.commercial_partner_id AS commercial_partner_id,
            partner.country_id AS country_id,
            partner.industry_id AS industry_id,
            partner.state_id AS state_id,
            partner.zip AS partner_zip,
            (SUM(p.weight) * l.qty / u.factor) AS weight,
            (SUM(p.volume) * l.qty / u.factor) AS volume,
            l.discount AS discount,
            SUM((l.price_unit * l.discount * l.qty / 100.0
                / {this.CaseValueOrOne("pos.currency_rate")}
                * {this.CaseValueOrOne("currency_table.rate")}))
            AS discount_amount,
            concat('pos.order', ',', pos.id) AS order_reference";

        var additionalFields = this.SelectAdditionalFields();
        var additionalFieldsInfo = this.FillPosFields(additionalFields);
        var template = @",
            {0} AS {1}";

        foreach (var (fname, value) in additionalFieldsInfo)
        {
            select_ += string.Format(template, value, fname);
        }

        return select_;
    }

    public virtual List<string> AvailableAdditionalPosFields()
    {
        return new List<string>()
        {
            "warehouse_id",
        };
    }

    public virtual Dictionary<string, string> FillPosFields(List<string> additionalFields)
    {
        var filledFields = new Dictionary<string, string>();

        foreach (var x in additionalFields)
        {
            filledFields.Add(x, "NULL");
        }

        foreach (var (fname, value) in AvailableAdditionalPosFields())
        {
            if (additionalFields.Contains(fname))
            {
                filledFields[fname] = value;
            }
        }

        return filledFields;
    }

    public virtual string FromPos()
    {
        var currencyTableSql = Env.Call("res.currency", "_get_query_currency_table", new object[] { Env.Companies.Ids, DateTime.Now });
        return $@"
            pos_order_line l
            JOIN pos_order pos ON l.order_id = pos.id
            LEFT JOIN res_partner partner ON (pos.partner_id=partner.id OR pos.partner_id = NULL)
            LEFT JOIN product_product p ON l.product_id=p.id
            LEFT JOIN product_template t ON p.product_tmpl_id=t.id
            LEFT JOIN uom_uom u ON u.id=t.uom_id
            LEFT JOIN pos_session session ON session.id = pos.session_id
            LEFT JOIN pos_config config ON config.id = session.config_id
            LEFT JOIN stock_picking_type picking ON picking.id = config.picking_type_id
            JOIN {Env.Cr.Mogrify(currencyTableSql).ToString()} ON currency_table.company_id = pos.company_id
            ";
    }

    public virtual string WherePos()
    {
        return @"
            l.sale_order_line_id IS NULL";
    }

    public virtual string GroupByPos()
    {
        return @"
            l.order_id,
            l.product_id,
            l.price_unit,
            l.discount,
            l.qty,
            t.uom_id,
            t.categ_id,
            pos.id,
            pos.name,
            pos.date_order,
            pos.partner_id,
            pos.user_id,
            pos.state,
            pos.company_id,
            pos.pricelist_id,
            p.product_tmpl_id,
            partner.commercial_partner_id,
            partner.country_id,
            partner.industry_id,
            partner.state_id,
            partner.zip,
            u.factor,
            pos.crm_team_id,
            currency_table.rate,
            picking.warehouse_id";
    }

    public virtual string Query()
    {
        var res = Env.Call("sale.report", "_query");
        return $"{res}UNION ALL (
            SELECT {this.SelectPos()}
            FROM {this.FromPos()}
            WHERE {this.WherePos()}
            GROUP BY {this.GroupByPos()}
            )
        ";
    }

    public virtual object CaseValueOrOne(string field)
    {
        var caseSql = $@"CASE WHEN {field} IS NULL THEN 1 ELSE {field} END";
        return Env.Cr.Mogrify(caseSql).ToString();
    }

    public virtual List<string> SelectAdditionalFields()
    {
        return Env.Call("sale.report", "_select_additional_fields");
    }
}
