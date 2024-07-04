csharp
public partial class ProjectTaskRecurrence
{
    public virtual int RepeatInterval { get; set; }
    public virtual string RepeatUnit { get; set; }
    public virtual string RepeatType { get; set; }
    public virtual DateTime RepeatUntil { get; set; }
    public virtual ICollection<ProjectTask> TaskIds { get; set; }

    public void CheckRepeatInterval()
    {
        if (RepeatInterval <= 0)
        {
            throw new Exception("The interval should be greater than 0");
        }
    }

    public void CheckRepeatUntilDate()
    {
        if (RepeatType == "until" && RepeatUntil < Env.Today)
        {
            throw new Exception("The end date should be in the future");
        }
    }

    public ICollection<string> GetRecurringFieldsToCopy()
    {
        return new List<string>
        {
            "RecurrenceId",
        };
    }

    public ICollection<string> GetRecurringFieldsToPostpone()
    {
        return new List<string>
        {
            "DateDeadline",
        };
    }

    public Dictionary<int, int> GetLastTaskIdPerRecurrenceId()
    {
        var result = new Dictionary<int, int>();
        var groups = Env.GetModel("Project.ProjectTask").ReadGroup(
            new[] { new Search("RecurrenceId", "in", new List<int> { this.Id }) },
            new[] { "RecurrenceId" },
            new[] { "Id:max" });
        foreach (var group in groups)
        {
            result.Add((int)group["RecurrenceId"], (int)group["Id:max"]);
        }
        return result;
    }

    public RelativeTimeSpan GetRecurrenceDelta()
    {
        return new RelativeTimeSpan
        {
            {RepeatUnit, RepeatInterval}
        };
    }

    public void CreateNextOccurrence(ProjectTask occurrenceFrom)
    {
        if (RepeatType == "until" && Env.Today > RepeatUntil)
        {
            return;
        }

        occurrenceFrom.Copy(CreateNextOccurrenceValues(occurrenceFrom));
    }

    public Dictionary<string, object> CreateNextOccurrenceValues(ProjectTask occurrenceFrom)
    {
        var fieldsToCopy = occurrenceFrom.Read(GetRecurringFieldsToCopy()).First();
        var createValues = fieldsToCopy.ToDictionary(x => x.Key, x => x.Value);

        var fieldsToPostpone = occurrenceFrom.Read(GetRecurringFieldsToPostpone()).First();
        fieldsToPostpone.Remove("Id");
        foreach (var field in fieldsToPostpone)
        {
            if (field.Value is DateTime)
            {
                createValues[field.Key] = (DateTime)field.Value + GetRecurrenceDelta();
            }
        }

        createValues["Priority"] = "0";
        createValues["StageId"] = occurrenceFrom.ProjectId.TypeIds.FirstOrDefault()?.Id ?? occurrenceFrom.StageId.Id;

        createValues["ChildIds"] = occurrenceFrom.ChildIds.Select(child =>
        {
            var childCopy = child.Copy(CreateNextOccurrenceValues(child));
            return childCopy.Id;
        }).ToList();

        return createValues;
    }
}
