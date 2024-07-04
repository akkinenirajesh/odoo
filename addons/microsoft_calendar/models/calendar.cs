csharp
public partial class microsoft_calendar_Meeting {

    public void ActionMassArchive(string RecurrenceUpdateSetting) {
        // Do not allow archiving if recurrence is synced with Outlook. Suggest updating directly from Outlook.
        if (Env.User.CheckMicrosoftSyncStatus() && this.MicrosoftId != null) {
            ForbidRecurrenceUpdate();
        }
        this.ActionMassArchive(RecurrenceUpdateSetting);
    }

    private void ForbidRecurrenceUpdate() {
        throw new Exception("Due to an Outlook Calendar limitation, recurrence updates must be done directly in Outlook Calendar.");
    }

    private void _CheckRecurrenceOverlapping(DateTime NewStart) {
        // Outlook does not allow to modify time fields of an event if this event crosses
        // or overlaps the recurrence. In this case a 400 error with the Outlook code "ErrorOccurrenceCrossingBoundary"
        // is returned. That means that the update violates the following Outlook restriction on recurrence exceptions:
        // an occurrence cannot be moved to or before the day of the previous occurrence, and cannot be moved to or after
        // the day of the following occurrence.
        // For example: E1 E2 E3 E4 cannot becomes E1 E3 E2 E4
        var beforeCount = this.RecurrenceId.CalendarEventIds.Where(e => e.Start.Date < this.Start.Date && e != this).Count();
        var afterCount = this.RecurrenceId.CalendarEventIds.Where(e => e.Start.Date < NewStart.Date && e != this).Count();
        if (beforeCount != afterCount) {
            throw new Exception("Outlook limitation: in a recurrence, an event cannot be moved to or before the day of the previous event, and cannot be moved to or after the day of the following event.");
        }
    }

    private bool _IsMatchingTimeslot(DateTime Start, DateTime Stop, bool AllDay) {
        // Check if an event matches with the provided timeslot
        var eventStart = this._Range().Item1;
        var eventStop = this._Range().Item2;
        if (AllDay) {
            eventStart = new DateTime(eventStart.Year, eventStart.Month, eventStart.Day, 0, 0, 0);
            eventStop = new DateTime(eventStop.Year, eventStop.Month, eventStop.Day, 0, 0, 0);
        }
        return (eventStart, eventStop) == (Start, Stop);
    }

    private void _ForbidRecurrenceCreation() {
        throw new Exception("Due to an Outlook Calendar limitation, recurrent events must be created directly in Outlook Calendar.");
    }

    public void Write(Dictionary<string, object> Values) {
        var recurrenceUpdateSetting = Values.GetValueOrDefault("RecurrenceUpdate");
        var dontNotify = Env.Context.GetValueOrDefault("dont_notify");

        // Forbid recurrence updates through Odoo and suggest user to update it in Outlook.
        if (Env.User.CheckMicrosoftSyncStatus()) {
            var recurrencyInBatch = this.Where(ev => ev.Reccurrency).Any();
            var recurrenceUpdateAttempt = recurrenceUpdateSetting != null || Values.ContainsKey("Reccurrency") || (recurrencyInBatch && this.Count() > 0);
            if (!dontNotify && recurrenceUpdateAttempt && !Values.ContainsKey("Active")) {
                ForbidRecurrenceUpdate();
            }
        }

        // When changing the organizer, check its sync status and verify if the user is listed as attendee.
        // Updates from Microsoft must skip this check since changing the organizer on their side is not possible.
        var changeFromMicrosoft = Env.Context.GetValueOrDefault("dont_notify");
        var deactivatedEventsIds = new List<int>();
        foreach (var event in this) {
            if (Values.ContainsKey("UserId") && event.UserId.Id != (int)Values["UserId"] && !changeFromMicrosoft) {
                var senderUser = Env.User.Browse((int)Values["UserId"]);
                var partnerIds = this._GetOrganizerUserChangeInfo(Values);
                var partnerIncluded = senderUser.PartnerId == event.AttendeeIds.FirstOrDefault(a => a.PartnerId == senderUser.PartnerId).PartnerId || partnerIds.Contains(senderUser.PartnerId.Id);
                event._CheckOrganizerValidation(senderUser, partnerIncluded);
                event._RecreateEventDifferentOrganizer(Values, senderUser);
                deactivatedEventsIds.Add(event.Id);
            }
        }

        // check a Outlook limitation in overlapping the actual recurrence
        if (recurrenceUpdateSetting == "self_only" && Values.ContainsKey("Start")) {
            _CheckRecurrenceOverlapping((DateTime)Values["Start"]);
        }

        // if a single event becomes the base event of a recurrency, it should be first
        // removed from the Outlook calendar. Additionaly, checks if synchronization is not paused.
        if (Env.User.CheckMicrosoftSyncStatus() != "sync_paused" && Values.ContainsKey("Reccurrency")) {
            foreach (var event in this) {
                if (!event.Reccurrency && event.RecurrenceId == null) {
                    event._MicrosoftDelete(this._GetOrganizer(), event.MicrosoftId, 3);
                    event.MicrosoftId = null;
                    event.MsUniversalEventId = null;
                }
            }
        }

        var deactivatedEvents = this.Browse(deactivatedEventsIds);
        // Update attendee status before 'values' variable is overridden in super.
        var attendeeIds = Values.GetValueOrDefault("AttendeeIds");
        if (attendeeIds != null && Values.ContainsKey("PartnerIds")) {
            (this - deactivatedEvents)._UpdateAttendeeStatus((List<object>)attendeeIds);
        }

        var res = base.Write(Values, dontNotify);

        // Deactivate events that were recreated after changing organizer.
        if (deactivatedEvents.Any()) {
            res |= base.Write(deactivatedEvents, new Dictionary<string, object>() { { "Active", false } }, dontNotify);
        }

        if (recurrenceUpdateSetting == "all_events" && this.Count() == 1 && this._GetMicrosoftSyncedFields().Intersect(Values.Keys).Any()) {
            this.RecurrenceId.NeedSyncM = true;
        }
        return res;
    }

    public void Unlink() {
        // Forbid recurrent events unlinking from calendar list view with sync active.
        if (this.Any() && Env.User.CheckMicrosoftSyncStatus()) {
            var syncedEvents = _GetSyncedEvents();
            var changeFromMicrosoft = Env.Context.GetValueOrDefault("dont_notify");
            var recurrenceDeletion = syncedEvents.Any(ev => ev.Reccurrency && ev.RecurrenceId != null && ev.FollowRecurrence);
            if (!changeFromMicrosoft && recurrenceDeletion) {
                ForbidRecurrenceUpdate();
            }
        }
        base.Unlink();
    }

    private void _RecreateEventDifferentOrganizer(Dictionary<string, object> Values, microsoft_calendar_Meeting SenderUser) {
        // Copy current event values, delete it and recreate it with the new organizer user.
        var eventCopy = new Dictionary<string, object>(this.CopyData()[0]) { { "MicrosoftId", null } };
        Env.WithUser(SenderUser).Create(new Dictionary<string, object>(eventCopy) { Values });
        if (this.MsUniversalEventId != null) {
            _MicrosoftDelete(this._GetOrganizer(), this.MicrosoftId);
        }
    }

    private List<int> _GetOrganizerUserChangeInfo(Dictionary<string, object> Values) {
        // Return the sender user of the event and the partner ids listed on the event values.
        var senderUserId = Values.GetValueOrDefault("UserId");
        if (senderUserId == null) {
            senderUserId = Env.User.Id;
        }
        var senderUser = Env.User.Browse((int)senderUserId);
        var attendeeValues = _AttendeesValues((List<object>)Values["PartnerIds"]) ?? new List<object>();
        var partnerIds = new List<int>();
        if (attendeeValues.Any()) {
            foreach (var command in attendeeValues) {
                if (command.Count() == 3 && command[2] is Dictionary<string, object> && ((Dictionary<string, object>)command[2]).ContainsKey("PartnerId")) {
                    partnerIds.Add((int)((Dictionary<string, object>)command[2])["PartnerId"]);
                }
            }
        }
        return partnerIds;
    }

    private void _UpdateAttendeeStatus(List<object> AttendeeIds) {
        // Merge current status from 'attendees_ids' with new attendees values for avoiding their info loss in write().
        // Create a dict getting the state of each attendee received from 'attendee_ids' variable and then update their state.
        // :param attendee_ids: List of attendee commands carrying a dict with 'partner_id' and 'state' keys in its third position.
        var stateByPartner = new Dictionary<int, string>();
        foreach (var cmd in AttendeeIds) {
            if (cmd.Count() == 3 && cmd[2] is Dictionary<string, object> && ((Dictionary<string, object>)cmd[2]).ContainsKey("PartnerId") && ((Dictionary<string, object>)cmd[2]).ContainsKey("State")) {
                stateByPartner[(int)((Dictionary<string, object>)cmd[2])["PartnerId"]] = (string)((Dictionary<string, object>)cmd[2])["State"];
            }
        }
        foreach (var attendee in this.AttendeeIds) {
            var stateUpdate = stateByPartner.GetValueOrDefault(attendee.PartnerId.Id);
            if (stateUpdate != null) {
                attendee.State = stateUpdate;
            }
        }
    }

    private List<object> _AttendeesValues(List<object> PartnerIds) {
        // Return the list of partner ids from the command passed to the write() method.
        var attendeeValues = new List<object>();
        if (PartnerIds.Any()) {
            foreach (var partnerId in PartnerIds) {
                attendeeValues.Add(new List<object>() { 0, 0, new Dictionary<string, object>() { { "PartnerId", partnerId } } });
            }
        }
        return attendeeValues;
    }

    private void _CheckOrganizerValidation(microsoft_calendar_Meeting SenderUser, bool PartnerIncluded) {
        // Check if the proposed event organizer can be set accordingly.
        // Edge case: events created or updated from Microsoft should not check organizer validation.
        var changeFromMicrosoft = Env.Context.GetValueOrDefault("dont_notify");
        if (SenderUser != null && SenderUser != Env.User && !changeFromMicrosoft) {
            var currentSyncStatus = Env.User.CheckMicrosoftSyncStatus();
            var senderSyncStatus = Env.WithUser(SenderUser).CheckMicrosoftSyncStatus();
            if (!senderSyncStatus && currentSyncStatus) {
                throw new Exception("For having a different organizer in your event, it is necessary that the organizer have its Odoo Calendar synced with Outlook Calendar.");
            } else if (senderSyncStatus && !PartnerIncluded) {
                throw new Exception("It is necessary adding the proposed organizer as attendee before saving the event.");
            }
        }
    }

    private (DateTime, DateTime) _Range() {
        // Return the starting and ending date and time of the current event.
        var eventStart = this.Start;
        var eventStop = this.Stop;
        if (this.AllDay) {
            eventStart = new DateTime(eventStart.Year, eventStart.Month, eventStart.Day, 0, 0, 0);
            eventStop = new DateTime(eventStop.Year, eventStop.Month, eventStop.Day, 0, 0, 0);
        }
        return (eventStart, eventStop);
    }

    private List<string> _GetMicrosoftSyncedFields() {
        // Return the list of fields that should be synced with Microsoft.
        return new List<string>() { "Name", "Description", "AllDay", "Start", "DateEnd", "Stop", "UserId", "Privacy", "AttendeeIds", "AlarmIds", "Location", "ShowAs", "Active" };
    }

    private IEnumerable<microsoft_calendar_Meeting> _GetSyncedEvents() {
        // Return the list of events that are synced with Microsoft.
        return this.Where(ev => ev.MicrosoftId != null);
    }

    private void _MicrosoftDelete(microsoft_calendar_Meeting Organizer, string MicrosoftId, int Timeout = 3) {
        // Delete an event from Microsoft Calendar.
        // :param organizer: The organizer of the event.
        // :param microsoft_id: The Microsoft Calendar ID of the event.
        // :param timeout: The timeout in seconds for the request.
        // :return: None
        var client = Env.WithUser(Organizer).MicrosoftClient();
        if (client != null) {
            client.DeleteEventAsync(MicrosoftId, Timeout).Wait();
        }
    }

    private microsoft_calendar_Meeting _GetEventUserM(int? UserId = null) {
        // Get the user who will send the request to Microsoft (organizer if synchronized and current user otherwise).
        // Current user must have access to token in order to access event properties (non-public user).
        var currentUserStatus = Env.User.CheckMicrosoftCalendarToken();
        if (UserId != null && UserId != Env.User.Id && currentUserStatus) {
            if (UserId == null) {
                UserId = this.UserId.Id;
            }
            if (UserId != null && Env.WithUser(UserId).CheckMicrosoftSyncStatus()) {
                return Env.User.Browse((int)UserId);
            }
        }
        return Env.User;
    }

    private Dictionary<string, object> _MicrosoftValues(List<string> FieldsToSync, Dictionary<string, object> InitialValues = null) {
        var values = InitialValues ?? new Dictionary<string, object>();
        if (!FieldsToSync.Any()) {
            return values;
        }

        var microsoftGuid = Env.ConfigParameter.GetParam("microsoft_calendar.microsoft_guid");

        if (this.MicrosoftRecurrenceMasterId != null && !values.ContainsKey("type")) {
            values["seriesMasterId"] = this.MicrosoftRecurrenceMasterId;
            values["type"] = "exception";
        }

        if (FieldsToSync.Contains("Name")) {
            values["subject"] = this.Name ?? "";
        }

        if (FieldsToSync.Contains("Description")) {
            values["body"] = new Dictionary<string, object>() {
                { "content", this.Description != null && !this.Description.IsNullOrEmpty() ? this.Description : "" },
                { "contentType", "html" },
            };
        }

        if (FieldsToSync.Any(x => new List<string>() { "AllDay", "Start", "DateEnd", "Stop" }.Contains(x))) {
            if (this.AllDay) {
                var start = new Dictionary<string, object>() { { "dateTime", this.Start.Date.ToString("yyyy-MM-dd"), "timeZone", "Europe/London" } };
                var end = new Dictionary<string, object>() { { "dateTime", this.Stop.Date.AddDays(1).ToString("yyyy-MM-dd"), "timeZone", "Europe/London" } };
                values["start"] = start;
                values["end"] = end;
                values["isAllDay"] = this.AllDay;
            } else {
                var start = new Dictionary<string, object>() { { "dateTime", this.Start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), "timeZone", "Europe/London" } };
                var end = new Dictionary<string, object>() { { "dateTime", this.Stop.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), "timeZone", "Europe/London" } };
                values["start"] = start;
                values["end"] = end;
                values["isAllDay"] = this.AllDay;
            }
        }

        if (FieldsToSync.Contains("Location")) {
            values["location"] = new Dictionary<string, object>() { { "displayName", this.Location ?? "" } };
        }

        if (FieldsToSync.Contains("AlarmIds")) {
            var alarmId = this.AlarmIds.Where(a => a.AlarmType == "notification").FirstOrDefault();
            values["isReminderOn"] = alarmId != null;
            values["reminderMinutesBeforeStart"] = alarmId != null ? alarmId.Duration : 0;
        }

        if (FieldsToSync.Contains("UserId")) {
            values["organizer"] = new Dictionary<string, object>() { { "emailAddress", new Dictionary<string, object>() { { "address", this.UserId.Email ?? "" }, { "name", this.UserId.DisplayName ?? "" } } } };
            values["isOrganizer"] = this.UserId.Id == Env.User.Id;
        }

        if (FieldsToSync.Contains("AttendeeIds")) {
            var attendees = this.AttendeeIds.Where(att => att.PartnerId != this.UserId.PartnerId).ToList();
            values["attendees"] = attendees.Select(attendee => new Dictionary<string, object>() {
                { "emailAddress", new Dictionary<string, object>() { { "address", attendee.Email ?? "" }, { "name", attendee.DisplayName ?? "" } } },
                { "status", new Dictionary<string, object>() { { "response", _GetAttendeeStatusO2M(attendee) } } }
            }).ToList();
        }

        if (FieldsToSync.Contains("Privacy") || FieldsToSync.Contains("ShowAs")) {
            values["showAs"] = this.ShowAs;
            var sensitivityO2M = new Dictionary<string, string>() {
                { "public", "normal" },
                { "private", "private" },
                { "confidential", "confidential" },
            };
            // Set default privacy in event according to the organizer's calendar default privacy if defined.
            if (this.UserId != null) {
                sensitivityO2M[null] = sensitivityO2M.GetValueOrDefault(this.UserId.CalendarDefaultPrivacy);
            } else {
                sensitivityO2M[null] = "normal";
            }
            values["sensitivity"] = sensitivityO2M.GetValueOrDefault(this.Privacy);
        }

        if (FieldsToSync.Contains("Active") && !this.Active) {
            values["isCancelled"] = true;
        }

        if (values.GetValueOrDefault("type") == "seriesMaster") {
            var recurrence = this.RecurrenceId;
            var pattern = new Dictionary<string, object>() { { "interval", recurrence.Interval } };
            if (recurrence.RruleType == "daily" || recurrence.RruleType == "weekly") {
                pattern["type"] = recurrence.RruleType;
            } else {
                var prefix = recurrence.MonthBy == "date" ? "absolute" : "relative";
                pattern["type"] = recurrence.RruleType != null ? prefix + recurrence.RruleType.First().ToString().ToUpper() + recurrence.RruleType.Substring(1) : "";
            }

            if (recurrence.MonthBy == "date") {
                pattern["dayOfMonth"] = recurrence.Day;
            }

            if (recurrence.MonthBy == "day" || recurrence.RruleType == "weekly") {
                var daysOfWeek = new List<string>();
                var weekdays = new Dictionary<string, bool>() {
                    { "monday", recurrence.Mon },
                    { "tuesday", recurrence.Tue },
                    { "wednesday", recurrence.Wed },
                    { "thursday", recurrence.Thu },
                    { "friday", recurrence.Fri },
                    { "saturday", recurrence.Sat },
                    { "sunday", recurrence.Sun },
                };
                foreach (var (weekdayName, weekday) in weekdays) {
                    if (weekday) {
                        daysOfWeek.Add(weekdayName);
                    }
                }
                pattern["daysOfWeek"] = daysOfWeek;
                pattern["firstDayOfWeek"] = "sunday";
            }

            if (recurrence.RruleType == "monthly" && recurrence.MonthBy == "day") {
                var bydaySelection = new Dictionary<string, string>() {
                    { "1", "first" },
                    { "2", "second" },
                    { "3", "third" },
                    { "4", "fourth" },
                    { "-1", "last" },
                };
                pattern["index"] = bydaySelection.GetValueOrDefault(recurrence.ByDay);
            }

            var dtstart = recurrence.Dtstart ?? DateTime.Now;
            var ruleRange = new Dictionary<string, object>() { { "startDate", dtstart.Date.ToString("yyyy-MM-dd") } };

            if (recurrence.EndType == "count") { // e.g. stop after X occurence
                ruleRange["numberOfOccurrences"] = Math.Min(recurrence.Count, 720);
                ruleRange["type"] = "numbered";
            } else if (recurrence.EndType == "forever") {
                ruleRange["numberOfOccurrences"] = 720;
                ruleRange["type"] = "numbered";
            } else if (recurrence.EndType == "end_date") { // e.g. stop after 12/10/2020
                ruleRange["endDate"] = recurrence.Until.ToString("yyyy-MM-dd");
                ruleRange["type"] = "endDate";
            }

            values["recurrence"] = new Dictionary<string, object>() {
                { "pattern", pattern },
                { "range", ruleRange },
            };
        }

        return values;
    }

    private string _GetAttendeeStatusO2M(calendar_attendee Attendee) {
        // Return the attendee's status in Microsoft Calendar format.
        if (this.UserId != null && this.UserId.Id == Attendee.PartnerId.UserId.Id) {
            return "organizer";
        }
        var attendeeConverterO2M = new Dictionary<string, string>() {
            { "needsAction", "None" },
            { "tentative", "tentativelyAccepted" },
            { "declined", "declined" },
            { "accepted", "accepted" },
        };
        return attendeeConverterO2M.GetValueOrDefault(Attendee.State);
    }

    private Dictionary<string, object> _MicrosoftValuesOccurence(Dictionary<string, object> InitialValues = null) {
        var values = InitialValues ?? new Dictionary<string, object>();
        values["type"] = "occurrence";

        if (this.AllDay) {
            var start = new Dictionary<string, object>() { { "dateTime", this.Start.Date.ToString("yyyy-MM-dd"), "timeZone", "Europe/London" } };
            var end = new Dictionary<string, object>() { { "dateTime", this.Stop.Date.AddDays(1).ToString("yyyy-MM-dd"), "timeZone", "Europe/London" } };
            values["start"] = start;
            values["end"] = end;
            values["isAllDay"] = this.AllDay;
        } else {
            var start = new Dictionary<string, object>() { { "dateTime", this.Start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), "timeZone", "Europe/London" } };
            var end = new Dictionary<string, object>() { { "dateTime", this.Stop.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss"), "timeZone", "Europe/London" } };
            values["start"] = start;
            values["end"] = end;
            values["isAllDay"] = this.AllDay;
        }

        return values;
    }

    private void _CancelMicrosoft() {
        // Cancel an Microsoft event.
        // There are 2 cases:
        //   1) the organizer is an Odoo user: he's the only one able to delete the Odoo event. Attendees can just decline.
        //   2) the organizer is NOT an Odoo user: any attendee should remove the Odoo event.
        var user = Env.User;
        var records = this.Where(e => e.UserId == null || e.UserId == user || user.PartnerId == e.PartnerIds.FirstOrDefault(p => p.PartnerId == user.PartnerId).PartnerId).ToList();
        foreach (var event in records) {
            // remove the tracking data to avoid calling _track_template in the pre-commit phase
            Env.Cr.PrecommitData.Remove($"mail.tracking.create.{event.Name}.{event.Id}");
        }
        base._CancelMicrosoft(records);
        var attendees = (this - records).AttendeeIds.Where(a => a.PartnerId == user.PartnerId).ToList();
        attendees.DoDecline();
    }
}
