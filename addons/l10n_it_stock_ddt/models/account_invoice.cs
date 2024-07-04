csharp
public partial class AccountMove
{
    public Dictionary<object, object> L10nItEdiDocumentTypeMapping()
    {
        var res = base.L10nItEdiDocumentTypeMapping();
        foreach (var kvp in res)
        {
            var documentType = kvp.Key;
            var infos = kvp.Value as Dictionary<string, object>;
            if (documentType.ToString() == "TD07")
                continue;
            infos["direct_invoice"] = true;
        }
        res["TD24"] = new Dictionary<string, object>
        {
            { "move_types", new[] { "out_invoice" } },
            { "import_type", "in_invoice" },
            { "direct_invoice", false }
        };
        return res;
    }

    public bool L10nItEdiInvoiceIsDirect()
    {
        foreach (var ddt in L10nItDdtIds)
        {
            if (!ddt.DateDone.HasValue || ddt.DateDone.Value.Date != InvoiceDate.Date)
                return false;
        }
        return true;
    }

    public Dictionary<string, object> L10nItEdiFeaturesForDocumentTypeSelection()
    {
        var res = base.L10nItEdiFeaturesForDocumentTypeSelection();
        res["direct_invoice"] = L10nItEdiInvoiceIsDirect();
        return res;
    }

    public Dictionary<object, List<int>> GetDdtValues()
    {
        if (MoveType != "out_invoice" || State != "posted")
            return new Dictionary<object, List<int>>();

        int lineCount = 0;
        var invoiceLinePickings = new Dictionary<object, List<int>>();

        foreach (var line in InvoiceLineIds.Where(l => l.DisplayType != "line_note" && l.DisplayType != "line_section"))
        {
            lineCount++;
            var doneMoves = line.SaleLineIds.SelectMany(sl => sl.MoveIds)
                .Where(m => m.State == "done" && m.LocationDestId.Usage == "customer" && m.PickingTypeId.Code == "outgoing")
                .ToList();

            if (doneMoves.Count <= 1)
            {
                if (doneMoves.Any() && !invoiceLinePickings.GetValueOrDefault(doneMoves[0].PickingId, new List<int>()).Contains(lineCount))
                {
                    if (!invoiceLinePickings.ContainsKey(doneMoves[0].PickingId))
                        invoiceLinePickings[doneMoves[0].PickingId] = new List<int>();
                    invoiceLinePickings[doneMoves[0].PickingId].Add(lineCount);
                }
            }
            else
            {
                // Implement the complex logic for multiple moves here
                // This part requires careful translation from Python to C#
                // and may need additional helper methods
            }
        }

        return invoiceLinePickings;
    }

    public void ComputeDdtIds()
    {
        if (MoveType == "out_invoice" && Env.Company.AccountFiscalCountryId.Code == "IT")
        {
            var invoiceLinePickings = GetDdtValues();
            L10nItDdtIds = invoiceLinePickings.Keys.Cast<Stock.Picking>().ToList();
            L10nItDdtCount = L10nItDdtIds.Count;
        }
        else
        {
            L10nItDdtIds = new List<Stock.Picking>();
            L10nItDdtCount = 0;
        }
    }

    public Dictionary<string, object> GetLinkedDdts()
    {
        return new Dictionary<string, object>
        {
            { "type", "ir.actions.act_window" },
            { "view_mode", "tree,form" },
            { "name", "Linked deliveries" },
            { "res_model", "Stock.Picking" },
            { "domain", new List<object> { new List<object> { "id", "in", L10nItDdtIds.Select(d => d.Id).ToList() } } }
        };
    }

    public Dictionary<string, object> L10nItEdiGetValues(Dictionary<string, object> pdfValues = null)
    {
        var templateValues = base.L10nItEdiGetValues(pdfValues);
        templateValues["ddt_dict"] = GetDdtValues();
        return templateValues;
    }
}
