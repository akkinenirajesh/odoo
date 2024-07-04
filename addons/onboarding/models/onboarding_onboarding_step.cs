C#
public partial class OnboardingStep
{
    public void ComputeCurrentProgress()
    {
        // Implement the logic for computing CurrentProgressStepId and CurrentStepState based on ProgressIds and company context.
        // Use Env to access the company context, and use the filtered_domain functionality to retrieve the relevant records.
    }

    public void CheckStepOnOnboardingHasAction()
    {
        // Implement the logic to raise an error if the OnboardingIds are not empty and the PanelStepOpenActionName is empty.
        // Use the Env to access the current context and retrieve the OnboardingIds.
    }

    public void Write(Dictionary<string, object> vals)
    {
        // Implement the logic to handle the Write operation.
        // This method should first call the base Write method.
        // Then, handle the specific logic for updating IsPerCompany and refreshing the related ProgressIds based on the provided vals.
        // Use Env to access the current context and perform the necessary operations.
    }

    public void ActionSetJustDone()
    {
        // Implement the logic to set the CurrentProgressStepId to "just_done".
        // Use Env to access the current context and retrieve the CurrentProgressStepId.
        // Use the appropriate method to update the CurrentProgressStepId to "just_done".
    }

    public string ActionValidateStep(string xmlId)
    {
        // Implement the logic to validate the step based on the provided xmlId.
        // Use Env to access the current context and retrieve the OnboardingStep object based on the xmlId.
        // Use the ActionSetJustDone method to set the step to "just_done".
        // Return "JUST_DONE" if the step is set to "just_done", "WAS_DONE" if the step was already "done", and "NOT_FOUND" if the step is not found.
    }

    public string GetPlaceholderFilename(string field)
    {
        // Implement the logic to return the placeholder filename for the specified field.
        // If the field is "StepImage", return "base/static/img/onboarding_default.png".
        // Otherwise, return the placeholder filename based on the field name.
    }

    public void CreateProgressSteps()
    {
        // Implement the logic to create ProgressStep records for the current context (company).
        // Use Env to access the current context and retrieve the relevant OnboardingProgress records.
        // Use the appropriate methods to create ProgressStep records for each step.
        // Ensure that the company_id is set correctly based on the IsPerCompany flag.
    }
}
