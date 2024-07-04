csharp
public partial class ProductTemplate
{
    public Dictionary<string, object> GetTemplateMatrix(Dictionary<string, object> kwargs)
    {
        var company = kwargs.ContainsKey("CompanyId") ? Env.GetRecord<Core.Company>(kwargs["CompanyId"]) : this.CompanyId ?? Env.Company;
        var currency = kwargs.ContainsKey("CurrencyId") ? Env.GetRecord<Core.Currency>(kwargs["CurrencyId"]) : this.CurrencyId;
        var displayExtra = kwargs.ContainsKey("DisplayExtraPrice") ? (bool)kwargs["DisplayExtraPrice"] : false;
        var attributeLines = this.ValidProductTemplateAttributeLineIds;

        var attributeValue = Env.GetModel<Product.ProductTemplateAttributeValue>();
        var firstLineAttributes = attributeLines[0].ProductTemplateValueIds.Where(x => x.Active).ToList();
        var attributeIdsByLine = attributeLines.Select(line => line.ProductTemplateValueIds.Where(x => x.Active).Select(x => x.Id).ToList()).ToList();

        var header = new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Name", this.DisplayName } } };
        header.AddRange(firstLineAttributes.Select(attr => attr.GetGridHeaderCell(this.CurrencyId, currency, company, displayExtra)));

        var result = new List<List<object>> { new List<object>() };
        foreach (var pool in attributeIdsByLine)
        {
            result = result.SelectMany(x => pool.Select(y => x.Concat(new List<object> { y }).ToList())).ToList();
        }

        var args = Enumerable.Repeat(result.Select(x => x.GetEnumerator()).ToList(), firstLineAttributes.Count).ToList();
        var rows = args.ZipLongest().ToList();

        var matrix = new List<List<Dictionary<string, object>>>();
        foreach (var row in rows)
        {
            var rowAttributes = attributeValue.Browse(row[0].Skip(1).ToList());
            var rowHeaderCell = rowAttributes.GetGridHeaderCell(this.CurrencyId, currency, company, displayExtra);
            var resultRow = new List<Dictionary<string, object>> { rowHeaderCell };

            foreach (var cell in row)
            {
                var combination = attributeValue.Browse(cell.Cast<long>().ToList());
                var isPossibleCombination = this.IsCombinationPossible(combination);
                cell.Sort();
                resultRow.Add(new Dictionary<string, object>
                {
                    { "PtavIds", cell },
                    { "Qty", 0 },
                    { "IsPossibleCombination", isPossibleCombination }
                });
            }
            matrix.Add(resultRow);
        }

        return new Dictionary<string, object>
        {
            { "Header", header },
            { "Matrix", matrix }
        };
    }

    private bool IsCombinationPossible(List<Product.ProductTemplateAttributeValue> combination)
    {
        // This method needs to be implemented based on the specific logic of your application.
        return true; // Example: return true if all combinations are possible
    }
}

public partial class ProductTemplateAttributeValue
{
    public Dictionary<string, object> GetGridHeaderCell(Core.Currency froCurrency, Core.Currency toCurrency, Core.Company company, bool displayExtra = true)
    {
        var headerCell = new Dictionary<string, object> { { "Name", this.Name } };
        var extraPrice = displayExtra ? this.PriceExtra : 0;
        if (extraPrice > 0)
        {
            headerCell["CurrencyId"] = toCurrency.Id;
            headerCell["Price"] = froCurrency.Convert(extraPrice, toCurrency, company, DateTime.Today);
        }
        return headerCell;
    }
}
