C#
public partial class A
{
    public void OnChangeF2()
    {
        this.F3 = (int)(this.F2 / 2);
        this.F5 = this.F2;
        this.F6 = this.F2;
    }

    public void ComputeF4()
    {
        this.F4 = this.F2 / (int.Parse(this.F1) ?? 1);
    }
}

public partial class B
{
    public void ComputeF2()
    {
        this.F2 = 2 * this.F1;
    }
}

public partial class C
{
    public void OnChangeF2()
    {
        this.Name = this.F2.Name;
    }
}

public partial class D
{
    public void OnChangeF2()
    {
        this.F = this.Env.Search<M2O>(new[] { ("Name", "ilike", this.F2) }, 1) ?? null;
    }
}

public partial class E
{
    public void M2MCount()
    {
        this.Count = this.M2M.Count;
    }

    public void SetCount()
    {
        var sub2Records = this.Env.Create<Sub2>(Enumerable.Range(0, this.Count).Select(n => new Sub2 { Name = n.ToString() }));
        this.M2M = sub2Records;
    }
}

public partial class F
{
    public void OnChangeM2O()
    {
        this.M2M = this.M2M.Union(new[] { this.M2O });
    }
}

public partial class Parent
{
    public void OnchangeValues()
    {
        this.V = this.Value + this.Subs.Sum(s => s.Value);
    }
}

public partial class Sub
{
    public void OnchangeValue()
    {
        this.V = this.Value;
    }

    public void ComputeName()
    {
        this.Name = this.V.ToString();
    }

    public void OnchangeHasParent()
    {
        if (this.HasParent)
        {
            this.Value = this.ParentId.Value;
        }
    }
}

public partial class Ref
{
}

public partial class RefSub
{
}

public partial class Default
{
    public void OnchangeValue()
    {
        if (this.Value == 42)
        {
            this.Subs = null;
        }
    }

    public List<Sub3> DefaultSubs()
    {
        return new List<Sub3> {
            this.Env.Create<Sub3>(new Sub3 { V = 5 })
        };
    }
}

public partial class O2MRecursive
{
}

public partial class OnchangeParent
{
    public void OnchangeLineIds()
    {
        foreach (var line in this.LineIds.Where(l => l.Flag))
        {
            this.Env.New<OnchangeLine>(new OnchangeLine { Parent = this });
        }
    }
}

public partial class OnchangeLine
{
    public void OnchangeFlag()
    {
        this.Flag = true;
    }
}

public partial class OnchangeCount
{
    public void OnchangeCount()
    {
        var subRecords = this.Env.Create<OnchangeCountSub>(Enumerable.Range(0, this.Count).Select(i => new OnchangeCountSub { Name = i.ToString() }));
        this.LineIds = subRecords;
    }
}

public partial class OnchangeCountSub
{
}

public partial class O2mReadonlySubfieldChild
{
    public void ComputeF()
    {
        this.F = (this.Name?.Length ?? 0);
    }
}

public partial class ReqBool
{
}

public partial class O2mChangesParent
{
    public void OnchangeName()
    {
        foreach (var line in this.LineIds)
        {
            line.LineIds = line.LineIds.Select(l => this.Env.Delete(l)).ToList()
                .Concat(new List<O2mChangesChildrenLines> { this.Env.Create<O2mChangesChildrenLines>(new O2mChangesChildrenLines { V = 0, Vv = 0 }) }).ToList();
        }
    }
}

public partial class O2mChangesChildren
{
    public void OnchangeV()
    {
        foreach (var line in this.LineIds)
        {
            line.V = this.V;
        }
    }
}

public partial class O2mChangesChildrenLines
{
}

public partial class ResConfigTest
{
}
