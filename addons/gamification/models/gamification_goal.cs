csharp
public partial class Goal
{
    public float GetCompletion()
    {
        if (DefinitionCondition == GoalCondition.Higher)
        {
            if (Current >= TargetGoal)
            {
                return 100.0f;
            }
            else
            {
                return TargetGoal != 0 ? (float)Math.Round(100.0 * Current / TargetGoal, 2) : 0;
            }
        }
        else if (Current < TargetGoal)
        {
            return 100.0f;
        }
        else
        {
            return 0.0f;
        }
    }

    public Dictionary<string, object> CheckRemindDelay()
    {
        if (!(RemindUpdateDelay > 0 && LastUpdate.HasValue))
        {
            return new Dictionary<string, object>();
        }

        var deltaMax = TimeSpan.FromDays(RemindUpdateDelay);
        if (DateTime.Today - LastUpdate.Value < deltaMax)
        {
            return new Dictionary<string, object>();
        }

        // Generate a reminder report
        // Note: This is a placeholder for the actual implementation
        // You'll need to implement the equivalent of Odoo's message_notify method
        
        return new Dictionary<string, object> { { "ToUpdate", true } };
    }

    public Dictionary<string, object> GetWriteValues(float newValue)
    {
        if (newValue == Current)
        {
            return new Dictionary<string, object>();
        }

        var result = new Dictionary<string, object> { { "Current", newValue } };
        if ((DefinitionCondition == GoalCondition.Higher && newValue >= TargetGoal) ||
            (DefinitionCondition == GoalCondition.Lower && newValue <= TargetGoal))
        {
            result["State"] = GoalState.Reached;
        }
        else if (EndDate.HasValue && DateTime.Today > EndDate.Value)
        {
            result["State"] = GoalState.Failed;
            result["Closed"] = true;
        }

        return result;
    }

    public void UpdateGoal()
    {
        // Implement the goal update logic here
        // This will involve querying the database and updating the goal values
        // You'll need to implement the equivalent of Odoo's ORM methods
    }

    public void ActionStart()
    {
        State = GoalState.InProgress;
        UpdateGoal();
    }

    public void ActionReach()
    {
        State = GoalState.Reached;
    }

    public void ActionFail()
    {
        State = GoalState.Failed;
    }

    public void ActionCancel()
    {
        State = GoalState.InProgress;
    }

    public Dictionary<string, object> GetAction()
    {
        // Implement the logic to return the appropriate action
        // This will depend on how actions are handled in your C# framework
        return new Dictionary<string, object>();
    }

    public override string ToString()
    {
        return $"Goal: {DefinitionId}";
    }
}
