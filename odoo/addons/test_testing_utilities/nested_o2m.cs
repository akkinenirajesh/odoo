csharp
public partial class TtuRoot
{
    public int QtyProduced { get; set; }

    public void GetProducedQty()
    {
        this.QtyProduced = 0;
        foreach (var moveFinished in Env.GetCollection("Ttu.Child").Where(m => m.RootId == this.Id))
        {
            foreach (var moveLine in Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == moveFinished.Id))
            {
                this.QtyProduced += moveLine.QtyDone;
            }
        }
    }

    public void OnchangeProducing()
    {
        var productionMove = Env.GetCollection("Ttu.Child").Where(m => m.RootId == this.Id && m.ProductId == this.ProductId).FirstOrDefault();
        if (productionMove == null)
        {
            return;
        }
        var qtyProducing = this.QtyProducing - this.QtyProduced;
        foreach (var moveLine in Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == productionMove.Id))
        {
            moveLine.QtyDone = 0;
        }
        var vals = productionMove.SetQuantityDonePrepareVals(qtyProducing);
        if (vals.ToCreate.Count > 0)
        {
            foreach (var res in vals.ToCreate)
            {
                Env.GetCollection("Ttu.Grandchild").Create(res);
            }
        }
        if (vals.ToWrite.Count > 0)
        {
            foreach (var (moveLine, res) in vals.ToWrite)
            {
                moveLine.Update(res);
            }
        }

        foreach (var move in Env.GetCollection("Ttu.Child").Where(m => (m.RootRawId == this.Id || (m.RootId == this.Id && m.ProductId != this.ProductId))))
        {
            var newQty = qtyProducing * move.UnitFactor;
            foreach (var moveLine in Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == move.Id))
            {
                moveLine.QtyDone = 0;
            }
            vals = move.SetQuantityDonePrepareVals(newQty);
            if (vals.ToCreate.Count > 0)
            {
                foreach (var res in vals.ToCreate)
                {
                    Env.GetCollection("Ttu.Grandchild").Create(res);
                }
            }
            if (vals.ToWrite.Count > 0)
            {
                foreach (var (moveLine, res) in vals.ToWrite)
                {
                    moveLine.Update(res);
                }
            }
        }
    }

    public System.Xml.Linq.XElement GetDefaultFormView()
    {
        var moveSubview = new System.Xml.Linq.XElement("tree",
            new System.Xml.Linq.XAttribute("editable", "bottom"),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "ProductId")),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "UnitFactor")),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "QuantityDone")),
            new System.Xml.Linq.XElement("field",
                new System.Xml.Linq.XAttribute("name", "MoveLineIds"),
                new System.Xml.Linq.XAttribute("column_invisible", "1"),
                new System.Xml.Linq.XElement("tree",
                    new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "QtyDone"), new System.Xml.Linq.XAttribute("invisible", "1")),
                    new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "ProductId"), new System.Xml.Linq.XAttribute("invisible", "1")),
                    new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "MoveId"), new System.Xml.Linq.XAttribute("invisible", "1")),
                    new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "Id"), new System.Xml.Linq.XAttribute("invisible", "1"))
                )
            )
        );

        var form = new System.Xml.Linq.XElement("form",
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "ProductId")),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "ProductQty")),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "QtyProducing")),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "MoveRawIds"), new System.Xml.Linq.XAttribute("on_change", "1"), moveSubview),
            new System.Xml.Linq.XElement("field", new System.Xml.Linq.XAttribute("name", "MoveFinishedIds"), new System.Xml.Linq.XAttribute("on_change", "1"), moveSubview)
        );

        foreach (var f in form.Descendants("field"))
        {
            f.SetAttributeValue("on_change", "1");
        }

        return form;
    }
}

public partial class TtuChild
{
    public int QuantityDone { get; set; }

    public (List<Dictionary<string, object>> ToCreate, List<(object, Dictionary<string, object>)> ToWrite) SetQuantityDonePrepareVals(int qty)
    {
        var res = new (List<Dictionary<string, object>> ToCreate, List<(object, Dictionary<string, object>)> ToWrite)
        {
            ToCreate = new List<Dictionary<string, object>>(),
            ToWrite = new List<(object, Dictionary<string, object>)>()
        };
        foreach (var ml in Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == this.Id))
        {
            var mlQty = ml.ProductUomQty - ml.QtyDone;
            if (mlQty <= 0)
            {
                continue;
            }

            var takenQty = Math.Min(qty, mlQty);

            res.ToWrite.Add((ml, new Dictionary<string, object>() { { "QtyDone", ml.QtyDone + takenQty } }));
            qty -= takenQty;

            if (qty <= 0)
            {
                break;
            }
        }

        if (qty > 0)
        {
            res.ToCreate.Add(new Dictionary<string, object>()
            {
                { "MoveId", this.Id },
                { "ProductId", this.ProductId },
                { "ProductUomQty", 0 },
                { "QtyDone", qty }
            });
        }
        return res;
    }

    public void QuantityDoneCompute()
    {
        this.QuantityDone = 0;
        foreach (var moveLine in Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == this.Id))
        {
            this.QuantityDone += moveLine.QtyDone;
        }
    }

    public void QuantityDoneSet()
    {
        var quantityDone = this.QuantityDone;
        foreach (var move in Env.GetCollection("Ttu.Child").Where(m => m.Id == this.Id))
        {
            var moveLines = Env.GetCollection("Ttu.Grandchild").Where(ml => ml.MoveId == move.Id);
            if (!moveLines.Any())
            {
                if (quantityDone > 0)
                {
                    // do not impact reservation here
                    var moveLine = Env.GetCollection("Ttu.Grandchild").Create(new Dictionary<string, object>()
                    {
                        { "MoveId", move.Id },
                        { "ProductId", move.ProductId },
                        { "ProductUomQty", 0 },
                        { "QtyDone", quantityDone }
                    });
                    move.Update(new Dictionary<string, object>() { { "MoveLineIds", new List<object>() { moveLine.Id } } });
                }
            }
            else if (moveLines.Count() == 1)
            {
                moveLines.First().QtyDone = quantityDone;
            }
            else
            {
                // Bypass the error if we're trying to write the same value.
                var mlQuantityDone = moveLines.Sum(l => l.QtyDone);
                if (quantityDone != mlQuantityDone)
                {
                    throw new Exception("Cannot set the done quantity from this stock move, work directly with the move lines.");
                }
            }
        }
    }
}
