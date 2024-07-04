csharp
public partial class ResCompany {
    public virtual void ValidatePeriodLockDate() {
        var posSessionModel = Env.GetModel("Pos.Session");
        foreach (var record in this) {
            var sessionsInPeriod = posSessionModel.Search(new[] {
                new SearchCriteria("company_id", "child_of", record.Id),
                new SearchCriteria("state", "!=", "closed"),
                new SearchCriteria("start_at", "<=", record.GetUserFiscalLockDate()),
            });
            if (sessionsInPeriod.Any()) {
                var sessionsStr = string.Join(", ", sessionsInPeriod.Select(s => s.Name));
                throw new ValidationError($"Please close all the point of sale sessions in this period before closing it. Open sessions are: {sessionsStr}");
            }
        }
    }
}
