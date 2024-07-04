csharp
public partial class PurchaseStock.VendorDelayReport 
{
    public void Init()
    {
        Env.Cr.Execute("""
CREATE OR replace VIEW vendor_delay_report AS(
SELECT m.id                     AS id,
       m.date                   AS date,
       m.purchase_line_id       AS purchase_line_id,
       m.product_id             AS product_id,
       Min(pc.id)               AS category_id,
       Min(po.partner_id)       AS partner_id,
       Min(m.product_qty)       AS qty_total,
       Sum(CASE
             WHEN (m.state = 'done' and pol.date_planned::date >= m.date::date) THEN (ml.quantity / ml_uom.factor * pt_uom.factor)
             ELSE 0
           END)                 AS qty_on_time
FROM   stock_move m
       JOIN purchase_order_line pol
         ON pol.id = m.purchase_line_id
       JOIN purchase_order po
         ON po.id = pol.order_id
       JOIN product_product p
         ON p.id = m.product_id
       JOIN product_template pt
         ON pt.id = p.product_tmpl_id
       JOIN uom_uom pt_uom
         ON pt_uom.id = pt.uom_id
       JOIN product_category pc
         ON pc.id = pt.categ_id
       LEFT JOIN stock_move_line ml
         ON ml.move_id = m.id
       LEFT JOIN uom_uom ml_uom
         ON ml_uom.id = ml.product_uom_id
GROUP  BY m.id
)""")
    }

    public object ReadGroupSelect(string aggregateSpec, string query)
    {
        if (aggregateSpec == "OnTimeRate:sum")
        {
            // Make a weigthed average instead of simple average for these fields
            return SQL(
                "CASE WHEN SUM({0}) !=0 THEN SUM({0}) / SUM({1}) * 100 ELSE 100 END",
                _FieldToSql(this._Table, "QtyTotal", query),
                _FieldToSql(this._Table, "QtyOnTime", query),
                _FieldToSql(this._Table, "QtyTotal", query)
            );
        }
        return base.ReadGroupSelect(aggregateSpec, query);
    }

    public object ReadGroup(object[] domain, object[] groupBy = null, object[] aggregates = null, object[] having = null, int offset = 0, int? limit = null, object[] order = null)
    {
        if (aggregates.Contains("OnTimeRate:sum"))
        {
            having = Expression.AND(having, new object[] {("QtyTotal:sum", ">", "0") });
        }
        return base.ReadGroup(domain, groupBy, aggregates, having, offset, limit, order);
    }

    private string _FieldToSql(string table, string field, string query)
    {
        // Implement _FieldToSql logic here
        return "";
    }
}
