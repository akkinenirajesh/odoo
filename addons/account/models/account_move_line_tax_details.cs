csharp
public partial class AccountMoveLine
{
    public SQL GetQueryTaxDetailsFromDomain(Domain domain, bool fallback = true)
    {
        Env.AccountMoveLine.CheckAccessRights("read");

        var query = Env.AccountMoveLine.WhereCalc(domain);

        // Wrap the query with 'company_id IN (...)' to avoid bypassing company access rights.
        Env.AccountMoveLine.ApplyIrRules(query);

        return GetQueryTaxDetails(query.FromClause, query.WhereClause, fallback);
    }

    public SQL GetExtraQueryBaseTaxLineMapping()
    {
        // TO OVERRIDE
        return new SQL();
    }

    public SQL GetQueryTaxDetails(SQL tableReferences, SQL searchCondition, bool fallback = true)
    {
        // The implementation of this method would be quite complex and would require
        // significant changes to work with C# and a different database system.
        // Here's a skeleton of how it might look:

        var groupTaxes = Env.AccountTax.Search(new Domain().Add("AmountType", "=", "group"));

        var groupTaxesQueryList = new List<SQL>();
        foreach (var groupTax in groupTaxes)
        {
            var childrenTaxes = groupTax.ChildrenTaxIds;
            if (!childrenTaxes.Any()) continue;

            var childrenTaxesInQuery = new SQL(",").Join(childrenTaxes.Select(t => new SQL(t.Id.ToString())));
            groupTaxesQueryList.Add(new SQL($"WHEN tax.id = {groupTax.Id} THEN ARRAY[{childrenTaxesInQuery}]"));
        }

        SQL groupTaxesQuery;
        if (groupTaxesQueryList.Any())
        {
            groupTaxesQuery = new SQL($"UNNEST(CASE {new SQL(" ").Join(groupTaxesQueryList)} ELSE ARRAY[tax.id] END)");
        }
        else
        {
            groupTaxesQuery = new SQL("tax.id");
        }

        // ... (rest of the method implementation)

        return new SQL(/* complex SQL query */);
    }
}
