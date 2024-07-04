csharp
public partial class PurchaseBillUnion {
    public void Init() {
        // tools.drop_view_if_exists(self.env.cr, 'purchase_bill_union')
        Env.Cr.Execute("""
            CREATE OR REPLACE VIEW purchase_bill_union AS (
                SELECT
                    id, name, ref as reference, partner_id, date, amount_untaxed as amount, currency_id, company_id,
                    id as vendor_bill_id, NULL as purchase_order_id
                FROM account_move
                WHERE
                    move_type='in_invoice' and state = 'posted'
            UNION
                SELECT
                    -id, name, partner_ref as reference, partner_id, date_order::date as date, amount_untaxed as amount, currency_id, company_id,
                    NULL as vendor_bill_id, id as purchase_order_id
                FROM purchase_order
                WHERE
                    state in ('purchase', 'done') AND
                    invoice_status in ('to invoice', 'no')
            )""");
    }

    public void ComputeDisplayName() {
        // @api.depends('currency_id', 'reference', 'amount', 'purchase_order_id')
        // @api.depends_context('show_total_amount')
        string name = this.Name ?? "";
        if (!string.IsNullOrEmpty(this.Reference)) {
            name += " - " + this.Reference;
        }
        decimal amount = this.Amount;
        if (this.PurchaseOrderId != null && this.PurchaseOrderId.InvoiceStatus == "no") {
            amount = 0.0M;
        }
        name += ": " + Env.FormatLang(amount, monetary: true, currencyObj: this.CurrencyId);
        this.DisplayName = name;
    }
}
