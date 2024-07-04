csharp
public partial class Event
{
    public void ComputeTrackCount()
    {
        var data = Env.GetModel("WebsiteEventTrack.Track").ReadGroup(new[] { ("StageId.IsCancel", "!=", true) }, new[] { "EventId" }, new[] { "__count" });
        var result = data.Select(d => new { EventId = d.EventId, Count = d.__count }).ToDictionary(d => d.EventId, d => d.Count);
        this.TrackCount = result.ContainsKey(this.Id) ? result[this.Id] : 0;
    }

    public void ComputeWebsiteTrack()
    {
        if (this.EventTypeId != null && this.EventTypeId != this.Original.EventTypeId)
        {
            this.WebsiteTrack = this.EventTypeId.WebsiteTrack;
        }
        else if (this.WebsiteMenu != null && (this.WebsiteMenu != this.Original.WebsiteMenu || !this.WebsiteTrack))
        {
            this.WebsiteTrack = true;
        }
        else if (this.WebsiteMenu == null)
        {
            this.WebsiteTrack = false;
        }
    }

    public void ComputeWebsiteTrackProposal()
    {
        if (this.EventTypeId != null && this.EventTypeId != this.Original.EventTypeId)
        {
            this.WebsiteTrackProposal = this.EventTypeId.WebsiteTrackProposal;
        }
        else if (this.WebsiteTrack != this.Original.WebsiteTrack || !this.WebsiteTrack || !this.WebsiteTrackProposal)
        {
            this.WebsiteTrackProposal = this.WebsiteTrack;
        }
    }

    public void ComputeTracksTagIds()
    {
        this.TracksTagIds = this.TrackIds.SelectMany(t => t.TagIds).Where(t => t.Color != 0).Select(t => t.Id).ToList();
    }

    public void ToggleWebsiteTrack(bool val)
    {
        this.WebsiteTrack = val;
    }

    public void ToggleWebsiteTrackProposal(bool val)
    {
        this.WebsiteTrackProposal = val;
    }

    public List<string> GetMenuUpdateFields()
    {
        var baseFields = base.GetMenuUpdateFields();
        baseFields.AddRange(new[] { "WebsiteTrack", "WebsiteTrackProposal" });
        return baseFields;
    }

    public void UpdateWebsiteMenus(Dictionary<string, List<Event>> menusUpdateByField = null)
    {
        base.UpdateWebsiteMenus(menusUpdateByField);
        if (this.MenuId != null && (menusUpdateByField == null || menusUpdateByField.ContainsKey("WebsiteTrack")))
        {
            this.UpdateWebsiteMenuEntry("WebsiteTrack", "TrackMenuIds", "track");
        }
        if (this.MenuId != null && (menusUpdateByField == null || menusUpdateByField.ContainsKey("WebsiteTrackProposal")))
        {
            this.UpdateWebsiteMenuEntry("WebsiteTrackProposal", "TrackProposalMenuIds", "track_proposal");
        }
    }

    public Dictionary<string, string> GetMenuTypeFieldMatching()
    {
        var res = base.GetMenuTypeFieldMatching();
        res["track_proposal"] = "WebsiteTrackProposal";
        return res;
    }

    public List<Tuple<string, string, bool, int, string>> GetWebsiteMenuEntries()
    {
        var baseEntries = base.GetWebsiteMenuEntries();
        baseEntries.AddRange(new[] {
            new Tuple<string, string, bool, int, string>("Talks", "/event/" + this.Slug + "/track", false, 10, "track"),
            new Tuple<string, string, bool, int, string>("Agenda", "/event/" + this.Slug + "/agenda", false, 70, "track"),
            new Tuple<string, string, bool, int, string>("Talk Proposals", "/event/" + this.Slug + "/track_proposal", false, 15, "track_proposal")
        });
        return baseEntries;
    }
}
