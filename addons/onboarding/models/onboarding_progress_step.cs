csharp
public partial class OnboardingProgressStep 
{
    public void Init() 
    {
        //Make sure there aren't multiple records for the same onboarding step and company.
        // not in _sql_constraint because COALESCE is not supported for PostgreSQL constraint
        Env.Cr.Execute("""
            CREATE UNIQUE INDEX IF NOT EXISTS onboarding_progress_step_company_uniq
            ON onboarding_progress_step (step_id, COALESCE(company_id, 0))
        """);
    }

    public OnboardingProgressStep ActionConsolidateJustDone()
    {
        var wasJustDone = this.Where(progress => progress.StepState == "JustDone");
        wasJustDone.StepState = "Done";
        return wasJustDone;
    }

    public OnboardingProgressStep ActionSetJustDone()
    {
        var notDone = this.Where(progress => progress.StepState == "NotDone");
        notDone.StepState = "JustDone";
        return notDone;
    }
}
