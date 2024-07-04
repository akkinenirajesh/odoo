csharp
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MailActivity
{
    public virtual void ComputeHasRecommendedActivities()
    {
        this.HasRecommendedActivities = this.PreviousActivityTypeId != null && this.PreviousActivityTypeId.SuggestedNextTypeIds.Any();
    }

    public virtual void ComputeDateDone()
    {
        if (this.Active)
        {
            this.DateDone = null;
        }
        else
        {
            this.DateDone = DateTime.Now;
        }
    }

    public virtual void ComputeResName()
    {
        this.ResName = this.ResModel != null ? Env.GetModel(this.ResModel).Get(this.ResId).DisplayName : null;
    }

    public virtual void ComputeState()
    {
        if (this.DateDeadline != null)
        {
            if (!this.Active)
            {
                this.State = "Done";
            }
            else
            {
                this.State = ComputeStateFromDate(this.DateDeadline, Env.User.TimeZone);
            }
        }
    }

    public virtual string ComputeStateFromDate(DateTime dateDeadline, string timeZone)
    {
        DateTime today = DateTime.Now;
        if (!string.IsNullOrEmpty(timeZone))
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        TimeSpan diff = dateDeadline - today;
        if (diff.Days == 0)
        {
            return "Today";
        }
        else if (diff.Days < 0)
        {
            return "Overdue";
        }
        else
        {
            return "Planned";
        }
    }

    public virtual void ComputeCanWrite()
    {
        // write / unlink: valid for creator / assigned
        this.CanWrite = (Env.User == this.UserId) || (this.UserId == Env.User);
    }

    public virtual void OnChangeActivityTypeId()
    {
        if (this.ActivityTypeId != null)
        {
            if (!string.IsNullOrEmpty(this.ActivityTypeId.Summary))
            {
                this.Summary = this.ActivityTypeId.Summary;
            }
            this.DateDeadline = this.ActivityTypeId.GetDateDeadline();
            this.UserId = this.ActivityTypeId.DefaultUserId != null ? this.ActivityTypeId.DefaultUserId : Env.User;
            if (!string.IsNullOrEmpty(this.ActivityTypeId.DefaultNote))
            {
                this.Note = this.ActivityTypeId.DefaultNote;
            }
        }
    }

    public virtual void OnChangeRecommendedActivityTypeId()
    {
        if (this.RecommendedActivityTypeId != null)
        {
            this.ActivityTypeId = this.RecommendedActivityTypeId;
        }
    }

    // Override create method to send notifications
    public virtual void Create(Dictionary<string, object> vals)
    {
        base.Create(vals);

        // Send notification to assigned user
        if (this.UserId != Env.User)
        {
            if (!Env.Context.ContainsKey("mail_activity_quick_update"))
            {
                ActionNotify();
            }
        }

        // Subscribe user to the related record
        if (this.UserId != null)
        {
            Env.GetModel(this.ResModel).Get(this.ResId).MessageSubscribe(new List<int> { this.UserId.PartnerId });
        }

        // Send bus notifications for todo activities
        if (this.DateDeadline <= DateTime.Now.Date)
        {
            Env.GetModel("Bus.Bus").SendMany(new List<object[]> {
                new object[] { this.UserId.PartnerId, "mail.activity/updated", new Dictionary<string, object> { { "activity_created", true } } }
            });
        }
    }

    // Override write method to send notifications
    public virtual void Write(Dictionary<string, object> values)
    {
        base.Write(values);

        // Send notification to the new assigned user
        if (values.ContainsKey("UserId") && (int)values["UserId"] != Env.User.Id)
        {
            if (!Env.Context.ContainsKey("mail_activity_quick_update"))
            {
                ActionNotify();
            }
            Env.GetModel(this.ResModel).Get(this.ResId).MessageSubscribe(new List<int> { (int)values["UserId"] });
        }

        // Send bus notifications for todo activities
        if (values.ContainsKey("UserId") && (this.DateDeadline <= DateTime.Now.Date))
        {
            Env.GetModel("Bus.Bus").SendMany(new List<object[]> {
                new object[] { this.UserId.PartnerId, "mail.activity/updated", new Dictionary<string, object> { { "activity_created", true } } }
            });
        }
    }

    // Override unlink method to send notifications
    public virtual void Unlink()
    {
        if (this.DateDeadline <= DateTime.Now.Date)
        {
            Env.GetModel("Bus.Bus").SendMany(new List<object[]> {
                new object[] { this.UserId.PartnerId, "mail.activity/updated", new Dictionary<string, object> { { "activity_deleted", true } } }
            });
        }

        base.Unlink();
    }

    public virtual void ActionNotify()
    {
        if (this.UserId.Lang != null)
        {
            Env.SetContext(new Dictionary<string, object> { { "lang", this.UserId.Lang } });
        }

        string modelDescription = Env.GetModel("Ir.Model").Get(this.ResModel).DisplayName;
        string body = Env.GetModel("Ir.Qweb").Render("mail.message_activity_assigned", new Dictionary<string, object>
        {
            { "activity", this },
            { "model_description", modelDescription }
        }, true);

        if (this.UserId != null)
        {
            Env.GetModel(this.ResModel).Get(this.ResId).MessageNotify(
                new List<int> { this.UserId.PartnerId },
                body,
                this.ResName,
                modelDescription,
                "mail.mail_notification_layout",
                string.Format("“{0}: {1}” assigned to you", this.ResName, this.Summary ?? this.ActivityTypeId.Name),
                new List<string> { string.Format("Activity: {0}", this.ActivityTypeId.Name), string.Format("Deadline: {0}", this.DateDeadline.ToString("yyyy-MM-dd")) });
        }
    }

    public virtual void ActionDone()
    {
        ActionFeedback();
    }

    public virtual void ActionDoneRedirectToOther()
    {
        ActionDone();

        var action = Env.GetModel("Ir.Actions.Actions").GetForXmlId("mail.mail_activity_without_access_action");
        Dictionary<string, object> actionContext = action.ContainsKey("context") ? (Dictionary<string, object>)action["context"] : new Dictionary<string, object>();
        if (Env.Context.ContainsKey("active_model") && Env.Context["active_model"] == "mail.activity")
        {
            if (Env.Context.ContainsKey("active_ids"))
            {
                actionContext["active_ids"] = Env.Context["active_ids"];
            }
        }
        else
        {
            // Recompute activities for which user has no access to the underlying record
            var activityGroups = Env.GetModel("Res.Users").GetActivityGroups();
            var activityModelId = Env.GetModel("Ir.Model").GetId("mail.activity");
            actionContext["active_ids"] = activityGroups.FirstOrDefault(g => g.ContainsKey("id") && (int)g["id"] == activityModelId)?.ContainsKey("activity_ids") == true ? (List<int>)activityGroups.FirstOrDefault(g => g.ContainsKey("id") && (int)g["id"] == activityModelId)["activity_ids"] : new List<int>();
        }
        actionContext["active_model"] = "mail.activity";
        action["context"] = actionContext;

        // Return action
        return action;
    }

    public virtual void ActionFeedback(string feedback = null, List<int> attachmentIds = null)
    {
        Env.SetContext(new Dictionary<string, object> { { "clean_context", true } });

        // Call private method _ActionDone to mark activity as done
        var messages = _ActionDone(feedback, attachmentIds);
    }

    public virtual void ActionDoneScheduleNext()
    {
        ActionFeedbackScheduleNext();
    }

    public virtual void ActionFeedbackScheduleNext(string feedback = null, List<int> attachmentIds = null)
    {
        Dictionary<string, object> ctx = new Dictionary<string, object> {
            { "clean_context", true },
            { "default_previous_activity_type_id", this.ActivityTypeId.Id },
            { "activity_previous_deadline", this.DateDeadline },
            { "default_res_id", this.ResId },
            { "default_res_model", this.ResModel }
        };

        // Call private method _ActionDone to mark activity as done
        var _messages = _ActionDone(feedback, attachmentIds);

        // Return an action to create a new activity
        return new Dictionary<string, object> {
            { "name", "Schedule an Activity" },
            { "context", ctx },
            { "view_mode", "form" },
            { "res_model", "mail.activity" },
            { "views", new List<object> { new object[] { false, "form" } } },
            { "type", "ir.actions.act_window" },
            { "target", "new" }
        };
    }

    // Private method to mark activity as done
    private List<int> _ActionDone(string feedback, List<int> attachmentIds)
    {
        var messages = new List<int>();

        // Search for attachments linked to the activity
        var attachments = Env.GetModel("Ir.Attachment").SearchRead(new List<object[]> {
            new object[] { "res_model", "mail.activity" },
            new object[] { "res_id", "in", new List<int> { this.Id } }
        }, new List<string> { "id", "res_id" });

        Dictionary<int, List<int>> activityAttachments = new Dictionary<int, List<int>>();
        foreach (var attachment in attachments)
        {
            int activityId = (int)attachment["res_id"];
            if (!activityAttachments.ContainsKey(activityId))
            {
                activityAttachments.Add(activityId, new List<int>());
            }
            activityAttachments[activityId].Add((int)attachment["id"]);
        }

        // Mark the activity as done
        Env.GetModel(this.ResModel).Get(this.ResId).MessagePostWithSource(
            "mail.message_activity_done",
            attachmentIds,
            Env.User.PartnerId,
            new Dictionary<string, object>
            {
                { "activity", this },
                { "feedback", feedback },
                { "display_assignee", this.UserId != Env.User }
            },
            this.ActivityTypeId.Id,
            "mail.mt_activities");

        // Keep done activities
        if (this.ActivityTypeId.KeepDone)
        {
            if (attachmentIds == null)
            {
                attachmentIds = new List<int>();
            }
            if (activityAttachments.ContainsKey(this.Id))
            {
                attachmentIds.AddRange(activityAttachments[this.Id]);
            }
            this.AttachmentIds = attachmentIds;
        }

        // Unlink activity
        if (!this.ActivityTypeId.KeepDone)
        {
            this.Unlink();
        }
        else
        {
            // Archive activity
            this.Active = false;
        }

        return messages;
    }

    public virtual void ActionCloseDialog()
    {
        return new Dictionary<string, object> { { "type", "ir.actions.act_window_close" } };
    }

    public virtual void ActionOpenDocument()
    {
        return new Dictionary<string, object> {
            { "res_id", this.ResId },
            { "res_model", this.ResModel },
            { "target", "current" },
            { "type", "ir.actions.act_window" },
            { "view_mode", "form" }
        };
    }

    public virtual void ActionSnooze()
    {
        DateTime today = DateTime.Now.Date;
        this.DateDeadline = this.DateDeadline > today ? this.DateDeadline : today.AddDays(7);
    }

    public virtual Dictionary<string, object> ActivityFormat()
    {
        return Env.GetModel("Mail.Discuss.Store").GetResult(this);
    }

    public virtual void ToStore(Mail.Discuss.Store store)
    {
        var activities = Env.Read(this);

        // Fetch mail templates and attachments
        this.MailTemplateIds.Fetch(new List<string> { "name" });
        this.AttachmentIds.Fetch(new List<string> { "name" });

        // Add persona to store
        store.Add("Persona", this.UserId.PartnerId.MailPartnerFormat().Values.ToList());

        // Add activities to store
        for (int i = 0; i < activities.Count; i++)
        {
            activities[i]["mail_template_ids"] = this.MailTemplateIds.Select(mailTemplate => new Dictionary<string, object>
            {
                { "id", mailTemplate.Id },
                { "name", mailTemplate.Name }
            }).ToList();

            activities[i]["attachment_ids"] = this.AttachmentIds.Select(attachment => new Dictionary<string, object>
            {
                { "id", attachment.Id },
                { "name", attachment.Name }
            }).ToList();

            activities[i]["persona"] = new Dictionary<string, object> { { "id", this.UserId.PartnerId.Id }, { "type", "partner" } };
            store.Add("Activity", activities[i]);
        }
    }

    public virtual Dictionary<string, object> GetActivityData(string resModel, List<object[]> domain, int limit = 0, int offset = 0, bool fetchDone = false)
    {
        // Fetch activity types
        var activityTypes = Env.GetModel("Mail.ActivityType").Search(new List<object[]> {
            new object[] { "res_model", "in", new List<string> { resModel, null } }
        });

        // Fetch all ongoing and completed activities
        var activityDomain = new List<object[]> {
            new object[] { "res_model", "=", resModel }
        };
        if (domain != null || limit > 0 || offset > 0)
        {
            activityDomain.Add(new object[] { "res_id", "in", Env.GetModel(resModel).Search(domain ?? new List<object[]>(), offset, limit) });
        }

        // Fetch completed activities
        var allActivities = Env.GetModel("Mail.Activity").WithContext(new Dictionary<string, object> { { "active_test", !fetchDone } }).Search(activityDomain, "date_done DESC, date_deadline ASC");
        var allOngoing = allActivities.Where(act => act.Active).ToList();
        var allCompleted = allActivities.Where(act => !act.Active).ToList();

        // Group activities per record and activity type
        Dictionary<Tuple<int, int>, List<MailActivity>> groupedCompleted = allCompleted.GroupBy(a => new Tuple<int, int>(a.ResId, a.ActivityTypeId.Id)).ToDictionary(group => group.Key, group => group.ToList());
        Dictionary<Tuple<int, int>, List<MailActivity>> groupedOngoing = allOngoing.GroupBy(a => new Tuple<int, int>(a.ResId, a.ActivityTypeId.Id)).ToDictionary(group => group.Key, group => group.ToList());

        // Filter out unreadable records
        List<Tuple<int, int>> resIdTypeTuples = groupedOngoing.Keys.Union(groupedCompleted.Keys).ToList();
        if (domain == null && limit == 0 && offset == 0)
        {
            var filtered = Env.GetModel(resModel).Search(new List<object[]> {
                new object[] { "id", "in", resIdTypeTuples.Select(r => r.Item1).ToList() }
            }).ToList();
            resIdTypeTuples = resIdTypeTuples.Where(r => filtered.Contains(r.Item1)).ToList();
        }

        // Format data
        Dictionary<int, DateTime> resIdToDateDone = new Dictionary<int, DateTime>();
        Dictionary<int, DateTime> resIdToDeadline = new Dictionary<int, DateTime>();
        Dictionary<int, Dictionary<int, Dictionary<string, object>>> groupedActivities = new Dictionary<int, Dictionary<int, Dictionary<string, object>>>();
        foreach (var resIdTuple in resIdTypeTuples)
        {
            int resId = resIdTuple.Item1;
            int activityTypeId = resIdTuple.Item2;
            var ongoing = groupedOngoing.ContainsKey(resIdTuple) ? groupedOngoing[resIdTuple] : new List<MailActivity>();
            var completed = groupedCompleted.ContainsKey(resIdTuple) ? groupedCompleted[resIdTuple] : new List<MailActivity>();
            var activities = ongoing.Union(completed).ToList();

            // Get date done and deadline
            DateTime dateDone = completed.Any() ? completed[0].DateDone : DateTime.MinValue;
            DateTime dateDeadline = ongoing.Any() ? ongoing[0].DateDeadline : DateTime.MaxValue;
            if (dateDeadline != DateTime.MaxValue && (!resIdToDeadline.ContainsKey(resId) || dateDeadline < resIdToDeadline[resId]))
            {
                resIdToDeadline[resId] = dateDeadline;
            }
            if (dateDone != DateTime.MinValue && (!resIdToDateDone.ContainsKey(resId) || dateDone > resIdToDateDone[resId]))
            {
                resIdToDateDone[resId] = dateDone;
            }

            // Get user assigned IDs and attachments
            var userAssignedIds = ongoing.Select(a => a.UserId.Id).ToList();
            var attachments = completed.SelectMany(a => a.AttachmentIds).Select(attach => Env.GetModel("Ir.Attachment").Get(attach.Id)).ToList();

            // Add grouped activities
            if (!groupedActivities.ContainsKey(resId))
            {
                groupedActivities.Add(resId, new Dictionary<int, Dictionary<string, object>>());
            }
            groupedActivities[resId][activityTypeId] = new Dictionary<string, object>
            {
                {
                    "count_by_state",
                    activities.GroupBy(act => act.Active ? ComputeStateFromDate(act.DateDeadline, Env.User.TimeZone) : "Done").ToDictionary(group => group.Key, group => group.Count())
                },
                { "ids", activities.Select(a => a.Id).ToList() },
                { "reporting_date", ongoing.Any() ? dateDeadline : dateDone },
                { "state", ongoing.Any() ? ComputeStateFromDate(dateDeadline, Env.User.TimeZone) : "Done" },
                { "user_assigned_ids", userAssignedIds },
            };

            // Add attachment information
            if (attachments.Any())
            {
                var mostRecentAttachment = attachments.OrderByDescending(a => (a.CreateDate, a.Id)).First();
                groupedActivities[resId][activityTypeId]["attachments_info"] = new Dictionary<string, object>
                {
                    { "most_recent_id", mostRecentAttachment.Id },
                    { "most_recent_name", mostRecentAttachment.Name },
                    { "count", attachments.Count }
                };
            }
        }

        // Sort record IDs by deadline
        List<int> ongoingResIds = resIdToDeadline.Keys.OrderBy(item => resIdToDeadline[item]).ToList();
        // Sort record IDs by date done
        List<int> completedResIds = resIdToDateDone.Keys.Where(resId => !resIdToDeadline.ContainsKey(resId)).OrderByDescending(item => resIdToDateDone[item]).ToList();

        // Return activity data
        return new Dictionary<string, object>
        {
            { "activity_res_ids", ongoingResIds.Concat(completedResIds).ToList() },
            { "activity_types", activityTypes.Select(activityType => new Dictionary<string, object>
            {
                { "id", activityType.Id },
                { "keep_done", activityType.KeepDone },
                { "name", activityType.Name },
                { "template_ids", activityType.MailTemplateIds.Select(mailTemplateId => new Dictionary<string, object>
                {
                    { "id", mailTemplateId.Id },
                    { "name", mailTemplateId.Name }
                }).ToList()
            }).ToList() },
            { "grouped_activities", groupedActivities }
        };
    }

    public virtual Dictionary<string, object> _ClassifyByModel()
    {
        Dictionary<string, Dictionary<string, object>> dataByModel = new Dictionary<string, Dictionary<string, object>>();
        foreach (var activity in this.Where(act => act.ResModel != null && act.ResId != null).ToList())
        {
            if (!dataByModel.ContainsKey(activity.ResModel))
            {
                dataByModel.Add(activity.ResModel, new Dictionary<string, object>
                {
                    { "activities", new List<MailActivity>() },
                    { "record_ids", new List<int>() }
                });
            }
            ((List<MailActivity>)dataByModel[activity.ResModel]["activities"]).Add(activity);
            ((List<int>)dataByModel[activity.ResModel]["record_ids"]).Add(activity.ResId);
        }
        return dataByModel;
    }

    public virtual Dictionary<string, object> _PrepareNextActivityValues()
    {
        Dictionary<string, object> vals = Env.DefaultGet(this.FieldsGet());
        vals.Add("PreviousActivityTypeId", this.ActivityTypeId.Id);
        vals.Add("ResId", this.ResId);
        vals.Add("ResModel", this.ResModel);
        vals.Add("ResModelId", Env.GetModel("Ir.Model").Get(this.ResModel).Id);

        var virtualActivity = Env.New(vals, this.GetType());
        virtualActivity.OnChangePreviousActivityTypeId();
        virtualActivity.OnChangeActivityTypeId();

        return virtualActivity._ConvertToWrite(virtualActivity._Cache);
    }

    public virtual void GCDeleteOldOverdueActivities()
    {
        int yearThreshold = Convert.ToInt32(Env.GetModel("Ir.Config.Parameter").GetParam("mail.activity.gc.delete_overdue_years", "0"));
        if (yearThreshold == 0)
        {
            Console.WriteLine("The ir.config_parameter 'mail.activity.gc.delete_overdue_years' is missing or set to 0. Skipping gc routine.");
            return;
        }
        if (yearThreshold < 0)
        {
            Console.WriteLine("The ir.config_parameter 'mail.activity.gc.delete_overdue_years' is set to a negative number which is invalid. Skipping gc routine.");
            return;
        }

        DateTime deadlineThresholdDt = DateTime.Now.AddYears(-yearThreshold);
        var oldOverdueActivities = Env.GetModel("Mail.Activity").Search(new List<object[]> {
            new object[] { "date_deadline", "<", deadlineThresholdDt }
        }, 10000);
        oldOverdueActivities.Unlink();
    }
}
