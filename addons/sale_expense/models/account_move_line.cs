C#
public partial class AccountMoveLine {
    public bool SaleCanBeReinvoice()
    {
        if (this.ExpenseId != null)
        {
            return Env.Ref("product.product").GetRecord<Product>(this.ExpenseId.ProductId).ExpensePolicy in new string[] { "sales_price", "cost" }
                   && this.ExpenseId.SaleOrderId != null;
        }
        return this.CallSuper<bool>("SaleCanBeReinvoice");
    }

    public List<Dictionary<string, object>> SaleDetermineOrder()
    {
        var mappingFromInvoice = this.CallSuper<List<Dictionary<string, object>>>("SaleDetermineOrder");
        var mappingFromExpense = new List<Dictionary<string, object>>();
        foreach (var moveLine in this.Filtered(ml => ml.ExpenseId != null))
        {
            mappingFromExpense.Add(new Dictionary<string, object>() { { "id", moveLine.Id }, { "SaleOrderId", moveLine.ExpenseId.SaleOrderId } });
        }
        mappingFromInvoice.AddRange(mappingFromExpense);
        return mappingFromInvoice;
    }

    public Dictionary<string, object> SalePrepareSaleLineValues(SaleOrder order, decimal price)
    {
        var res = this.CallSuper<Dictionary<string, object>>("SalePrepareSaleLineValues", order, price);
        if (this.ExpenseId != null)
        {
            res["ProductUomQty"] = this.ExpenseId.Quantity;
        }
        return res;
    }

    public List<Dictionary<string, object>> SaleCreateReinvoiceSaleLine()
    {
        var expensedLines = this.Filtered(line => line.ExpenseId != null);
        var res = this.CallSuper<List<Dictionary<string, object>>>("SaleCreateReinvoiceSaleLine");
        res.AddRange(this.CallSuper<List<Dictionary<string, object>>>("SaleCreateReinvoiceSaleLine", expensedLines.WithContext(new Dictionary<string, object>() { { "force_split_lines", true } })));
        return res;
    }
}

public partial class AccountMove {
    public List<Dictionary<string, object>> ReverseMoves(List<Dictionary<string, object>> defaultValuesList = null, bool cancel = false)
    {
        this.ExpenseSheetId._SaleExpenseResetSolQuantities();
        return this.CallSuper<List<Dictionary<string, object>>>("ReverseMoves", defaultValuesList, cancel);
    }

    public List<Dictionary<string, object>> ButtonDraft()
    {
        var res = this.CallSuper<List<Dictionary<string, object>>>("ButtonDraft");
        this.ExpenseSheetId._SaleExpenseResetSolQuantities();
        return res;
    }
}
