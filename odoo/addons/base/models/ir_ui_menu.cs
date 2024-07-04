csharp
public partial class BaseIrUiMenu
{
    // all the model methods are written here.
    public void ComputeCompleteName()
    {
        this.CompleteName = GetFullName(6);
    }

    public string GetFullName(int level)
    {
        if (level <= 0)
        {
            return "...";
        }

        if (Env.Get("ParentId") != null)
        {
            return Env.Get("ParentId").GetFullName(level - 1) + "/" + (this.Name ?? "");
        }
        else
        {
            return this.Name;
        }
    }
}
