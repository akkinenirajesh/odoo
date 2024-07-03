csharp
public partial class ActivityReport
{
    public override string ToString()
    {
        // Since _rec_name was set to 'id' in the original Python code,
        // we'll use the Id property for the string representation
        return Id.ToString();
    }

    // You might want to add custom methods here if needed
    // For example, a method to get related tags:
    public IEnumerable<Core.Tag> GetTags()
    {
        return Env.GetById<Crm.Lead>(Lead.Id).Tags;
    }
}
