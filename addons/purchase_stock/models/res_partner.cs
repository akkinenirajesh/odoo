csharp
public partial class PurchaseStock.ResPartner
{
    public void ComputeOnTimeRate()
    {
        int dateOrderDaysDelta = int.Parse(Env.GetParam("purchase_stock.on_time_delivery_days", "365"));
        var orderLines = Env.Search<Purchase.PurchaseOrderLine>(new[] {
            new CSharp.Condition("PartnerId", this.Id),
            new CSharp.Condition("DateOrder", ">", Env.Date.Today - new TimeSpan(dateOrderDaysDelta, 0, 0, 0)),
            new CSharp.Condition("QtyReceived", "!=", 0),
            new CSharp.Condition("OrderId.State", "in", new[] { "done", "purchase" }),
            new CSharp.Condition("ProductId", "in", Env.Search<Product.Product>(new[] { new CSharp.Condition("Type", "!=", "service") }).Ids)
        });
        var linesQuantity = new Dictionary<int, decimal>();
        var moves = Env.Search<Stock.Move>(new[] {
            new CSharp.Condition("PurchaseLineId", "in", orderLines.Ids),
            new CSharp.Condition("State", "=", "done")
        });
        orderLines.Read(new[] { "DatePlanned", "PartnerId", "ProductUomQty" });
        moves.Read(new[] { "PurchaseLineId", "Date" });
        moves = moves.Where(m => m.Date.Date <= m.PurchaseLineId.DatePlanned.Date).ToList();
        foreach (var move in moves)
        {
            if (linesQuantity.ContainsKey(move.PurchaseLineId.Id))
            {
                linesQuantity[move.PurchaseLineId.Id] += move.Quantity;
            }
            else
            {
                linesQuantity.Add(move.PurchaseLineId.Id, move.Quantity);
            }
        }
        var partnerDict = new Dictionary<int, Tuple<decimal, decimal>>();
        foreach (var line in orderLines)
        {
            if (partnerDict.ContainsKey(line.PartnerId))
            {
                var onTime = partnerDict[line.PartnerId].Item1;
                var ordered = partnerDict[line.PartnerId].Item2;
                onTime += linesQuantity[line.Id];
                ordered += line.ProductUomQty;
                partnerDict[line.PartnerId] = new Tuple<decimal, decimal>(onTime, ordered);
            }
            else
            {
                partnerDict.Add(line.PartnerId, new Tuple<decimal, decimal>(linesQuantity[line.Id], line.ProductUomQty));
            }
        }
        var seenPartner = new List<PurchaseStock.ResPartner>();
        foreach (var partner in partnerDict.Keys)
        {
            var numbers = partnerDict[partner];
            var onTime = numbers.Item1;
            var ordered = numbers.Item2;
            seenPartner.Add(Env.GetById<PurchaseStock.ResPartner>(partner));
            seenPartner.Last().OnTimeRate = ordered == 0 ? -1 : onTime / ordered * 100;
        }
        foreach (var partner in this.Where(p => !seenPartner.Contains(p)))
        {
            partner.OnTimeRate = -1;
        }
    }
}
