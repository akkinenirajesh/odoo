csharp
public partial class Lead
{
    public override string ToString()
    {
        return Name;
    }

    public void ToggleActive()
    {
        Active = !Active;
        if (Active)
        {
            LostReasonId = null;
            ComputeProbabilities();
        }
        else
        {
            Probability = 0;
            AutomatedProbability = 0;
        }
    }

    public void ActionSetLost(Dictionary<string, object> additionalValues = null)
    {
        ActionArchive();
        if (additionalValues != null)
        {
            // Update additional values
            // Note: In C#, we would typically use properties instead of a dictionary
            // This is just a placeholder to represent the concept
        }
    }

    public void ActionSetWon()
    {
        ActionUnarchive();
        StageId = Env.Crm.Stage.Search(new[] { ("IsWon", "=", true) }).OrderBy(s => s.Sequence).FirstOrDefault();
        Probability = 100;
    }

    public void ActionSetAutomatedProbability()
    {
        Probability = AutomatedProbability;
    }

    public Dictionary<string, object> ActionScheduleMeeting(bool smartCalendar = true)
    {
        // Implementation details would depend on how we handle actions and returns in the C# version
        // This is a placeholder to represent the concept
        return new Dictionary<string, object>();
    }

    public Dictionary<string, object> ActionRescheduleMeeting()
    {
        var action = ActionScheduleMeeting(false);
        var nextActivity = ActivityIds.Where(a => a.UserId == Env.User).FirstOrDefault();
        if (nextActivity?.CalendarEventId != null)
        {
            ((Dictionary<string, object>)action["context"])["initial_date"] = nextActivity.CalendarEventId.Start;
        }
        return action;
    }

    public Dictionary<string, object> ActionShowPotentialDuplicates()
    {
        // Implementation details would depend on how we handle actions and returns in the C# version
        // This is a placeholder to represent the concept
        return new Dictionary<string, object>();
    }

    public void ActionSnooze()
    {
        var myNextActivity = ActivityIds.Where(a => a.UserId == Env.User).FirstOrDefault();
        myNextActivity?.ActionSnooze();
    }

    public Dictionary<string, object> RedirectLeadOpportunityView()
    {
        // Implementation details would depend on how we handle actions and returns in the C# version
        // This is a placeholder to represent the concept
        return new Dictionary<string, object>();
    }

    private void ComputeProbabilities()
    {
        // Implementation of probability computation
        // This would be a complex method that might involve multiple helper methods
    }
}
