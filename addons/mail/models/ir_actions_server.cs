csharp
public partial class MailServerActions
{
    public virtual void ComputeAvailableModelIds()
    {
        if (this.State == "mail_post" || this.State == "followers" || this.State == "remove_followers" || this.State == "next_activity")
        {
            var mailModels = Env.GetModel("ir.model").Search(new[] { new[] { "is_mail_thread", "=", true }, new[] { "transient", "=", false } });
            this.AvailableModelIds = mailModels.Ids;
        }
        else
        {
            base.ComputeAvailableModelIds();
        }
    }

    public virtual void ComputeTemplateId()
    {
        if (this.State != "mail_post" || (this.ModelId != this.TemplateId.ModelId))
        {
            this.TemplateId = null;
        }
    }

    public virtual void ComputeMailPostAutofollow()
    {
        if (this.State != "mail_post" || this.MailPostMethod == "email")
        {
            this.MailPostAutofollow = false;
        }
        else
        {
            this.MailPostAutofollow = true;
        }
    }

    public virtual void ComputeMailPostMethod()
    {
        if (this.State != "mail_post")
        {
            this.MailPostMethod = null;
        }
        else
        {
            this.MailPostMethod = "comment";
        }
    }

    public virtual void ComputePartnerIds()
    {
        if (this.State != "followers")
        {
            this.PartnerIds = null;
        }
    }

    public virtual void ComputeActivityTypeId()
    {
        if (this.State != "next_activity" || (this.ModelId.Model != this.ActivityTypeId.ResModel))
        {
            this.ActivityTypeId = null;
        }
    }

    public virtual void ComputeActivityInfo()
    {
        if (this.State != "next_activity")
        {
            this.ActivitySummary = null;
            this.ActivityNote = null;
            this.ActivityDateDeadlineRange = 0;
            this.ActivityDateDeadlineRangeType = null;
            this.ActivityUserType = null;
            this.ActivityUserId = null;
            this.ActivityUserFieldName = null;
        }
        else
        {
            if (this.ActivitySummary == null)
            {
                this.ActivitySummary = this.ActivityTypeId.Summary;
            }
            if (this.ActivityDateDeadlineRangeType == null)
            {
                this.ActivityDateDeadlineRangeType = "days";
            }
            if (this.ActivityUserType == null)
            {
                this.ActivityUserType = "specific";
            }
            if (this.ActivityUserFieldName == null)
            {
                this.ActivityUserFieldName = "user_id";
            }
        }
    }

    public virtual void CheckActivityDateDeadlineRange()
    {
        if (this.ActivityDateDeadlineRange < 0)
        {
            throw new System.Exception("The 'Due Date In' value can't be negative.");
        }
    }

    public virtual void CheckMailTemplateModel()
    {
        if (this.State == "mail_post")
        {
            if (this.TemplateId != null && this.TemplateId.ModelId != this.ModelId)
            {
                throw new System.Exception($"Mail template model of {this.Name} does not match action model.");
            }
        }
    }

    public virtual void CheckMailModelCoherency()
    {
        if (this.State == "mail_post" || this.State == "followers" || this.State == "remove_followers" || this.State == "next_activity")
        {
            if (this.ModelId.Transient)
            {
                throw new System.Exception("This action cannot be done on transient models.");
            }
            if (this.State == "mail_post" || this.State == "followers" || this.State == "remove_followers")
            {
                if (!this.ModelId.IsMailThread)
                {
                    throw new System.Exception("This action can only be done on a mail thread models");
                }
            }
            if (this.State == "next_activity")
            {
                if (!this.ModelId.IsMailActivity)
                {
                    throw new System.Exception("A next activity can only be planned on models that use activities.");
                }
            }
        }
    }

    public virtual bool RunActionFollowersMulti()
    {
        if (this.PartnerIds != null)
        {
            var model = Env.GetModel(this.ModelName);
            if (model.HasMethod("MessageSubscribe"))
            {
                var records = model.Browse(Env.Context.ActiveIds);
                records.MessageSubscribe(this.PartnerIds.Ids);
            }
        }
        return false;
    }

    public virtual bool RunActionRemoveFollowersMulti()
    {
        if (this.PartnerIds != null)
        {
            var model = Env.GetModel(this.ModelName);
            if (model.HasMethod("MessageUnsubscribe"))
            {
                var records = model.Browse(Env.Context.ActiveIds);
                records.MessageUnsubscribe(this.PartnerIds.Ids);
            }
        }
        return false;
    }

    public virtual bool IsRecompute()
    {
        var records = Env.GetModel(this.ModelName).Browse(Env.Context.ActiveIds);
        var oldValues = Env.Context.OldValues;
        if (oldValues != null)
        {
            var domainPost = Env.Context.DomainPost;
            var trackedFields = new List<string>();
            if (domainPost != null)
            {
                foreach (var leaf in domainPost)
                {
                    if (leaf is List<object> || leaf is Tuple<object, object>)
                    {
                        trackedFields.Add((string)leaf[0]);
                    }
                }
            }
            var fieldsToCheck = oldValues.SelectMany(v => v.Value.Where(f => !trackedFields.Contains(f))).ToList();
            if (fieldsToCheck.Count > 0)
            {
                var field = records.GetField(fieldsToCheck[0]);
                // Pick an arbitrary field; if it is marked to be recomputed,
                // it means we are in an extraneous write triggered by the recompute.
                // In this case, we should not create a new activity.
                if (records.Intersect(Env.RecordsToCompute(field)).Any())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public virtual bool RunActionMailPostMulti()
    {
        if (this.TemplateId == null || (Env.Context.ActiveIds == null && Env.Context.ActiveId == null) || IsRecompute())
        {
            return false;
        }
        var resIds = Env.Context.ActiveIds ?? new List<long> { Env.Context.ActiveId };

        // Clean context from default_type to avoid making attachment
        // with wrong values in subsequent operations
        var cleanedCtx = new Dictionary<string, object>(Env.Context);
        cleanedCtx.Remove("default_type");
        cleanedCtx.Remove("default_parent_id");
        cleanedCtx.Add("mail_create_nosubscribe", true); // do not subscribe random people to records
        cleanedCtx.Add("mail_post_autofollow", this.MailPostAutofollow);

        if (this.MailPostMethod == "comment" || this.MailPostMethod == "note")
        {
            var records = Env.GetModel(this.ModelName).WithContext(cleanedCtx).Browse(resIds);
            if (this.MailPostMethod == "comment")
            {
                var subtypeId = Env.GetModel("ir.model.data")._xmlidToResId("mail.mt_comment");
                records.MessagePostWithSource(this.TemplateId, subtypeId);
            }
            else
            {
                var subtypeId = Env.GetModel("ir.model.data")._xmlidToResId("mail.mt_note");
                records.MessagePostWithSource(this.TemplateId, subtypeId);
            }
        }
        else
        {
            var template = this.TemplateId.WithContext(cleanedCtx);
            foreach (var resId in resIds)
            {
                template.SendMail(resId, forceSend: false, raiseException: false);
            }
        }
        return false;
    }

    public virtual bool RunActionNextActivity()
    {
        if (this.ActivityTypeId == null || Env.Context.ActiveId == null || IsRecompute())
        {
            return false;
        }

        var records = Env.GetModel(this.ModelName).Browse(Env.Context.ActiveIds);

        var vals = new Dictionary<string, object>
        {
            { "summary", this.ActivitySummary ?? "" },
            { "note", this.ActivityNote ?? "" },
            { "activity_type_id", this.ActivityTypeId.Id },
        };
        if (this.ActivityDateDeadlineRange > 0)
        {
            vals.Add("date_deadline", DateTime.Now.Date.AddDays(this.ActivityDateDeadlineRange));
        }
        foreach (var record in records)
        {
            User user = null;
            if (this.ActivityUserType == "specific")
            {
                user = this.ActivityUserId;
            }
            else if (this.ActivityUserType == "generic" && record.HasField(this.ActivityUserFieldName))
            {
                user = record.GetField<User>(this.ActivityUserFieldName);
            }
            if (user != null)
            {
                vals.Add("user_id", user.Id);
            }
            record.ActivitySchedule(vals);
        }
        return false;
    }

    public virtual Dictionary<string, object> GetEvalContext()
    {
        var evalContext = base.GetEvalContext();
        var ctx = new Dictionary<string, object>(evalContext["env"].Context);
        ctx.Add("mail_notify_force_send", false);
        evalContext["env"].Context = ctx;
        return evalContext;
    }
}
