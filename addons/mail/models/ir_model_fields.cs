csharp
public partial class MailIrModelField
{
    public int Tracking { get; set; }

    public virtual void ReflectFieldParams(dynamic field, int modelId)
    {
        var vals = base.ReflectFieldParams(field, modelId);
        var tracking = field.tracking;
        if (tracking == true)
        {
            tracking = 100;
        }
        else if (tracking == false)
        {
            tracking = null;
        }
        vals["Tracking"] = tracking;
        return vals;
    }

    public virtual dynamic InstanciateAttrs(dynamic fieldData)
    {
        var attrs = base.InstanciateAttrs(fieldData);
        if (attrs != null && fieldData.ContainsKey("Tracking"))
        {
            attrs["Tracking"] = fieldData["Tracking"];
        }
        return attrs;
    }

    public virtual void Unlink()
    {
        var tracked = this.Where(x => x.Tracking != null);
        if (tracked.Count() > 0)
        {
            var trackingValues = Env.Get("Mail.MailTrackingValue").Search(x => x.FieldId.IsIn(tracked.Ids));
            var fieldToTrackings = trackingValues.GroupBy(x => x.FieldId);
            foreach (var field in fieldToTrackings)
            {
                Env.Get("Mail.MailTrackingValue").Concat(field.ToList()).Write(new
                {
                    FieldInfo = new
                    {
                        Desc = field.Key.FieldDescription,
                        Name = field.Key.Name,
                        Sequence = Env.Get(field.Key.ModelId.Model).MailTrackGetFieldSequence(field.Key.Name),
                        Type = field.Key.Ttype
                    }
                });
            }
        }
        base.Unlink();
    }
}
