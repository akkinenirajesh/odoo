C#
public partial class MrpProduction
{
    public float ExtraCost { get; set; }

    public void _CalPrice(List<MrpProduction> consumedMoves)
    {
        var finishedMove = this.MoveFinishedIds.Where(x => x.ProductId == this.ProductId && x.State != "done" && x.State != "cancel" && x.Quantity > 0).ToList();

        // Take the price unit of the reception move
        var lastDoneReceipt = finishedMove.SelectMany(m => m.MoveDestIds).Where(m => m.State == "done").ToList().LastOrDefault();
        if (lastDoneReceipt.IsSubcontract)
        {
            this.ExtraCost = lastDoneReceipt._GetPriceUnit();
        }

        this._CalPrice(consumedMoves);
    }
}
