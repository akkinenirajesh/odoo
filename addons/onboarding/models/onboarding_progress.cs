csharp
public partial class OnboardingProgress
{
    public void Init()
    {
        Env.Cr.Execute("""
            CREATE UNIQUE INDEX IF NOT EXISTS onboarding_progress_onboarding_company_uniq
            ON onboarding_progress (onboarding_id, COALESCE(company_id, 0))
        """);
    }

    public void ComputeOnboardingState()
    {
        OnboardingState = (
            (
                ProgressStepIds.Where(p => p.StepState == "JustDone" || p.StepState == "Done").Count()
                != OnboardingId.StepIds.Count()
            )
            ? "NotDone"
            : "Done"
        );
    }

    public void RecomputeProgressStepIds()
    {
        ProgressStepIds = OnboardingId.StepIds.CurrentProgressStepId;
    }

    public void ActionClose()
    {
        IsOnboardingClosed = true;
    }

    public void ActionToggleVisibility()
    {
        IsOnboardingClosed = !IsOnboardingClosed;
    }

    public Dictionary<string, string> GetAndUpdateOnboardingState()
    {
        var onboardingStatesValues = new Dictionary<string, string>();
        var progressStepsToConsolidate = Env.GetModel("Onboarding.ProgressStep");

        foreach (var step in OnboardingId.StepIds)
        {
            var stepState = step.CurrentStepState;
            if (stepState == "JustDone")
            {
                progressStepsToConsolidate |= step.CurrentProgressStepId;
            }
            onboardingStatesValues[step.Id] = stepState;
        }

        progressStepsToConsolidate.ActionConsolidateJustDone();

        if (IsOnboardingClosed)
        {
            onboardingStatesValues["OnboardingState"] = "Closed";
        }
        else if (OnboardingState == "Done")
        {
            onboardingStatesValues["OnboardingState"] = progressStepsToConsolidate.Any() ? "JustDone" : "Done";
        }

        return onboardingStatesValues;
    }
}
