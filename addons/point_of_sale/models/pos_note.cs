csharp
public partial class PointOfSale_PosNote
{
    public virtual void _LoadPosDataDomain(dynamic data)
    {
        if (data["pos.config"]["data"][0]["note_ids"] != null)
        {
            return new object[] { new object[] { "Id", "in", data["pos.config"]["data"][0]["note_ids"] } };
        }
        else
        {
            return new object[] { };
        }
    }

    public virtual object[] _LoadPosDataFields(int configId)
    {
        return new object[] { "Name" };
    }
}
