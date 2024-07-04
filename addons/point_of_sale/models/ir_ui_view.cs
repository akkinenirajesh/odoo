C#
public partial class PointOfSaleIrUiView 
{
    public virtual List<CoreGroup> GroupsId { get; set; }
    public virtual CoreModel ModelId { get; set; }
    public virtual CoreMenu MenuId { get; set; }
    public virtual CoreIrUiView ParentId { get; set; }
    public virtual List<CoreIrUiView> ParentIdCollection { get; set; }
    public virtual CoreIrUiView InheritId { get; set; }
    public virtual List<CoreIrUiView> InheritIdCollection { get; set; }

    public virtual List<object> LoadPosData(object data)
    {
        List<object> fields = LoadPosDataFields(data);
        return Env.Ref("base.view_partner_form").Sudo().Read(fields);
    }

    public virtual List<object> LoadPosDataFields(object configId)
    {
        return new List<object>() { "Id", "Name" };
    }
}
