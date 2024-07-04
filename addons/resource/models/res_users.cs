C#
public partial class ResourceResUsers {
    public virtual void Write(Dictionary<string, object> vals) {
        var result = this.Env.Call("super", "write", this, vals);

        if (vals.ContainsKey("Timezone") && this.Env.User.LoginDate == null && this.Env.User.Id == this.Env.Ref("base.user_admin").Id && this == this.Env.User) {
            if (this.ResourceCalendarId != null) {
                this.Env.Call("write", this.ResourceCalendarId, new Dictionary<string, object>() { { "Timezone", vals["Timezone"] } });
            } else {
                var defaultCalendar = this.Env.Ref("resource.resource_calendar_std");
                this.Env.Call("write", defaultCalendar, new Dictionary<string, object>() { { "Timezone", vals["Timezone"] } });
            }
        }
    }
}
