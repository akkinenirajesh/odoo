C#
public partial class ProjectTask
{
    public int Active { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public int Sequence { get; set; }
    public int StageId { get; set; }
    public List<int> TagIds { get; set; }
    public string State { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime WriteDate { get; set; }
    public DateTime DateEnd { get; set; }
    public DateTime DateAssign { get; set; }
    public DateTime DateDeadline { get; set; }
    public DateTime DateLastStageUpdate { get; set; }
    public int ProjectId { get; set; }
    public bool DisplayInProject { get; set; }
    public bool ShowDisplayInProject { get; set; }
    public List<int> UserIds { get; set; }
    public string PortalUserNames { get; set; }
    public List<int> PersonalStageTypeIds { get; set; }
    public int PersonalStageId { get; set; }
    public int PersonalStageTypeId { get; set; }
    public int PartnerId { get; set; }
    public string EmailCc { get; set; }
    public int CompanyId { get; set; }
    public int Color { get; set; }
    public bool RatingActive { get; set; }
    public List<int> AttachmentIds { get; set; }
    public int DisplayedImageId { get; set; }
    public int ParentId { get; set; }
    public List<int> ChildIds { get; set; }
    public int SubtaskCount { get; set; }
    public int ClosedSubtaskCount { get; set; }
    public string ProjectPrivacyVisibility { get; set; }
    public double WorkingHoursOpen { get; set; }
    public double WorkingHoursClose { get; set; }
    public double WorkingDaysOpen { get; set; }
    public double WorkingDaysClose { get; set; }
    public bool AllowMilestones { get; set; }
    public int MilestoneId { get; set; }
    public bool HasLateAndUnreachedMilestone { get; set; }
    public bool AllowTaskDependencies { get; set; }
    public List<int> DependOnIds { get; set; }
    public int DependOnCount { get; set; }
    public List<int> DependentIds { get; set; }
    public int DependentTasksCount { get; set; }
    public bool DisplayParentTaskButton { get; set; }
    public bool CurrentUserSameCompanyPartner { get; set; }
    public bool DisplayFollowButton { get; set; }
    public bool RecurringTask { get; set; }
    public int RecurringCount { get; set; }
    public int RecurrenceId { get; set; }
    public int RepeatInterval { get; set; }
    public string RepeatUnit { get; set; }
    public string RepeatType { get; set; }
    public DateTime RepeatUntil { get; set; }
    public int AnalyticAccountId { get; set; }
    public string DisplayName { get; set; }

    private int _getDefaultStageId()
    {
        var projectId = Env.Context.Get("default_project_id");
        if (projectId == 0)
        {
            return 0;
        }
        return StageFind(projectId, "[('Fold', '=', False)]");
    }

    private int StageFind(int sectionId, string domain = "", string order = "Sequence, Id")
    {
        // collect all section_ids
        var sectionIds = new List<int>();
        if (sectionId != 0)
        {
            sectionIds.Add(sectionId);
        }
        sectionIds.AddRange(this.ProjectId != 0 ? new List<int> { this.ProjectId } : new List<int>());
        var searchDomain = new List<string>();
        if (sectionIds.Count > 0)
        {
            searchDomain.AddRange(Enumerable.Repeat("|", sectionIds.Count - 1));
            foreach (var sectionId in sectionIds)
            {
                searchDomain.Add($"('ProjectIds', '=', {sectionId})");
            }
        }
        searchDomain.AddRange(domain.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)));
        // perform search, return the first found
        return Env.Model<Project.TaskType>().Search(searchDomain, order).FirstOrDefault().Id;
    }

    private List<int> _readGroupStageIds(List<int> stages, string domain)
    {
        var searchDomain = new List<string> { "('Id', 'in', stages)" };
        if (Env.Context.Get("default_project_id") != 0 && !Env.Context.Get("subtask_action", false))
        {
            searchDomain = new List<string> { $"('|', ('ProjectIds', '=', {Env.Context.Get("default_project_id")}))" }.Concat(searchDomain).ToList();
        }
        return Env.Model<Project.TaskType>().Search(searchDomain, Env.Model<Project.TaskType>().Order).ToList();
    }

    private List<int> _readGroupPersonalStageTypeIds(List<int> stages, string domain)
    {
        return Env.Model<Project.TaskType>().Search($"('|', ('Id', 'in', stages), ('UserId', '=', Env.User.Id))").ToList();
    }

    private List<int> _defaultUserIds()
    {
        return (Env.Context.Keys.Contains("default_personal_stage_type_ids") || Env.Context.Keys.Contains("default_personal_stage_type_id")) && Env.User != null ? new List<int> { Env.User.Id } : new List<int>();
    }

    private int _defaultCompanyId()
    {
        if (Env.Context.Get("default_project_id") != 0)
        {
            return Env.Model<Project.Project>().Get(Env.Context.Get("default_project_id")).CompanyId;
        }
        return 0;
    }

    private int _defaultPersonalStageTypeId()
    {
        var defaultId = Env.Context.Get("default_personal_stage_type_ids");
        return (defaultId != null ? (List<int>)defaultId : Env.Model<Project.TaskType>().Search($"('UserId', '=', Env.User.Id)", limit: 1).ToList() ?? new List<int> { 0 }).FirstOrDefault();
    }

    private void _computeState()
    {
        var dependentOpenTasks = new List<Project.Task>();
        if (this.AllowTaskDependencies)
        {
            dependentOpenTasks = this.DependOnIds.Where(dependentTask => !(this.State.Equals("1_done") || this.State.Equals("1_canceled"))).ToList();
        }
        // if one of the blocking task is in a blocking state
        if (dependentOpenTasks.Count > 0)
        {
            // here we check that the blocked task is not already in a closed state (if the task is already done we don't put it in waiting state)
            if (!(this.State.Equals("1_done") || this.State.Equals("1_canceled")))
            {
                this.State = "04_waiting_normal";
            }
        }
        // if the task as no blocking dependencies and is in waiting_normal, the task goes back to in progress
        else if (!(this.State.Equals("1_done") || this.State.Equals("1_canceled")))
        {
            this.State = "01_in_progress";
        }
    }

    private void _inverseState()
    {
        var lastTaskIdPerRecurrenceId = Env.Model<Project.TaskRecurrence>().GetLastTaskIdPerRecurrenceId();
        if (this.State.Equals("1_done") || this.State.Equals("1_canceled"))
        {
            if (this.Id == lastTaskIdPerRecurrenceId.GetValueOrDefault(this.RecurrenceId))
            {
                Env.Model<Project.TaskRecurrence>().CreateNextOccurrence(this);
            }
        }
    }

    private void _computeIsClosed()
    {
        this.IsClosed = this.State.Equals("1_done") || this.State.Equals("1_canceled");
    }

    private void _computeProjectId()
    {
        if (this.DisplayInProject && this.ParentId != 0 && this.ProjectId != this.ParentId.ProjectId)
        {
            this.ProjectId = this.ParentId.ProjectId;
        }
    }

    private void _computeDisplayInProject()
    {
        if (!this.DisplayInProject && (this.ProjectId == 0 || (this.ParentId != 0 && this.ProjectId != this.ParentId.ProjectId)))
        {
            this.DisplayInProject = true;
        }
    }

    private void _computeShowDisplayInProject()
    {
        this.ShowDisplayInProject = this.ParentId != 0 && this.ProjectId == this.ParentId.ProjectId;
    }

    private void _computeAnalyticAccountId()
    {
        this.AnalyticAccountId = this.ProjectId != 0 ? this.ProjectId.AnalyticAccountId : 0;
    }

    private void _computePersonalStageId()
    {
        var personalStages = Env.Model<Project.TaskStagePersonal>().Search($"('UserId', '=', Env.User.Id), ('TaskId', 'in', new List<int> {{ this.Id }})");
        foreach (var personalStage in personalStages)
        {
            personalStage.TaskId.PersonalStageId = personalStage.Id;
        }
    }

    private void _computePersonalStageTypeId()
    {
        this.PersonalStageTypeId = this.PersonalStageId != 0 ? this.PersonalStageId.StageId : 0;
    }

    private void _inversePersonalStageTypeId()
    {
        this.PersonalStageId.StageId = this.PersonalStageTypeId;
    }

    private List<string> _searchPersonalStageTypeId(string operator, string value)
    {
        return new List<string> { $"('PersonalStageTypeIds', {operator}, {value})" };
    }

    private void _computePortalUserNames()
    {
        this.PortalUserNames = string.Join(",", this.UserIds.Select(u => u.Name));
    }

    private List<string> _searchPortalUserNames(string operator, string value)
    {
        if (operator != "ilike" || !value.GetType().Equals(typeof(string)))
        {
            throw new Exception("Not Implemented.");
        }
        return new List<string> { $"('Id', 'inselect', ('SELECT task_user.TaskId FROM ProjectTaskUserRel task_user INNER JOIN ResUsers users ON task_user.UserId = users.Id INNER JOIN ResPartner partners ON partners.Id = users.PartnerId WHERE partners.Name ILIKE {value}', new List<string> {{ value }}))" };
    }

    private void _computeSubtaskAllocatedHours()
    {
        this.SubtaskAllocatedHours = this.ChildIds.Sum(child => child.AllocatedHours);
    }

    private void _computeSubtaskCount()
    {
        this.SubtaskCount = this.ChildIds.Count;
        this.ClosedSubtaskCount = this.ChildIds.Count(c => c.State.Equals("1_done") || c.State.Equals("1_canceled"));
    }

    private void _computeCompanyId()
    {
        if (this.ParentId == 0 && this.ProjectId == 0)
        {
            return;
        }
        this.CompanyId = this.ProjectId != 0 ? this.ProjectId.CompanyId : this.ParentId.CompanyId;
    }

    private void _computeAttachmentIds()
    {
        var attachmentIds = Env.Model<ir.Attachment>().Search(this._getAttachmentsSearchDomain()).ToList();
        var messageAttachmentIds = this.MessageIds.SelectMany(m => m.AttachmentIds).ToList();
        this.AttachmentIds = attachmentIds.Where(id => !messageAttachmentIds.Contains(id)).ToList();
    }

    private List<string> _getAttachmentsSearchDomain()
    {
        return new List<string> { $"('ResId', '=', {this.Id})", $"('ResModel', '=', 'project.task')" };
    }

    private void _computeElapsed()
    {
        var taskLinkedToCalendar = this.ProjectId.ResourceCalendarId != null && this.CreateDate != DateTime.MinValue ? this : null;
        if (taskLinkedToCalendar != null)
        {
            var dtCreateDate = DateTime.Parse(this.CreateDate.ToString());
            if (this.DateAssign != DateTime.MinValue)
            {
                var dtDateAssign = DateTime.Parse(this.DateAssign.ToString());
                var durationData = this.ProjectId.ResourceCalendarId.GetWorkDurationData(dtCreateDate, dtDateAssign, computeLeaves: true);
                this.WorkingHoursOpen = durationData.Hours;
                this.WorkingDaysOpen = durationData.Days;
            }
            else
            {
                this.WorkingHoursOpen = 0;
                this.WorkingDaysOpen = 0;
            }
            if (this.DateEnd != DateTime.MinValue)
            {
                var dtDateEnd = DateTime.Parse(this.DateEnd.ToString());
                var durationData = this.ProjectId.ResourceCalendarId.GetWorkDurationData(dtCreateDate, dtDateEnd, computeLeaves: true);
                this.WorkingHoursClose = durationData.Hours;
                this.WorkingDaysClose = durationData.Days;
            }
            else
            {
                this.WorkingHoursClose = 0;
                this.WorkingDaysClose = 0;
            }
        }
        else
        {
            this.WorkingHoursOpen = 0;
            this.WorkingHoursClose = 0;
            this.WorkingDaysOpen = 0;
            this.WorkingDaysClose = 0;
        }
    }

    private void _computeDependOnCount()
    {
        var tasksWithDependency = this.AllowTaskDependencies ? this : null;
        if (tasksWithDependency == null)
        {
            this.DependOnCount = 0;
            return;
        }
        var dependOnCount = Env.Model<Project.Task>().ReadGroup(new List<string> { $"('DependentIds', 'in', new List<int> {{ this.Id }})" }, new List<string> { "DependentIds" }, new List<string> { "__count" })
                .Select(d => (d.DependentIds.FirstOrDefault(), (int)d.Count))
                .ToDictionary(x => x.Item1, x => x.Item2);
        this.DependOnCount = dependOnCount.GetValueOrDefault(this.Id, 0);
    }

    private void _computeDependentTasksCount()
    {
        var tasksWithDependency = this.AllowTaskDependencies ? this : null;
        if (tasksWithDependency == null)
        {
            this.DependentTasksCount = 0;
            return;
        }
        var groupDependent = Env.Model<Project.Task>().ReadGroup(
            new List<string> { $"('DependOnIds', 'in', new List<int> {{ this.Id }})", $"('IsClosed', '=', false)" },
            new List<string> { "DependOnIds" },
            new List<string> { "__count" })
            .Select(d => (d.DependOnIds.FirstOrDefault(), (int)d.Count))
            .ToDictionary(x => x.Item1, x => x.Item2);
        this.DependentTasksCount = groupDependent.GetValueOrDefault(this.Id, 0);
    }

    private void _computeMilestoneId()
    {
        if (this.ProjectId != this.MilestoneId.ProjectId)
        {
            this.MilestoneId = this.ParentId != 0 && this.ProjectId == this.ParentId.ProjectId ? this.ParentId.MilestoneId : 0;
        }
    }

    private void _computeHasLateAndUnreachedMilestone()
    {
        if (!this.AllowMilestones)
        {
            this.HasLateAndUnreachedMilestone = false;
            return;
        }
        var lateMilestones = Env.Model<Project.Milestone>().Search(new List<string> { $"('Id', 'in', new List<int> {{ this.MilestoneId }})", $"('IsReached', '=', false)", $"('Deadline', '<=', DateTime.Now)" });
        this.HasLateAndUnreachedMilestone = this.AllowMilestones && lateMilestones.Any(lm => lm.Id == this.MilestoneId);
    }

    private List<string> _searchHasLateAndUnreachedMilestone(string operator, bool value)
    {
        if (operator != "=" && operator != "!=" || value.GetType().Equals(typeof(bool)))
        {
            throw new Exception("The search does not support operator {operator} or value {value}.");
        }
        var domain = new List<string> { $"('AllowMilestones', '=', true)", $"('MilestoneId', '!=', false)", $"('MilestoneId.IsReached', '=', false)", $"('MilestoneId.Deadline', '!=', false)", $"('MilestoneId.Deadline', '<', DateTime.Now)" };
        if ((operator == "!=" && value) || (operator == "=" && !value))
        {
            domain.Insert(0, "!");
        }
        return domain;
    }

    private void _computeDisplayParentTaskButton()
    {
        this.DisplayParentTaskButton = this.ParentId != 0 && Env.Model<Project.Task>().Get(this.ParentId.Id).CheckAccessRights("read");
    }

    private void _computeCurrentUserSameCompanyPartner()
    {
        this.CurrentUserSameCompanyPartner = this.PartnerId != 0 && Env.User.PartnerId.CommercialPartnerId == this.PartnerId.CommercialPartnerId;
    }

    private void _computeDisplayFollowButton()
    {
        if (!Env.User.Share)
        {
            this.DisplayFollowButton = false;
            return;
        }
        var projectCollaboratorReadGroup = Env.Model<Project.Collaborator>().ReadGroup(
            new List<string> { $"('ProjectId', 'in', new List<int> {{ this.ProjectId }})", $"('PartnerId', '=', Env.User.PartnerId.Id)" },
            new List<string> { "ProjectId" },
            new List<string> { "LimitedAccess" });
        var limitedAccessPerProjectId = projectCollaboratorReadGroup.Select(d => (d.ProjectId.FirstOrDefault(), (bool)d.LimitedAccess)).ToDictionary(x => x.Item1, x => x.Item2);
        this.DisplayFollowButton = !limitedAccessPerProjectId.GetValueOrDefault(this.ProjectId, true);
    }

    private void _computeRecurringCount()
    {
        if (this.RecurrenceId == 0)
        {
            return;
        }
        var recurringTasks = this.RecurrenceId != 0 ? this : null;
        var count = Env.Model<Project.Task>().ReadGroup(new List<string> { $"('RecurrenceId', 'in', new List<int> {{ this.RecurrenceId }})" }, new List<string> { "RecurrenceId" }, new List<string> { "__count" })
                .Select(d => (d.RecurrenceId.FirstOrDefault(), (int)d.Count))
                .ToDictionary(x => x.Item1, x => x.Item2);
        this.RecurringCount = count.GetValueOrDefault(this.RecurrenceId, 0);
    }

    private void _computeRepeat()
    {
        var recFields = new List<string> { "RepeatInterval", "RepeatUnit", "RepeatType", "RepeatUntil" };
        var defaults = this.DefaultGet(recFields);
        foreach (var field in recFields)
        {
            if (this.RecurrenceId != 0)
            {
                this[field] = this.RecurrenceId.GetValueOrDefault(field);
            }
            else
            {
                this[field] = this.RecurringTask ? defaults.GetValueOrDefault(field) : null;
            }
        }
    }

    private void _computeDisplayName()
    {
        // Implementation for _computeDisplayName
    }

    private void _inverseDisplayName()
    {
        // Implementation for _inverseDisplayName
    }

    private void _ensureFieldsAreAccessible(List<string> fields, string operation = "read", bool checkGroupUser = true)
    {
        if (fields.Count > 0 && (!checkGroupUser || Env.User.IsPortal() && !Env.IsSuperUser))
        {
            var unauthorizedFields = fields.Except(operation.Equals("read") ? SelfReadableFields : SelfWritableFields).ToList();
            if (unauthorizedFields.Count > 0)
            {
                var unauthorizedFieldList = string.Join(",", unauthorizedFields);
                throw new AccessError($"You cannot {operation} the following fields on tasks: {unauthorizedFieldList}");
            }
        }
    }

    private List<string> _determineFieldsToFetch(List<string> fieldNames, bool ignoreWhenInCache = false)
    {
        if (!Env.IsSuperUser && Env.User.IsPortal())
        {
            fieldNames = fieldNames.Where(fname => SelfReadableFields.Contains(fname)).ToList();
        }
        return Env.Model<Project.Task>().DetermineFieldsToFetch(fieldNames, ignoreWhenInCache);
    }

    private Dictionary<string, object> _getPortalSudoContext()
    {
        return Env.Context.Where