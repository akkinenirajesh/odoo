csharp
public partial class AccountAnalyticDistributionModel
{
    public override string ToString()
    {
        return CreateDate.ToString();
    }

    public Dictionary<string, object> GetDistribution(Dictionary<string, object> vals)
    {
        var domain = new List<object[]>();
        foreach (var kvp in vals)
        {
            var domainItem = CreateDomain(kvp.Key, kvp.Value);
            if (domainItem != null)
            {
                domain.Add(domainItem);
            }
        }

        int bestScore = 0;
        var res = new Dictionary<string, object>();
        var fnames = GetFieldsToCheck();

        var models = Env.Search<AccountAnalyticDistributionModel>(domain);
        foreach (var rec in models)
        {
            try
            {
                int score = fnames.Sum(key => rec.CheckScore(key, vals.ContainsKey(key) ? vals[key] : null));
                if (score > bestScore)
                {
                    res = rec.AnalyticDistribution;
                    bestScore = score;
                }
            }
            catch (NonMatchingDistributionException)
            {
                continue;
            }
        }

        return res;
    }

    private HashSet<string> GetFieldsToCheck()
    {
        // Implementation depends on how you handle field reflection in your system
        // This is a simplified example
        return new HashSet<string> { "Partner", "PartnerCategory", "Company" };
    }

    private int CheckScore(string key, object value)
    {
        if (key == "Company")
        {
            if (Company == null || value.Equals(Company.Id))
            {
                return Company != null ? 1 : 0;
            }
            throw new NonMatchingDistributionException();
        }

        var propertyInfo = GetType().GetProperty(key);
        if (propertyInfo == null || propertyInfo.GetValue(this) == null)
        {
            return 0;
        }

        if (value != null)
        {
            if (value is List<int> list && propertyInfo.PropertyType == typeof(int))
            {
                if (list.Contains((int)propertyInfo.GetValue(this)))
                {
                    return 1;
                }
            }
            else if (key.EndsWith("Prefix") && value is string strValue)
            {
                if (strValue.StartsWith((string)propertyInfo.GetValue(this)))
                {
                    return 1;
                }
            }
            else if (value.Equals(propertyInfo.GetValue(this)))
            {
                return 1;
            }
        }

        throw new NonMatchingDistributionException();
    }

    private object[] CreateDomain(string fname, object value)
    {
        if (value == null)
        {
            return null;
        }

        if (fname == "PartnerCategory")
        {
            if (value is List<int> list)
            {
                list.Add(0); // Assuming 0 represents False in your system
            }
            else if (value is int intValue)
            {
                value = new List<int> { intValue, 0 };
            }
            return new object[] { fname, "in", value };
        }
        else
        {
            return new object[] { fname, "in", new List<object> { value, null } };
        }
    }

    public Dictionary<string, object> ActionReadDistributionModel()
    {
        return new Dictionary<string, object>
        {
            { "name", ToString() },
            { "type", "ir.actions.act_window" },
            { "view_type", "form" },
            { "view_mode", "form" },
            { "res_model", "Analytic.AccountAnalyticDistributionModel" },
            { "res_id", Id }
        };
    }
}

public class NonMatchingDistributionException : Exception
{
}
