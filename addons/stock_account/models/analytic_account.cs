csharp
public partial class StockAccount.AccountAnalyticPlan {
    public decimal CalculateDistributionAmount(decimal amount, decimal percentage, decimal totalPercentage, Dictionary<StockAccount.AccountAnalyticPlan, Tuple<decimal, decimal>> distributionOnEachPlan) {
        decimal decimalPrecision = Env.GetDecimalPrecision("Percentage Analytic");
        Tuple<decimal, decimal> distributedPercentageAndAmount = distributionOnEachPlan.GetValueOrDefault(this, new Tuple<decimal, decimal>(0, 0));
        decimal allocatedPercentage = distributedPercentageAndAmount.Item1 + percentage;
        decimal calculatedAmount;
        if (allocatedPercentage == totalPercentage) {
            calculatedAmount = (amount * totalPercentage / 100) - distributedPercentageAndAmount.Item2;
        } else {
            calculatedAmount = amount * percentage / 100;
        }
        distributedPercentageAndAmount = new Tuple<decimal, decimal>(allocatedPercentage, distributedPercentageAndAmount.Item2 + calculatedAmount);
        distributionOnEachPlan[this] = distributedPercentageAndAmount;
        return calculatedAmount;
    }
}

public partial class StockAccount.AccountAnalyticAccount {
    public List<Dictionary<string, object>> PerformAnalyticDistribution(Dictionary<StockAccount.AccountAnalyticAccount, decimal> distribution, decimal amount, decimal unitAmount, List<StockAccount.AccountAnalyticLine> lines, object obj, bool additive = false) {
        if (distribution == null) {
            lines.ForEach(l => l.Unlink());
            return new List<Dictionary<string, object>>();
        }
        distribution = distribution.ToDictionary(kvp => Env.GetRecord<StockAccount.AccountAnalyticAccount>(kvp.Key.Id), kvp => kvp.Value);
        List<StockAccount.AccountAnalyticPlan> plans = new List<StockAccount.AccountAnalyticPlan>();
        plans.AddRange(Env.GetRecord<StockAccount.AccountAnalyticPlan>(this.PlanId).GetAllPlans());
        List<string> lineColumns = plans.Select(p => p.ColumnPropertyName()).ToList();
        List<StockAccount.AccountAnalyticLine> linesToUnlink = new List<StockAccount.AccountAnalyticLine>();
        Dictionary<StockAccount.AccountAnalyticPlan, Tuple<decimal, decimal>> distributionOnEachPlan = new Dictionary<StockAccount.AccountAnalyticPlan, Tuple<decimal, decimal>>();
        Dictionary<StockAccount.AccountAnalyticPlan, decimal> totalPercentages = new Dictionary<StockAccount.AccountAnalyticPlan, decimal>();

        foreach (var kvp in distribution) {
            foreach (StockAccount.AccountAnalyticPlan plan in kvp.Key.RootPlanId) {
                if (totalPercentages.ContainsKey(plan)) {
                    totalPercentages[plan] += kvp.Value;
                } else {
                    totalPercentages.Add(plan, kvp.Value);
                }
            }
        }

        foreach (StockAccount.AccountAnalyticLine existingAAL in lines) {
            List<StockAccount.AccountAnalyticAccount> accounts = new List<StockAccount.AccountAnalyticAccount>();
            foreach (string columnName in lineColumns) {
                accounts.AddRange(existingAAL.Get<StockAccount.AccountAnalyticAccount>(columnName));
            }
            if (distribution.ContainsKey(accounts.First())) {
                decimal percentage = distribution[accounts.First()];
                decimal newAmount = 0;
                decimal newUnitAmount = unitAmount;
                foreach (StockAccount.AccountAnalyticAccount account in accounts) {
                    newAmount = account.RootPlanId.CalculateDistributionAmount(amount, percentage, totalPercentages[account.RootPlanId], distributionOnEachPlan);
                }
                if (additive) {
                    newAmount += existingAAL.Amount;
                    newUnitAmount += existingAAL.UnitAmount;
                }
                if (newAmount == 0) {
                    linesToUnlink.Add(existingAAL);
                } else {
                    existingAAL.Amount = newAmount;
                    existingAAL.UnitAmount = newUnitAmount;
                }
                distribution.Remove(accounts.First());
            } else {
                linesToUnlink.Add(existingAAL);
            }
        }
        linesToUnlink.ForEach(l => l.Unlink());
        List<Dictionary<string, object>> linesToLink = new List<Dictionary<string, object>>();
        foreach (var kvp in distribution) {
            Dictionary<string, object> accountFieldValues = new Dictionary<string, object>();
            foreach (StockAccount.AccountAnalyticAccount account in kvp.Key) {
                decimal newAmount = account.RootPlanId.CalculateDistributionAmount(amount, kvp.Value, totalPercentages[account.RootPlanId], distributionOnEachPlan);
                accountFieldValues[account.PlanId.ColumnPropertyName()] = account.Id;
                if (newAmount != 0) {
                    linesToLink.Add(obj.PrepareAnalyticLineValues(accountFieldValues, newAmount, unitAmount));
                }
            }
        }
        return linesToLink;
    }
}
