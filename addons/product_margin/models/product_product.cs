csharp
public partial class ProductProduct {

    public void ComputeProductMarginFieldsValues()
    {
        DateTime dateFrom = Env.Context.Get<DateTime>("date_from", DateTime.Parse(DateTime.Now.ToString("yyyy-01-01")));
        DateTime dateTo = Env.Context.Get<DateTime>("date_to", DateTime.Parse(DateTime.Now.ToString("yyyy-12-31")));
        string invoiceState = Env.Context.Get<string>("invoice_state", "OpenPaid");
        Dictionary<int, Dictionary<string, object>> res = new Dictionary<int, Dictionary<string, object>>();
        foreach (int productId in this.Ids)
        {
            res[productId] = new Dictionary<string, object>
            {
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
                { "InvoiceState", invoiceState },
                { "Turnover", 0.0 },
                { "SaleAveragePrice", 0.0 },
                { "PurchaseAveragePrice", 0.0 },
                { "SaleNumInvoiced", 0.0 },
                { "PurchaseNumInvoiced", 0.0 },
                { "SalesGap", 0.0 },
                { "PurchaseGap", 0.0 },
                { "TotalCost", 0.0 },
                { "SaleExpected", 0.0 },
                { "NormalCost", 0.0 },
                { "TotalMargin", 0.0 },
                { "ExpectedMargin", 0.0 },
                { "TotalMarginRate", 0.0 },
                { "ExpectedMarginRate", 0.0 }
            };
        }
        string[] states = new string[] { };
        string[] paymentStates = new string[] { };
        if (invoiceState == "Paid")
        {
            states = new string[] { "Posted" };
            paymentStates = new string[] { "InPayment", "Paid", "Reversed" };
        }
        else if (invoiceState == "OpenPaid")
        {
            states = new string[] { "Posted" };
            paymentStates = new string[] { "NotPaid", "InPayment", "Paid", "Reversed", "Partial" };
        }
        else if (invoiceState == "DraftOpenPaid")
        {
            states = new string[] { "Posted", "Draft" };
            paymentStates = new string[] { "NotPaid", "InPayment", "Paid", "Reversed", "Partial" };
        }
        int companyId = Env.Context.ContainsKey("force_company") ? Env.Context.Get<int>("force_company") : Env.Company.Id;
        Env.GetModel<AccountMoveLine>().FlushModel(new[] { "PriceUnit", "Quantity", "Balance", "ProductId", "DisplayType" });
        Env.GetModel<AccountMove>().FlushModel(new[] { "State", "PaymentState", "MoveType", "InvoiceDate", "CompanyId" });
        Env.GetModel<ProductTemplate>().FlushModel(new[] { "ListPrice" });
        string sqlstr = $"""
                WITH currency_rate AS MATERIALIZED ({Env.GetModel<ResCurrency>().SelectCompaniesRates()})
                SELECT
                    l.product_id as product_id,
                    SUM(
                        l.price_unit / (CASE COALESCE(cr.rate, 0) WHEN 0 THEN 1.0 ELSE cr.rate END) *
                        l.quantity * (CASE WHEN i.move_type IN ('out_invoice', 'in_invoice') THEN 1 ELSE -1 END) * ((100 - l.discount) * 0.01)
                    ) / NULLIF(SUM(l.quantity * (CASE WHEN i.move_type IN ('out_invoice', 'in_invoice') THEN 1 ELSE -1 END)), 0) AS avg_unit_price,
                    SUM(l.quantity * (CASE WHEN i.move_type IN ('out_invoice', 'in_invoice') THEN 1 ELSE -1 END)) AS num_qty,
                    SUM(ABS(l.balance) * (CASE WHEN i.move_type IN ('out_invoice', 'in_invoice') THEN 1 ELSE -1 END)) AS total,
                    SUM(l.quantity * pt.list_price * (CASE WHEN i.move_type IN ('out_invoice', 'in_invoice') THEN 1 ELSE -1 END)) AS sale_expected
                FROM account_move_line l
                LEFT JOIN account_move i ON (l.move_id = i.id)
                LEFT JOIN product_product product ON (product.id=l.product_id)
                LEFT JOIN product_template pt ON (pt.id = product.product_tmpl_id)
                left join currency_rate cr on
                (cr.currency_id = i.currency_id and
                 cr.company_id = i.company_id and
                 cr.date_start <= COALESCE(i.invoice_date, NOW()) and
                 (cr.date_end IS NULL OR cr.date_end > COALESCE(i.invoice_date, NOW())))
                WHERE l.product_id IN {string.Join(",", this.Ids)}
                AND i.state IN {string.Join(",", states)}
                AND i.payment_state IN {string.Join(",", paymentStates)}
                AND i.move_type IN ('out_invoice', 'out_refund')
                AND i.invoice_date BETWEEN '{dateFrom.ToString("yyyy-MM-dd")}' AND '{dateTo.ToString("yyyy-MM-dd")}'
                AND i.company_id = {companyId}
                AND l.display_type = 'product'
                GROUP BY l.product_id
                """;
        Env.Cr.Execute(sqlstr);
        foreach (object[] row in Env.Cr.FetchAll())
        {
            int productId = Convert.ToInt32(row[0]);
            double avg = Convert.ToDouble(row[1]);
            double qty = Convert.ToDouble(row[2]);
            double total = Convert.ToDouble(row[3]);
            double sale = Convert.ToDouble(row[4]);
            res[productId]["SaleAveragePrice"] = avg;
            res[productId]["SaleNumInvoiced"] = qty;
            res[productId]["Turnover"] = total;
            res[productId]["SaleExpected"] = sale;
            res[productId]["SalesGap"] = sale - total;
            res[productId]["TotalMargin"] = total;
            res[productId]["ExpectedMargin"] = sale;
            res[productId]["TotalMarginRate"] = total > 0 ? (total * 100 / total) : 0.0;
            res[productId]["ExpectedMarginRate"] = sale > 0 ? (sale * 100 / sale) : 0.0;
        }
        Dictionary<string, object> ctx = new Dictionary<string, object>(Env.Context);
        ctx.Add("force_company", companyId);
        Env.Cr.Execute(sqlstr.Replace("AND i.move_type IN ('out_invoice', 'out_refund')", "AND i.move_type IN ('in_invoice', 'in_refund')"));
        foreach (object[] row in Env.Cr.FetchAll())
        {
            int productId = Convert.ToInt32(row[0]);
            double avg = Convert.ToDouble(row[1]);
            double qty = Convert.ToDouble(row[2]);
            double total = Convert.ToDouble(row[3]);
            res[productId]["PurchaseAveragePrice"] = avg;
            res[productId]["PurchaseNumInvoiced"] = qty;
            res[productId]["TotalCost"] = total;
            res[productId]["TotalMargin"] = Convert.ToDouble(res[productId]["Turnover"]) - total;
            res[productId]["TotalMarginRate"] = Convert.ToDouble(res[productId]["Turnover"]) > 0 ? (Convert.ToDouble(res[productId]["TotalMargin"]) * 100 / Convert.ToDouble(res[productId]["Turnover"])) : 0.0;
        }
        foreach (ProductProduct product in this)
        {
            res[product.Id]["NormalCost"] = product.StandardPrice * Convert.ToDouble(res[product.Id]["PurchaseNumInvoiced"]);
            res[product.Id]["PurchaseGap"] = Convert.ToDouble(res[product.Id]["NormalCost"]) - Convert.ToDouble(res[product.Id]["TotalCost"]);
            res[product.Id]["ExpectedMargin"] = Convert.ToDouble(res[product.Id]["SaleExpected"]) - Convert.ToDouble(res[product.Id]["NormalCost"]);
            res[product.Id]["ExpectedMarginRate"] = Convert.ToDouble(res[product.Id]["SaleExpected"]) > 0 ? (Convert.ToDouble(res[product.Id]["ExpectedMargin"]) * 100 / Convert.ToDouble(res[product.Id]["SaleExpected"])) : 0.0;
            product.Update(res[product.Id]);
        }
    }
}
