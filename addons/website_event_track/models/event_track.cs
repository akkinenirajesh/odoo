csharp
public partial class WebsiteEventTrack.Track
{
    public void ComputeKanbanStateLabel()
    {
        if (this.KanbanState == "normal")
        {
            this.KanbanStateLabel = Env.Get<WebsiteEventTrack.TrackStage>(this.StageId).LegendNormal;
        }
        else if (this.KanbanState == "blocked")
        {
            this.KanbanStateLabel = Env.Get<WebsiteEventTrack.TrackStage>(this.StageId).LegendBlocked;
        }
        else
        {
            this.KanbanStateLabel = Env.Get<WebsiteEventTrack.TrackStage>(this.StageId).LegendDone;
        }
    }

    public void ComputePartnerName()
    {
        if (this.PartnerId != null && string.IsNullOrEmpty(this.PartnerName))
        {
            this.PartnerName = Env.Get<Res.Partner>(this.PartnerId).Name;
        }
    }

    public void ComputePartnerEmail()
    {
        if (this.PartnerId != null && string.IsNullOrEmpty(this.PartnerEmail))
        {
            this.PartnerEmail = Env.Get<Res.Partner>(this.PartnerId).Email;
        }
    }

    public void ComputePartnerPhone()
    {
        if (this.PartnerId != null && string.IsNullOrEmpty(this.PartnerPhone))
        {
            this.PartnerPhone = Env.Get<Res.Partner>(this.PartnerId).Phone;
        }
    }

    public void ComputePartnerBiography()
    {
        if (string.IsNullOrEmpty(this.PartnerBiography))
        {
            this.PartnerBiography = Env.Get<Res.Partner>(this.PartnerId).WebsiteDescription;
        }
        else if (this.PartnerId != null && string.IsNullOrEmpty(this.PartnerBiography) && !string.IsNullOrEmpty(Env.Get<Res.Partner>(this.PartnerId).WebsiteDescription))
        {
            this.PartnerBiography = Env.Get<Res.Partner>(this.PartnerId).WebsiteDescription;
        }
    }

    public void ComputePartnerFunction()
    {
        if (this.PartnerId != null && string.IsNullOrEmpty(this.PartnerFunction))
        {
            this.PartnerFunction = Env.Get<Res.Partner>(this.PartnerId).Function;
        }
    }

    public void ComputePartnerCompanyName()
    {
        if (Env.Get<Res.Partner>(this.PartnerId).CompanyType == "company")
        {
            this.PartnerCompanyName = Env.Get<Res.Partner>(this.PartnerId).Name;
        }
        else if (string.IsNullOrEmpty(this.PartnerCompanyName))
        {
            this.PartnerCompanyName = Env.Get<Res.Partner>(this.PartnerId).Parent.Name;
        }
    }

    public void ComputePartnerTagLine()
    {
        if (string.IsNullOrEmpty(this.PartnerName))
        {
            this.PartnerTagLine = null;
            return;
        }

        var tagLine = this.PartnerName;
        if (!string.IsNullOrEmpty(this.PartnerFunction))
        {
            if (!string.IsNullOrEmpty(this.PartnerCompanyName))
            {
                tagLine = $"{this.PartnerName}, {this.PartnerFunction} at {this.PartnerCompanyName}";
            }
            else
            {
                tagLine = $"{this.PartnerName}, {this.PartnerFunction}";
            }
        }
        else if (!string.IsNullOrEmpty(this.PartnerCompanyName))
        {
            tagLine = $"{this.PartnerName} from {this.PartnerCompanyName}";
        }

        this.PartnerTagLine = tagLine;
    }

    public void ComputePartnerImage()
    {
        if (this.Image == null)
        {
            this.Image = Env.Get<Res.Partner>(this.PartnerId).Image256;
        }
    }

    public void ComputeContactEmail()
    {
        if (this.PartnerId != null)
        {
            this.ContactEmail = Env.Get<Res.Partner>(this.PartnerId).Email;
        }
    }

    public void ComputeContactPhone()
    {
        if (this.PartnerId != null)
        {
            this.ContactPhone = Env.Get<Res.Partner>(this.PartnerId).Phone;
        }
    }

    public void ComputeEndDate()
    {
        if (this.Date != null)
        {
            var delta = TimeSpan.FromMinutes(60 * this.Duration);
            this.DateEnd = this.Date.Add(delta);
        }
        else
        {
            this.DateEnd = null;
        }
    }

    public void ComputeWebsiteImageUrl()
    {
        if (this.WebsiteImage != null)
        {
            this.WebsiteImageUrl = Env.Get<Website.Website>().ImageUrl(this, "WebsiteImage", 1024);
        }
        else
        {
            this.WebsiteImageUrl = $"/website_event_track/static/src/img/event_track_default_{this.Id % 2}.jpeg";
        }
    }

    public void ComputeIsReminderOn()
    {
        var currentVisitor = Env.Get<Website.Visitor>().GetVisitorFromRequest();
        if (Env.User.IsPublic() && currentVisitor == null)
        {
            this.IsReminderOn = this.WishlistedByDefault;
        }
        else
        {
            if (Env.User.IsPublic())
            {
                var domain = new[] { ("VisitorId", "=", currentVisitor.Id) };
            }
            else if (currentVisitor != null)
            {
                var domain = new[]
                {
                    "|",
                    ("PartnerId", "=", Env.User.PartnerId.Id),
                    ("VisitorId", "=", currentVisitor.Id)
                };
            }
            else
            {
                var domain = new[] { ("PartnerId", "=", Env.User.PartnerId.Id) };
            }

            var eventTrackVisitors = Env.Get<WebsiteEventTrack.TrackVisitor>().SearchRead(
                Domain.And(domain, new[] { ("TrackId", "in", new[] { this.Id }) }),
                new[] { "TrackId", "IsWishlisted", "IsBlacklisted" });

            var wishlistMap = eventTrackVisitors.ToDictionary(
                trackVisitor => trackVisitor["TrackId"][0],
                trackVisitor => new { IsWishlisted = trackVisitor["IsWishlisted"], IsBlacklisted = trackVisitor["IsBlacklisted"] });

            if (wishlistMap.ContainsKey(this.Id))
            {
                this.IsReminderOn = wishlistMap[this.Id].IsWishlisted || (this.WishlistedByDefault && !wishlistMap[this.Id].IsBlacklisted);
            }
            else
            {
                this.IsReminderOn = this.WishlistedByDefault;
            }
        }
    }

    public void ComputeWishlistVisitorIds()
    {
        var results = Env.Get<WebsiteEventTrack.TrackVisitor>().ReadGroup(
            new[] { ("TrackId", "in", new[] { this.Id }), ("IsWishlisted", "=", true) },
            new[] { "TrackId" },
            new[] { ("VisitorId", "array_agg") });

        var visitorIdsMap = results.ToDictionary(
            track => track["TrackId"][0],
            track => (List<int>)track["VisitorId"]);

        this.WishlistVisitorIds = visitorIdsMap.GetValueOrDefault(this.Id, new List<int>());
        this.WishlistVisitorCount = visitorIdsMap.GetValueOrDefault(this.Id, new List<int>()).Count;
    }

    public void ComputeTrackTimeData()
    {
        var nowUtc = DateTime.UtcNow;
        if (this.Date == null)
        {
            this.IsTrackLive = false;
            this.IsTrackSoon = false;
            this.IsTrackToday = false;
            this.IsTrackUpcoming = false;
            this.IsTrackDone = false;
            this.TrackStartRelative = 0;
            this.TrackStartRemaining = 0;
            return;
        }

        var dateBeginUtc = DateTime.SpecifyKind(this.Date, DateTimeKind.Utc);
        var dateEndUtc = DateTime.SpecifyKind(this.DateEnd, DateTimeKind.Utc);

        this.IsTrackLive = dateBeginUtc <= nowUtc && nowUtc < dateEndUtc;
        this.IsTrackSoon = (dateBeginUtc - nowUtc).TotalSeconds < 30 * 60 && dateBeginUtc > nowUtc;
        this.IsTrackToday = dateBeginUtc.Date == nowUtc.Date;
        this.IsTrackUpcoming = dateBeginUtc > nowUtc;
        this.IsTrackDone = dateEndUtc <= nowUtc;

        if (dateBeginUtc >= nowUtc)
        {
            this.TrackStartRelative = (int)(dateBeginUtc - nowUtc).TotalSeconds;
            this.TrackStartRemaining = this.TrackStartRelative;
        }
        else
        {
            this.TrackStartRelative = (int)(nowUtc - dateBeginUtc).TotalSeconds;
            this.TrackStartRemaining = 0;
        }
    }

    public void ComputeCtaTimeData()
    {
        var nowUtc = DateTime.UtcNow;
        if (!this.WebsiteCta)
        {
            this.IsWebsiteCtaLive = false;
            this.WebsiteCtaStartRemaining = 0;
            return;
        }

        var dateBeginUtc = DateTime.SpecifyKind(this.Date, DateTimeKind.Utc).AddMinutes(this.WebsiteCtaDelay ?? 0);
        var dateEndUtc = DateTime.SpecifyKind(this.DateEnd, DateTimeKind.Utc);

        this.IsWebsiteCtaLive = dateBeginUtc <= nowUtc && nowUtc <= dateEndUtc;
        if (dateBeginUtc >= nowUtc)
        {
            this.WebsiteCtaStartRemaining = (int)(dateBeginUtc - nowUtc).TotalSeconds;
        }
        else
        {
            this.WebsiteCtaStartRemaining = 0;
        }
    }

    public List<WebsiteEventTrack.Track> GetTrackSuggestions(List<object> restrictDomain = null, int limit = 0)
    {
        var baseDomain = new[]
        {
            "&",
            ("EventId", "=", this.EventId),
            ("Id", "!=", this.Id)
        };

        if (restrictDomain != null)
        {
            baseDomain = Domain.And(baseDomain, restrictDomain);
        }

        var trackCandidates = Env.Get<WebsiteEventTrack.Track>().Search(baseDomain, limit);
        if (trackCandidates.Count == 0)
        {
            return trackCandidates;
        }

        trackCandidates = trackCandidates.OrderBy(
            track => new[]
            {
                track.IsPublished,
                track.TrackStartRemaining == 0 && track.TrackStartRelative < (10 * 60) && !track.IsTrackDone, // First get the tracks that started less than 10 minutes ago ...
                track.TrackStartRemaining > 0, // Then the one that will begin later (the sooner come first)
                -1 * track.TrackStartRemaining,
                track.IsReminderOn,
                !track.WishlistedByDefault,
                Env.Get<WebsiteEventTrack.TrackTag>().Search(new[] { ("Id", "in", track.TagIds.Select(x => x.Id).ToList()) }).Intersect(Env.Get<WebsiteEventTrack.TrackTag>().Search(new[] { ("Id", "in", this.TagIds.Select(x => x.Id).ToList()) })).Count(),
                track.LocationId == this.LocationId,
                new Random().Next(0, 20)
            }).ToList();

        return trackCandidates.Take(limit).ToList();
    }

    // ... Other methods
}
