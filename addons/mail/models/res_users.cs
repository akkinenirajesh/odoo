csharp
public partial class ResUsers
{
  public void ComputeNotificationType()
  {
    // Because of the `GroupsId` in the `api.depends`,
    // this code will be called for any change of group on a user,
    // even unrelated to the group_mail_notification_type_inbox or share flag.
    // e.g. if you add HR > Manager to a user, this method will be called.
    // It should therefore be written to be as performant as possible, and make the less change/write as possible
    // when it's not `mail.group_mail_notification_type_inbox` or `share` that are being changed.
    int inboxGroupId = Env.Ref("mail.group_mail_notification_type_inbox");

    this.Filtered(u => u.GroupsId.Contains(inboxGroupId) && u.NotificationType != "Inbox")
        .ForEach(u => u.NotificationType = "Inbox");
    this.Filtered(u => !u.GroupsId.Contains(inboxGroupId) && u.NotificationType == "Inbox")
        .ForEach(u => u.NotificationType = "Email");

    // Special case: internal users with inbox notifications converted to portal must be converted to email users
    this.Filtered(u => u.Share && u.NotificationType == "Inbox")
        .ForEach(u => u.NotificationType = "Email");
  }

  public void InverseNotificationType()
  {
    var inboxGroup = Env.Ref("mail.group_mail_notification_type_inbox");
    var inboxUsers = this.Filtered(u => u.NotificationType == "Inbox");
    inboxUsers.ForEach(u => u.GroupsId = new List<int>() { inboxGroup.Id });
    (this - inboxUsers).ForEach(u => u.GroupsId.Remove(inboxGroup.Id));
  }

  // ------------------------------------------------------------
  // CRUD
  // ------------------------------------------------------------

  public ResUsers Create(List<Dictionary<string, object>> valsList)
  {
    var users = base.Create(valsList);

    // log a portal status change (manual tracking)
    bool logPortalAccess = !Env.Context.ContainsKey("mail_create_nolog") && !Env.Context.ContainsKey("mail_notrack");
    if (logPortalAccess)
    {
      users.ForEach(user =>
      {
        if (user.IsPortal())
        {
          string body = user.GetPortalAccessUpdateBody(true);
          user.PartnerId.MessagePost(body: body, messageType: "notification", subtypeXmlid: "mail.mt_note");
        }
      });
    }

    return users;
  }

  public void Write(Dictionary<string, object> vals)
  {
    bool logPortalAccess = vals.ContainsKey("GroupsId") && !Env.Context.ContainsKey("mail_create_nolog") && !Env.Context.ContainsKey("mail_notrack");
    Dictionary<int, bool> userPortalAccessDict = logPortalAccess ? this.ToDictionary(user => user.Id, user => user.IsPortal()) : new Dictionary<int, bool>();

    Dictionary<ResUsers, string> previousEmailByUser = new Dictionary<ResUsers, string>();
    if (vals.ContainsKey("Email"))
    {
      previousEmailByUser = this.Where(user => !string.IsNullOrEmpty(user.Email)).ToDictionary(user => user, user => user.Email);
    }

    ResUsers userNotificationTypeModified = new ResUsers();
    if (vals.ContainsKey("NotificationType"))
    {
      userNotificationTypeModified = this.Filtered(user => user.NotificationType != vals["NotificationType"]);
    }

    base.Write(vals);

    // log a portal status change (manual tracking)
    if (logPortalAccess)
    {
      this.ForEach(user =>
      {
        bool userHasGroup = user.IsPortal();
        bool portalAccessChanged = userHasGroup != userPortalAccessDict[user.Id];
        if (portalAccessChanged)
        {
          string body = user.GetPortalAccessUpdateBody(userHasGroup);
          user.PartnerId.MessagePost(body: body, messageType: "notification", subtypeXmlid: "mail.mt_note");
        }
      });
    }

    if (vals.ContainsKey("Login"))
    {
      this.NotifySecuritySettingUpdate("Security Update: Login Changed", "Your account login has been updated");
    }
    if (vals.ContainsKey("Password"))
    {
      this.NotifySecuritySettingUpdate("Security Update: Password Changed", "Your account password has been updated");
    }
    if (vals.ContainsKey("Email"))
    {
      // when the email is modified, we want notify the previous address (and not the new one)
      foreach (var item in previousEmailByUser)
      {
        this.NotifySecuritySettingUpdate("Security Update: Email Changed", $"Your account email has been changed from {item.Value} to {item.Key.Email}", mailValues: new Dictionary<string, object>() { { "email_to", item.Value } }, suggestPasswordReset: false);
      }
    }
    if (vals.ContainsKey("NotificationType"))
    {
      userNotificationTypeModified.ForEach(user => Env.BusBus.SendOne(user.PartnerId, "mail.record/insert", new Dictionary<string, object>() { { "Persona", new Dictionary<string, object>() { { "id", user.PartnerId.Id }, { "type", "partner" }, { "notification_preference", user.NotificationType } } } }));
    }
  }

  public void ActionArchive()
  {
    var activitiesToDelete = Env.MailActivity.Search(a => a.UserId.Contains(this.Ids));
    activitiesToDelete.Unlink();
    base.ActionArchive();
  }

  public void NotifySecuritySettingUpdate(string subject, string content, Dictionary<string, object> mailValues = null, bool suggestPasswordReset = true)
  {
    var mailCreateValues = new List<Dictionary<string, object>>();
    this.ForEach(user =>
    {
      string bodyHtml = Env.IrQweb.Render("mail.account_security_setting_update", user.NotifySecuritySettingUpdatePrepareValues(content, suggestPasswordReset: suggestPasswordReset), minimalQcontext: true);

      bodyHtml = Env.MailRenderMixin._RenderEncapsulate("mail.mail_notification_light", bodyHtml, addContext: new Dictionary<string, object>()
      {
        { "message", new MailMessage(body: bodyHtml, recordName: user.Name) },
        { "model_description", "Account" },
        { "company", user.CompanyId }
      });

      var vals = new Dictionary<string, object>()
      {
        { "auto_delete", true },
        { "body_html", bodyHtml },
        { "author_id", Env.User.PartnerId.Id },
        { "email_from", user.CompanyId.PartnerId.EmailFormatted ?? Env.User.EmailFormatted ?? Env.Ref("base.user_root").EmailFormatted },
        { "email_to", mailValues != null && mailValues.ContainsKey("force_email") ? mailValues["force_email"] : user.EmailFormatted },
        { "subject", subject }
      };

      if (mailValues != null)
      {
        vals.AddRange(mailValues);
      }

      mailCreateValues.Add(vals);
    });

    Env.MailMail.Create(mailCreateValues);
  }

  public Dictionary<string, object> NotifySecuritySettingUpdatePrepareValues(string content, bool suggestPasswordReset = true)
  {
    bool resetPasswordEnabled = Env.IrConfigParameter.GetParam("auth_signup.reset_password", true);
    return new Dictionary<string, object>()
    {
      { "company", this.CompanyId },
      { "password_reset_url", $"{this.GetBaseUrl()}/web/reset_password" },
      { "security_update_text", content },
      { "suggest_password_reset", suggestPasswordReset && resetPasswordEnabled },
      { "user", this },
      { "update_datetime", DateTime.Now }
    };
  }

  public string GetPortalAccessUpdateBody(bool accessGranted)
  {
    string body = accessGranted ? "Portal Access Granted" : "Portal Access Revoked";
    if (!string.IsNullOrEmpty(this.PartnerId.Email))
    {
      return $"{body} ({this.PartnerId.Email})";
    }

    return body;
  }

  public void DeactivatePortalUser(Dictionary<string, object> post)
  {
    var current_user = Env.User;
    this.ForEach(user =>
    {
      user.PartnerId.MessageLog(body: $"Archived because {current_user.Name} (#{current_user.Id}) deleted the portal account");
    });

    List<(ResUsers user, string userEmail)> usersToBlacklist = new List<(ResUsers user, string userEmail)>();
    if (post.ContainsKey("request_blacklist"))
    {
      usersToBlacklist = this.Where(user => !string.IsNullOrEmpty(user.Email)).Select(user => (user, user.Email)).ToList();
    }

    base.DeactivatePortalUser(post);

    usersToBlacklist.ForEach(item =>
    {
      Env.MailBlacklist.Add(item.userEmail, message: $"Blocked by deletion of portal account {item.user.Name} by {current_user.Name} (#{current_user.Id})");
    });
  }

  // ------------------------------------------------------------
  // DISCUSS
  // ------------------------------------------------------------

  public void InitStoreData(Store store)
  {
    // sudo: res.partner - exposing OdooBot data
    var odoobot = Env.Ref("base.partner_root");
    int xmlidToResid = Env.IrModelData.XmlidToResid("mail.action_discuss");
    store.Add("Persona", odoobot.MailPartnerFormat());
    store.Add(new Dictionary<string, object>()
    {
      { "action_discuss_id", xmlidToResid },
      { "hasLinkPreviewFeature", Env.MailLinkPreview.IsLinkPreviewEnabled() },
      { "internalUserGroupId", Env.Ref("base.group_user").Id },
      { "mt_comment_id", Env.IrModelData.XmlidToResid("mail.mt_comment") },
      { "odoobot", new Dictionary<string, object>() { { "id", odoobot.Id }, { "type", "partner" } } }
    });
    var guest = Env.MailGuest.GetGuestFromContext();
    if (!Env.User.IsPublic())
    {
      var settings = Env.ResUsersSettings.FindOrCreateForUser(Env.User);
      store.Add("Persona", new Dictionary<string, object>()
      {
        { "id", Env.User.PartnerId.Id },
        { "isAdmin", Env.User.IsAdmin() },
        { "isInternalUser", !Env.User.Share },
        { "name", Env.User.PartnerId.Name },
        { "notification_preference", Env.User.NotificationType },
        { "type", "partner" },
        { "userId", Env.User.Id },
        { "write_date", DateTime.Now.ToString() }
      });
      store.Add(new Dictionary<string, object>()
      {
        { "self", new Dictionary<string, object>() { { "id", Env.User.PartnerId.Id }, { "type", "partner" } } },
        { "settings", settings.ResUsersSettingsFormat() }
      });
    }
    else if (guest != null)
    {
      store.Add("Persona", new Dictionary<string, object>()
      {
        { "id", guest.Id },
        { "name", guest.Name },
        { "type", "guest" },
        { "write_date", DateTime.Now.ToString() }
      });
      store.Add(new Dictionary<string, object>() { { "self", new Dictionary<string, object>() { { "id", guest.Id }, { "type", "guest" } } } });
    }
  }

  public void InitMessaging(Store store)
  {
    // sudo: bus.bus: reading non-sensitive last id
    int busLastId = Env.BusBus.BusLastId();
    store.Add(new Dictionary<string, object>()
    {
      {
        "discuss", new Dictionary<string, object>()
        {
          {
            "inbox", new Dictionary<string, object>()
            {
              { "counter", this.PartnerId.GetNeedactionCount() },
              { "counter_bus_id", busLastId },
              { "id", "inbox" },
              { "model", "mail.box" }
            }
          },
          {
            "starred", new Dictionary<string, object>()
            {
              { "counter", Env.MailMessage.SearchCount(m => m.StarredPartnerIds.Contains(this.PartnerId.Id)) },
              { "counter_bus_id", busLastId },
              { "id", "starred" },
              { "model", "mail.box" }
            }
          }
        }
      }
    });
  }

  public List<Dictionary<string, object>> GetActivityGroups()
  {
    int searchLimit = Convert.ToInt32(Env.IrConfigParameter.GetParam("mail.activity.systray.limit", 1000));
    var activities = Env.MailActivity.Search(a => a.UserId == Env.Uid, order: "id desc", limit: searchLimit);
    Dictionary<string, Dictionary<ResRecord, List<MailActivity>>> activitiesByRecordByModelName = new Dictionary<string, Dictionary<ResRecord, List<MailActivity>>>();
    activities.ForEach(activity =>
    {
      var record = Env[activity.ResModel].Browse(activity.ResId);
      if (!activitiesByRecordByModelName.ContainsKey(activity.ResModel))
      {
        activitiesByRecordByModelName[activity.ResModel] = new Dictionary<ResRecord, List<MailActivity>>();
      }
      if (!activitiesByRecordByModelName[activity.ResModel].ContainsKey(record))
      {
        activitiesByRecordByModelName[activity.ResModel][record] = new List<MailActivity>();
      }
      activitiesByRecordByModelName[activity.ResModel][record].Add(activity);
    });
    Dictionary<string, List<MailActivity>> activitiesByModelName = new Dictionary<string, List<MailActivity>>();
    List<int> userCompanyIds = Env.User.CompanyIds.Select(c => c.Id).ToList();
    bool isAllUserCompaniesAllowed = new HashSet<int>(userCompanyIds).SetEquals(new HashSet<int>(Env.Context.GetOrDefault("allowed_company_ids") as List<int> ?? new List<int>()));
    foreach (var (modelName, activitiesByRecord) in activitiesByRecordByModelName)
    {
      List<int> resIds = activitiesByRecord.Keys.Select(r => r.Id).ToList();
      var Model = Env[modelName].WithContext(Env.Context);
      bool hasModelAccessRight = Env[modelName].CheckAccessRights("read", raiseException: false);
      List<ResRecord> allowedRecords = new List<ResRecord>();
      List<ResRecord> unallowedRecords = new List<ResRecord>();
      if (hasModelAccessRight)
      {
        allowedRecords = Model.Browse(resIds).FilterAccessRules("read");
        unallowedRecords = Model.Browse(resIds).Except(allowedRecords).ToList();
      }
      else
      {
        unallowedRecords = Model.Browse(resIds);
      }
      if (hasModelAccessRight && unallowedRecords.Any() && !isAllUserCompaniesAllowed)
      {
        unallowedRecords.RemoveAll(r => Model.WithContext(allowedCompanyIds: userCompanyIds).Browse(r.Id).FilterAccessRules("read").Any());
      }

      foreach (var (record, activities) in activitiesByRecord)
      {
        if (unallowedRecords.Contains(record))
        {
          if (!activitiesByModelName.ContainsKey("mail.activity"))
          {
            activitiesByModelName["mail.activity"] = new List<MailActivity>();
          }
          activitiesByModelName["mail.activity"].AddRange(activities);
        }
        else if (allowedRecords.Contains(record))
        {
          if (!activitiesByModelName.ContainsKey(modelName))
          {
            activitiesByModelName[modelName] = new List<MailActivity>();
          }
          activitiesByModelName[modelName].AddRange(activities);
        }
      }
    }

    List<int> modelIds = activitiesByModelName.Keys.Select(name => Env.IrModel.GetId(name)).ToList();
    Dictionary<string, Dictionary<string, object>> userActivities = new Dictionary<string, Dictionary<string, object>>();
    foreach (var (modelName, activities) in activitiesByModelName)
    {
      var Model = Env[modelName];
      string module = Model.OriginalModule;
      string icon = module != null ? modules.module.GetModuleIcon(module) : "";
      var model = Env.IrModel.Get(modelName).WithPrefetch(modelIds);
      userActivities[modelName] = new Dictionary<string, object>()
      {
        { "id", model.Id },
        { "name", model.Name },
        { "model", modelName },
        { "type", "activity" },
        { "icon", icon },
        { "total_count", 0 },
        { "today_count", 0 },
        { "overdue_count", 0 },
        { "planned_count", 0 },
        { "view_type", Model.SystrayView ?? "list" }
      };
      if (modelName == "mail.activity")
      {
        userActivities[modelName]["activity_ids"] = activities.Select(a => a.Id).ToList();
      }
      activities.ForEach(activity =>
      {
        userActivities[modelName][$"{activity.State}_count"] = Convert.ToInt32(userActivities[modelName][$"{activity.State}_count"]) + 1;
        if (activity.State == "today" || activity.State == "overdue")
        {
          userActivities[modelName]["total_count"] = Convert.ToInt32(userActivities[modelName]["total_count"]) + 1;
        }
      });
    }
    if (userActivities.ContainsKey("mail.activity"))
    {
      userActivities["mail.activity"]["name"] = "Other activities";
    }
    return userActivities.Values.Select(v => v.ToDictionary(k => k.Key, v => v.Value)).ToList();
  }

  private bool IsPortal()
  {
    // TODO: Implement this logic to check if user is portal
    return false;
  }

  private string GetBaseUrl()
  {
    // TODO: Implement this logic to get base URL
    return "";
  }

  private string EmailFormatted
  {
    get
    {
      // TODO: Implement this logic to get formatted email
      return "";
    }
  }

  private bool IsAdmin()
  {
    // TODO: Implement this logic to check if user is admin
    return false;
  }
}
