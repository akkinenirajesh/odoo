csharp
public partial class PosCategory
{
    public string GetDefaultColor()
    {
        return new Random().Next(0, 11).ToString();
    }

    public List<string> GetHierarchy()
    {
        var hierarchy = new List<string>();
        if (ParentId != null)
        {
            hierarchy.AddRange(ParentId.GetHierarchy());
        }
        hierarchy.Add(Name ?? "");
        return hierarchy;
    }

    public void ComputeDisplayName()
    {
        DisplayName = string.Join(" / ", GetHierarchy());
    }

    public void ComputeHasImage()
    {
        HasImage = Image128 != null && Image128.Length > 0;
    }

    public IEnumerable<PosCategory> GetDescendants()
    {
        var descendants = new List<PosCategory> { this };
        foreach (var child in ChildIds)
        {
            descendants.Add(child);
            descendants.AddRange(child.GetDescendants());
        }
        return descendants;
    }

    public override string ToString()
    {
        return DisplayName ?? Name ?? base.ToString();
    }

    public void OnBeforeCreate()
    {
        if (ParentId != null && Color == 0)
        {
            var parentCategory = Env.Find<PosCategory>(ParentId.Id);
            Color = parentCategory.Color;
        }
    }

    public void OnBeforeUpdate()
    {
        if (ParentId != null && Color == 0)
        {
            var parentCategory = Env.Find<PosCategory>(ParentId.Id);
            Color = parentCategory.Color;
        }
    }
}
