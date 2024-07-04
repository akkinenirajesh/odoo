C#
public partial class DiscussChannel {
  private const int MaxBounceLimit = 10;

  public virtual string GenerateRandomToken() {
    // Built to be shared on invitation link. It uses non-ambiguous characters and it is of a
    // reasonable length: enough to avoid brute force, but short enough to be shareable easily.
    // This token should not contain "mail.guest"._cookie_separator value.
    return string.Join("", Enumerable.Repeat('abcdefghijkmnopqrstuvwxyzABCDEFGHIJKLMNPQRSTUVWXYZ23456789', 10).Select(s => s[Env.Random.Next(s.Length)]));
  }

  public virtual void ComputeIsEditable() {
    if (this.ChannelType == "channel") {
      this.IsEditable = Env.User.IsAdmin() || this.CreateUid.Id == Env.User.Id;
    } else if (this.ChannelType == "group") {
      this.IsEditable = this.IsMember && !Env.User.Share;
    } else {
      this.IsEditable = false;
    }
  }

  public virtual byte[] ComputeAvatar128() {
    return this.Image128 != null ? this.Image128 : GenerateAvatar();
  }

  public virtual string ComputeAvatarCacheKey() {
    return this.Avatar128 == null ? "no-avatar" : System.Security.Cryptography.SHA512.Create().ComputeHash(this.Avatar128).Aggregate("", (s, b) => s + b.ToString("x2"));
  }

  private byte[] GenerateAvatar() {
    if (this.ChannelType != "channel" && this.ChannelType != "group") {
      return null;
    }
    string avatar = this.ChannelType == "group" ? GroupAvatar : ChannelAvatar;
    string bgcolor = GetHslFromSeed(this.Uuid);
    avatar = avatar.Replace("fill=\"#875a7b\"", $"fill=\"{bgcolor}\"");
    return System.Text.Encoding.UTF8.GetBytes(avatar);
  }

  public virtual void ComputeChannelPartnerIds() {
    this.ChannelPartnerIds = this.ChannelMemberIds.Where(m => m.PartnerId != null).Select(m => m.PartnerId).ToList();
  }

  public virtual void InverseChannelPartnerIds() {
    var currentMembers = this.ChannelMemberIds;
    var partners = this.ChannelPartnerIds;
    var newPartners = partners.Except(currentMembers.Where(m => m.PartnerId != null).Select(m => m.PartnerId)).ToList();
    var outdatedMembers = currentMembers.Where(m => !partners.Contains(m.PartnerId)).ToList();

    foreach (var partner in newPartners) {
      Env.Create<DiscussChannelMember>().With(new DiscussChannelMember {
        ChannelId = this.Id,
        PartnerId = partner
      });
    }
    if (outdatedMembers.Count > 0) {
      Env.Delete<DiscussChannelMember>(outdatedMembers.Select(m => m.Id).ToList());
    }
  }

  public virtual Domain SearchChannelPartnerIds(string operator, object operand) {
    return new Domain {
      new DomainPart("ChannelMemberIds", operator, new DomainPart("PartnerId", operator, operand))
    };
  }

  public virtual void ComputeIsMember() {
    var isSelf = Env.Context.Get("guest") != null ? Env.Context.Get("guest") : Env.Context.Get("uid");
    this.IsMember = Env.Create<DiscussChannelMember>().Search([
      new DomainPart("ChannelId", "=", this.Id),
      new DomainPart("IsSelf", "=", true),
      new DomainPart("PartnerId", "=", isSelf)
    ]).Any();
  }

  public virtual Domain SearchIsMember(string operator, object operand) {
    if ((operator == "=" && operand) || (operator == "!=" && !operand)) {
      // Separate query to fetch candidate channels because the sub-select that _search would
      // generate leads psql query plan to take bad decisions. When candidate ids are explicitly
      // given it doesn't need to make (incorrect) guess, at the cost of one extra but fast query.
      // It is expected to return hundreds of channels, a thousand at most, which is acceptable.
      // A "join" would be ideal, but the ORM is currently not able to generate it from the domain.
      var currentPartner = Env.Create<Core.Partner>().GetCurrentPersona();
      var currentGuest = Env.Create<MailGuest>().GetCurrentPersona();
      var channels = currentGuest != null ? Env.Create<DiscussChannel>().With(currentGuest).Search(new Domain()) : (currentPartner != null ? Env.Create<DiscussChannel>().With(currentPartner).Search(new Domain()) : Env.Create<DiscussChannel>().Search(new Domain()));
      return new Domain {
        new DomainPart("Id", operator == "=" ? "in" : "not in", channels.Select(c => c.Id).ToList())
      };
    }
    return new Domain();
  }

  public virtual void ComputeMemberCount() {
    this.MemberCount = Env.Create<DiscussChannelMember>().Search(new Domain {
      new DomainPart("ChannelId", "=", this.Id)
    }).Count();
  }

  public virtual void ComputeGroupPublicId() {
    if (this.ChannelType == "channel") {
      if (this.GroupPublicId == null) {
        this.GroupPublicId = Env.Ref<Core.Group>("base.group_user");
      }
    } else {
      this.GroupPublicId = null;
    }
  }

  public virtual void ComputeInvitationUrl() {
    this.InvitationUrl = $"/chat/{this.Id}/{this.Uuid}";
  }

  public virtual void ConstraintPartnersChat() {
    if (this.ChannelType == "chat") {
      if (this.ChannelMemberIds.Count > 2) {
        throw new Exception("A channel of type 'chat' cannot have more than two users.");
      }
    }
  }

  public virtual void ConstraintGroupIdChannel() {
    if (this.ChannelType != "channel" && (this.GroupPublicId != null || this.GroupIds.Any())) {
      throw new Exception($"For {string.Join(", ", this.Name)}, channel_type should be 'channel' to have the group-based authorization or group auto-subscription.");
    }
  }

  // ... other methods
}
