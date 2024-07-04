csharp
public partial class PurchaseOrderLine
{
    public void ComputeAmount()
    {
        // logic for computing price_subtotal, price_tax, price_total
    }

    public Dictionary<string, object> ConvertToTaxBaseLineDict()
    {
        // logic for converting to dictionary
    }

    public void ComputeTaxId()
    {
        // logic for computing taxes_id
    }

    public void ComputePriceUnitDiscounted()
    {
        // logic for computing price_unit_discounted
    }

    public void ComputeQtyInvoiced()
    {
        // logic for computing qty_invoiced, qty_to_invoice
    }

    public IEnumerable<AccountMoveLine> GetInvoiceLines()
    {
        // logic for getting invoice_lines
    }

    public void ComputeQtyReceivedMethod()
    {
        // logic for computing qty_received_method
    }

    public void ComputeQtyReceived()
    {
        // logic for computing qty_received
    }

    public void InverseQtyReceived()
    {
        // logic for updating qty_received_manual
    }

    public PurchaseOrderLine Create(Dictionary<string, object> values)
    {
        // logic for creating new purchase order line
    }

    public void Write(Dictionary<string, object> values)
    {
        // logic for updating existing purchase order line
    }

    public void UnlinkExceptPurchaseOrDone()
    {
        // logic for unlinking
    }

    public DateTime GetDatePlanned(ResPartner seller, PurchaseOrder po)
    {
        // logic for calculating date_planned
    }

    public void ComputeAnalyticDistribution()
    {
        // logic for computing analytic_distribution
    }

    public void OnchangeProductId()
    {
        // logic for onchange_product_id
    }

    public void ProductIdChange()
    {
        // logic for _product_id_change
    }

    public void OnchangeProductIdWarning()
    {
        // logic for onchange_product_id_warning
    }

    public void ComputePriceUnitAndDatePlannedAndName()
    {
        // logic for computing price_unit, date_planned, name
    }

    public void ComputeProductPackagingId()
    {
        // logic for computing product_packaging_id
    }

    public void OnchangeProductPackagingId()
    {
        // logic for onchange_product_packaging_id
    }

    public void ComputeProductPackagingQty()
    {
        // logic for computing product_packaging_qty
    }

    public void ComputeProductQty()
    {
        // logic for computing product_qty
    }

    public void ComputeProductUomQty()
    {
        // logic for computing product_uom_qty
    }

    public decimal GetGrossPriceUnit()
    {
        // logic for calculating gross_price_unit
    }

    public IrActionsActWindow ActionAddFromCatalog()
    {
        // logic for action_add_from_catalog
    }

    public IrActionsActWindow ActionPurchaseHistory()
    {
        // logic for action_purchase_history
    }

    public void SuggestQuantity()
    {
        // logic for _suggest_quantity
    }

    public Dictionary<string, object> GetProductCatalogLinesData()
    {
        // logic for _get_product_catalog_lines_data
    }

    public string GetProductPurchaseDescription(ProductProduct productLang)
    {
        // logic for _get_product_purchase_description
    }

    public Dictionary<string, object> PrepareAccountMoveLine(AccountMove move)
    {
        // logic for _prepare_account_move_line
    }

    public Dictionary<string, object> PrepareAddMissingFields(Dictionary<string, object> values)
    {
        // logic for _prepare_add_missing_fields
    }

    public Dictionary<string, object> PreparePurchaseOrderLine(ProductProduct productId, decimal productQty, UomUom productUom, ResCompany companyId, ResPartner supplier, PurchaseOrder po)
    {
        // logic for _prepare_purchase_order_line
    }

    public DateTime ConvertToMiddleOfDay(DateTime date)
    {
        // logic for _convert_to_middle_of_day
    }

    public void UpdateDatePlanned(DateTime updatedDate)
    {
        // logic for _update_date_planned
    }

    public void TrackQtyReceived(decimal newQty)
    {
        // logic for _track_qty_received
    }

    public void ValidateAnalyticDistribution()
    {
        // logic for _validate_analytic_distribution
    }

    public IrActionsActWindow ActionOpenOrder()
    {
        // logic for action_open_order
    }
}
