C#
public partial class CalendarRecurrenceRule {
    public void ComputeRrule() {
        // Note: 'NeedSyncM' is set to False to avoid syncing the updated recurrence with
        // Outlook, as this update may already come from Outlook. If not, this modification will
        // be already synced through the calendar.event.write()
        if (this.Rrule != this._RruleSerialize()) {
            this.Env.Model("calendar.recurrence").Write(new Dictionary<string, object>() {
                {"Rrule", this._RruleSerialize()}
            }, this.Id);
        }
    }

    public void InverseRrule() {
        // Note: 'NeedSyncM' is set to False to avoid syncing the updated recurrence with
        // Outlook, as this update mainly comes from Outlook (the 'rrule' field is not directly
        // modified in Odoo but computed from other fields).
        if (!string.IsNullOrEmpty(this.Rrule)) {
            var values = this._RruleParse(this.Rrule, this.Dtstart);
            this.Env.Model("calendar.recurrence").Write(new Dictionary<string, object>() {
                { "Dtstart", values.GetValueOrDefault("dtstart") },
                { "NeedSyncM", false }
            }, this.Id);
        }
    }

    public void ApplyRecurrence(Dictionary<string, object> specificValuesCreation, bool noSendEdit, Dictionary<string, object> genericValuesCreation) {
        var events = this.Env.Model("calendar.event").Search(new Dictionary<string, object>() {
            {"RecurrenceId", this.Id},
            {"NeedSyncM", true}
        });
        var detachedEvents = this.Env.Model("calendar.recurrence").Call("apply_recurrence", new object[] { specificValuesCreation, noSendEdit, genericValuesCreation }, this.Id);

        // If a synced event becomes a recurrence, the event needs to be deleted from
        // Microsoft since it's now the recurrence which is synced.
        foreach (var eventItem in events) {
            if (eventItem.Active && !string.IsNullOrEmpty(eventItem.MsUniversalEventId) && string.IsNullOrEmpty(this.MsUniversalEventId)) {
                this.Env.Model("calendar.event").Call("_microsoft_delete", new object[] { eventItem.UserId, eventItem.MicrosoftId }, eventItem.Id);
                this.Env.Model("calendar.event").Write(new Dictionary<string, object>() {
                    {"MsUniversalEventId", null},
                    {"NeedSyncM", true}
                }, eventItem.Id);
            }
        }

        this.Env.Model("calendar.event").Write(new Dictionary<string, object>() {
            {"NeedSyncM", false}
        }, events.Ids);
    }

    public void WriteEvents(Dictionary<string, object> values, DateTime? dtstart) {
        // If only some events are updated, sync those events.
        // If all events are updated, sync the recurrence instead.
        if (dtstart.HasValue || values.ContainsKey("NeedSyncM")) {
            values["NeedSyncM"] = dtstart.HasValue || values["NeedSyncM"];
        }
        this.Env.Model("calendar.recurrence").Call("_write_events", new object[] { values, dtstart }, this.Id);
    }

    public object GetOrganizer() {
        var baseEventId = this.BaseEventId;
        return this.Env.Model("calendar.event").Get(baseEventId).UserId;
    }

    public object GetRrule(DateTime? dtstart) {
        if (!dtstart.HasValue && this.Dtstart.HasValue) {
            dtstart = this.Dtstart;
        }
        return this.Env.Model("calendar.recurrence").Call("_get_rrule", new object[] { dtstart }, this.Id);
    }

    public Dictionary<string, object> GetMicrosoftSyncedFields() {
        var microsoftSyncedFields = this.Env.Model("calendar.event").Call("_get_microsoft_synced_fields", new object[] {});
        return new Dictionary<string, object>(microsoftSyncedFields) {
            {"Rrule", 1}
        };
    }

    public void RestartMicrosoftSync() {
        this.Env.Model("calendar.recurrence").Call("_restart_microsoft_sync", new object[] {});
    }

    public bool HasBaseEventTimeFieldsChanged(Dictionary<string, object> newValues) {
        // Indicates if at least one time field of the base event has changed, based
        // on provided `new` values.
        // Note: for all day event comparison, hours/minutes are ignored.
        var baseEvent = this.Env.Model("calendar.event").Get(this.BaseEventId);
        var oldValues = baseEvent.Read(new List<string>() { "Start", "Stop", "Allday" });
        return oldValues != null && (
            oldValues["Allday"] != newValues["Allday"]
            || (newValues["Allday"] == true && oldValues["Allday"] == true && (
                DateTime.Parse(newValues["Start"].ToString()).Date != DateTime.Parse(oldValues["Start"].ToString()).Date ||
                DateTime.Parse(newValues["Stop"].ToString()).Date != DateTime.Parse(oldValues["Stop"].ToString()).Date
            )) || (
                newValues["Allday"] == false && oldValues["Allday"] == false && (
                    DateTime.Parse(newValues["Start"].ToString()) != DateTime.Parse(oldValues["Start"].ToString()) ||
                    DateTime.Parse(newValues["Stop"].ToString()) != DateTime.Parse(oldValues["Stop"].ToString())
                )
            )
        );
    }

    public void WriteFromMicrosoft(object microsoftEvent, Dictionary<string, object> vals) {
        var currentRrule = this.Rrule;
        vals["EventTz"] = microsoftEvent.start.timeZone;
        this.Env.Model("calendar.recurrence").Call("_write_from_microsoft", new object[] { microsoftEvent, vals }, this.Id);
        var newEventValues = this.Env.Model("calendar.event").Call("_microsoft_to_odoo_values", new object[] { microsoftEvent });
        if (this.HasBaseEventTimeFieldsChanged(newEventValues) && DateTime.Parse(newEventValues["Start"].ToString()) >= DateTime.Parse(this.Env.Model("calendar.event").Get(this.BaseEventId).Start.ToString())) {
            // we need to recreate the recurrence, time_fields were modified.
            var baseEventId = this.BaseEventId;
            // We archive the old events to recompute the recurrence. These events are already deleted on Microsoft side.
            // We can't call _cancel because events without user_id would not be deleted
            var eventsToDelete = this.Env.Model("calendar.event").Search(new Dictionary<string, object>() {
                {"RecurrenceId", this.Id},
                {"Id", "!=", baseEventId}
            });
            this.Env.Model("calendar.event").Write(new Dictionary<string, object>() {
                {"MicrosoftId", null},
                {"MsUniversalEventId", null}
            }, eventsToDelete.Ids);
            eventsToDelete.Unlink();
            this.Env.Model("calendar.event").Write(new Dictionary<string, object>() {
                { "Start", newEventValues["Start"] },
                { "Stop", newEventValues["Stop"] },
                { "Allday", newEventValues["Allday"] },
                { "MicrosoftId", null },
                { "MsUniversalEventId", null },
                { "NeedSyncM", false }
            }, baseEventId);
            if (this.Rrule == currentRrule) {
                // if the rrule has changed, it will be recalculated below
                // There is no detached event now
                this.ApplyRecurrence(null, false, null);
            }
        } else {
            var timeFields = this.Env.Model("calendar.event").Call("_get_time_fields", new object[] {}).Cast<string>().Union(this.Env.Model("calendar.event").Call("_get_recurrent_fields", new object[] {}).Cast<string>()).ToList();
            // We avoid to write time_fields because they are not shared between events.
            this.Env.Model("calendar.recurrence").Call("_write_events", new object[] {
                new Dictionary<string, object>(newEventValues.Where(x => !timeFields.Contains(x.Key))),
                false
            }, this.Id);
        }
        // We apply the rrule check after the time_field check because the microsoft ids are generated according
        // to base_event start datetime.
        if (this.Rrule != currentRrule) {
            var detachedEvents = this.ApplyRecurrence(null, false, null);
            this.Env.Model("calendar.event").Write(new Dictionary<string, object>() {
                {"MsUniversalEventId", null}
            }, detachedEvents.Ids);
            detachedEvents.Unlink();
        }
    }

    public Dictionary<string, object> GetMicrosoftSyncDomain() {
        // Do not sync Odoo recurrences with Outlook Calendar anymore.
        return new Dictionary<string, object>();
    }

    public void CancelMicrosoft() {
        var events = this.Env.Model("calendar.event").Search(new Dictionary<string, object>() {
            {"RecurrenceId", this.Id}
        });
        this.Env.Model("calendar.event").Call("_cancel_microsoft", new object[] {}, events.Ids);
        this.Env.Model("calendar.recurrence").Call("_cancel_microsoft", new object[] {}, this.Id);
    }

    public Dictionary<string, object> MicrosoftToOdooValues(object microsoftRecurrence, List<object> defaultReminders, Dictionary<string, object> defaultValues, bool withIds) {
        var recurrence = microsoftRecurrence.recurrence;
        if (withIds) {
            recurrence = new Dictionary<string, object>(recurrence) {
                {"MicrosoftId", microsoftRecurrence.id},
                {"MsUniversalEventId", microsoftRecurrence.iCalUId},
            };
        }
        return recurrence;
    }

    public Dictionary<string, object> MicrosoftValues(List<string> fieldsToSync) {
        var baseEvent = this.Env.Model("calendar.event").Get(this.BaseEventId);
        return baseEvent.MicrosoftValues(fieldsToSync, new Dictionary<string, object>() { { "type", "seriesMaster" } });
    }

    public void EnsureAttendeesHaveEmail() {
        var events = this.Env.Model("calendar.event").Search(new Dictionary<string, object>() {
            {"RecurrenceId", this.Id},
            {"Active", true}
        });
        this.Env.Model("calendar.event").Call("_ensure_attendees_have_email", new object[] {}, events.Ids);
    }

    public object SplitFrom(object event, Dictionary<string, object> recurrenceValues) {
        var newRecurrence = this.Env.Model("calendar.recurrence").Call("_split_from", new object[] { event, recurrenceValues }, this.Id);
        if (newRecurrence != null && !string.IsNullOrEmpty(this.Env.Model("calendar.event").Get(this.BaseEventId).MicrosoftId)) {
            this.Env.Model("calendar.event").Call("_microsoft_delete", new object[] { this.Env.Model("calendar.event").Get(this.BaseEventId).UserId, this.Env.Model("calendar.event").Get(this.BaseEventId).MicrosoftId }, this.BaseEventId);
        }
        return newRecurrence;
    }

    public int GetEventUserM(int? userId) {
        var event = this._GetFirstEvent();
        if (event != null) {
            return event.GetEventUserM(userId);
        }
        return this.Env.User.Id;
    }

    private object _GetFirstEvent() {
        return this.Env.Model("calendar.event").Search(new Dictionary<string, object>() {
            {"RecurrenceId", this.Id},
            {"Active", true}
        }).FirstOrDefault();
    }

    private Dictionary<string, object> _RruleSerialize() {
        // TODO: Implement _RruleSerialize logic
        return new Dictionary<string, object>();
    }

    private Dictionary<string, object> _RruleParse(string rrule, DateTime dtstart) {
        // TODO: Implement _RruleParse logic
        return new Dictionary<string, object>();
    }
}
