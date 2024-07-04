csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

public partial class PosSession
{
    public virtual void LoadPosDataModels(PosConfig configId)
    {
        var data = Env.Call("Pos.PosSession", "_load_pos_data_models", configId);
        if (configId.ModulePosRestaurant)
        {
            data.Add("Restaurant.Floor");
            data.Add("Restaurant.Table");
        }
    }

    public virtual void SetLastOrderPreparationChange(List<int> orderIds)
    {
        foreach (var orderId in orderIds)
        {
            var order = Env.Get("Pos.PosOrder", orderId);
            var lastOrderPreparationChange = new Dictionary<string, Dictionary<string, object>>();
            foreach (var orderline in order.Lines)
            {
                lastOrderPreparationChange[orderline.Uuid + " - "] = new Dictionary<string, object>()
                {
                    { "Uuid", orderline.Uuid },
                    { "Name", orderline.FullProductName },
                    { "Note", "" },
                    { "ProductId", orderline.ProductId.Id },
                    { "Quantity", orderline.Qty },
                    { "AttributeValueIds", orderline.AttributeValueIds.Select(x => x.Id).ToList() }
                };
            }
            order.Write(new Dictionary<string, object>()
            {
                { "LastOrderPreparationChange", Newtonsoft.Json.JsonConvert.SerializeObject(lastOrderPreparationChange) }
            });
        }
    }

}
