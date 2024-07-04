csharp
public partial class OnboardingOnboarding
{
    public void ComputeIsPerCompany()
    {
        // Once an onboarding is made "per-company", there is no drawback to simply still consider
        // it per-company even when if its last per-company step is unlinked. This allows to avoid
        // handling the merging of existing progress (step) records.

        var onboardingsWithPerCompanyStepsOrProgress = Env.GetObjects<OnboardingOnboarding>().Where(o => o.ProgressIds.Any(p => p.CompanyId != null) || o.StepIds.Any(s => s.IsPerCompany));
        foreach (var o in onboardingsWithPerCompanyStepsOrProgress)
        {
            o.IsPerCompany = true;
        }
        var remainingOnboardings = Env.GetObjects<OnboardingOnboarding>().Except(onboardingsWithPerCompanyStepsOrProgress);
        foreach (var o in remainingOnboardings)
        {
            o.IsPerCompany = false;
        }
    }

    public void ComputeCurrentProgress()
    {
        foreach (var onboarding in Env.GetObjects<OnboardingOnboarding>())
        {
            var currentProgressId = onboarding.ProgressIds.Where(progress => progress.CompanyId == null || progress.CompanyId == Env.Company.Id);
            if (currentProgressId.Any())
            {
                onboarding.CurrentOnboardingState = currentProgressId.First().OnboardingState;
                onboarding.CurrentProgressId = currentProgressId.First();
                onboarding.IsOnboardingClosed = currentProgressId.First().IsOnboardingClosed;
            }
            else
            {
                onboarding.CurrentOnboardingState = "not_done";
                onboarding.CurrentProgressId = null;
                onboarding.IsOnboardingClosed = false;
            }
        }
    }

    public void Write(Dictionary<string, object> vals)
    {
        // Recompute progress step ids if new steps are added/removed.
        var alreadyLinkedSteps = this.StepIds;
        base.Write(vals);
        if (this.StepIds != alreadyLinkedSteps)
        {
            this.ProgressIds.ForEach(p => p.RecomputeProgressStepIds());
        }
    }

    public void ActionClose()
    {
        this.CurrentProgressId.ActionClose();
    }

    public void ActionClosePanel(string xmlid)
    {
        // Close the onboarding panel identified by its `xmlid`.
        // If not found, quietly do nothing.
        var onboarding = Env.Ref(xmlid);
        if (onboarding != null)
        {
            onboarding.ActionClose();
        }
    }

    public void ActionRefreshProgressIds()
    {
        // Re-initialize onboarding progress records (after step is_per_company change).
        // Meant to be called when `is_per_company` of linked steps is modified (or per-company
        // steps are added to an onboarding).
        var onboardingsToRefreshProgress = Env.GetObjects<OnboardingOnboarding>().Where(o => o.IsPerCompany && o.ProgressIds.Any() && o.ProgressIds.All(p => p.CompanyId == null));
        foreach (var o in onboardingsToRefreshProgress)
        {
            o.ProgressIds.ForEach(p => p.Delete());
            o.CreateProgress();
        }
    }

    public void ActionToggleVisibility()
    {
        this.CurrentProgressId.ActionToggleVisibility();
    }

    public OnboardingProgress SearchOrCreateProgress()
    {
        // Create Progress record(s) as necessary for the context.
        var onboardingsWithoutProgress = Env.GetObjects<OnboardingOnboarding>().Where(onboarding => onboarding.CurrentProgressId == null);
        foreach (var onboarding in onboardingsWithoutProgress)
        {
            onboarding.CreateProgress();
        }
        return this.CurrentProgressId;
    }

    public List<OnboardingProgress> CreateProgress()
    {
        return Env.Create<OnboardingProgress>(new List<Dictionary<string, object>>()
        {
            new Dictionary<string, object>()
            {
                { "CompanyId", this.IsPerCompany ? Env.Company.Id : null },
                { "OnboardingId", this.Id },
                { "ProgressStepIds", this.StepIds.Select(s => s.ProgressIds.FirstOrDefault(p => p.CompanyId == null || p.CompanyId == Env.Company.Id)) }
            }
        });
    }

    public Dictionary<string, object> PrepareRenderingValues()
    {
        // Ensure only one record
        if (Env.GetObjects<OnboardingOnboarding>().Count() != 1)
        {
            throw new Exception("Only one record should be processed.");
        }

        var values = new Dictionary<string, object>()
        {
            { "CloseMethod", this.PanelCloseActionName },
            { "CloseModel", "Onboarding.Onboarding" },
            { "Steps", this.StepIds },
            { "State", this.CurrentProgressId.GetAndUpdateOnboardingState() },
            { "TextCompleted", this.TextCompleted }
        };

        return values;
    }
}
