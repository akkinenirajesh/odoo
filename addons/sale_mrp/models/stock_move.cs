C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stock
{
    public partial class StockMove
    {
        public virtual void _PrepareProcurementValues()
        {
            var res = _PrepareProcurementValuesBase();
            res["AnalyticAccountId"] = Env.Get("Sale.SaleOrderLine").SearchOne(x => x.OrderId == this.SaleLineId).AnalyticAccountId;
            if (res["AnalyticAccountId"] != null)
            {
                res["AnalyticDistribution"] = new Dictionary<object, object>() { { res["AnalyticAccountId"], 100 } };
            }
            return;
        }

        private Dictionary<string, object> _PrepareProcurementValuesBase()
        {
            throw new NotImplementedException();
        }
    }
}

namespace Stock
{
    public partial class StockMoveLine
    {
        public virtual void _ComputeSalePrice()
        {
            var kitLines = this.Where(x => x.MoveId.BomLineId.BomId.Type == "phantom");
            foreach (var moveLine in kitLines)
            {
                moveLine.SalePrice = moveLine.ProductId.ListPrice * Env.Get("Product.Uom").ComputeQuantity(moveLine.Quantity, moveLine.ProductId.UomId);
            }
            var remainingLines = this.Except(kitLines);
            _ComputeSalePriceBase(remainingLines);
        }

        private void _ComputeSalePriceBase(IEnumerable<StockMoveLine> kitLines)
        {
            throw new NotImplementedException();
        }
    }
}
