csharp
public partial class DigestTip
{
    public override string ToString()
    {
        return Name;
    }

    public Core.Group DefaultGroup()
    {
        return Env.Ref("base.group_user") as Core.Group;
    }
}
