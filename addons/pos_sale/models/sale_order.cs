csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PosSale
{
    public partial class SaleOrder
    {
        public virtual void CountPosOrder()
        {
            var linkedOrders = this.PosOrderLineIds.Select(x => x.OrderId).Distinct();
            this.PosOrderCount = linkedOrders.Count();
        }

        public virtual void ViewPosOrder()
        {
            var linkedOrders = this.PosOrderLineIds.Select(x => x.OrderId).ToList();
            var action = new Dictionary<string, object>
            {
                {"type", "ir.actions.act_window"},
                {"name", "Linked POS Orders"},
                {"res_model", "Pos.PosOrder"},
                {"view_mode", "tree,form"},
                {"domain", new[] { new[] { "id", "in", linkedOrders } }}
            };
            // do something with action like return action
        }

        public virtual void ComputeAmountUnpaid()
        {
            var totalInvoicePaid = this.OrderLine
                .Where(l => l.DisplayType != "line_note" && l.DisplayType != "line_section")
                .SelectMany(l => l.InvoiceLines)
                .Where(l => l.ParentState != "cancel")
                .Sum(l => l.PriceTotal);
            var totalPosPaid = this.OrderLine
                .Where(l => l.DisplayType != "line_note" && l.DisplayType != "line_section")
                .SelectMany(l => l.PosOrderLineIds)
                .Sum(l => l.PriceSubtotalIncl);
            this.AmountUnpaid = this.AmountTotal - (totalInvoicePaid + totalPosPaid);
        }

        public virtual List<object> LoadPosDataFields(int configId)
        {
            return new List<object> {
                "Name",
                "State",
                "UserId",
                "OrderLine",
                "PartnerId",
                "PricelistId",
                "FiscalPositionId",
                "AmountTotal",
                "AmountUntaxed",
                "AmountUnpaid",
                "PickingIds",
                "PartnerShippingId",
                "PartnerInvoiceId",
                "DateOrder"
            };
        }

        public virtual List<object> LoadPosDataDomain(object data)
        {
            return new List<object> { new[] { "PosOrderLineIds.OrderId.State", "=", "draft" } };
        }
    }

    public partial class SaleOrderLine
    {
        public virtual void ComputeQtyDelivered()
        {
            var superQtyDelivered = Env.Call("PosSale.SaleOrderLine", "_compute_qty_delivered", this);
            var posQtyDelivered = this.PosOrderLineIds
                .Where(l => this.ProductId.Type != "service")
                .Sum(l => Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, l.Qty, "p2s"));
            this.QtyDelivered = superQtyDelivered + posQtyDelivered;
        }

        public virtual void ComputeQtyInvoiced()
        {
            var superQtyInvoiced = Env.Call("PosSale.SaleOrderLine", "_compute_qty_invoiced", this);
            var posQtyInvoiced = this.PosOrderLineIds.Sum(l => Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, l.Qty, "p2s"));
            this.QtyInvoiced = superQtyInvoiced + posQtyInvoiced;
        }

        public virtual List<object> GetSaleOrderFields()
        {
            return new List<object>
            {
                "ProductId",
                "DisplayName",
                "PriceUnit",
                "ProductUomQty",
                "TaxId",
                "QtyDelivered",
                "QtyInvoiced",
                "Discount",
                "QtyToInvoice",
                "PriceTotal"
            };
        }

        public virtual List<object> ReadConverted()
        {
            var fieldNames = this.GetSaleOrderFields();
            var results = new List<object>();
            if (this.ProductId.Type != "service")
            {
                var productUom = this.ProductId.UomId;
                var saleLineUom = this.ProductUom;
                var item = this.Read(fieldNames, false)[0];
                if (this.ProductId.Tracking != "none")
                {
                    item["LotNames"] = this.MoveIds.SelectMany(m => m.MoveLineIds).Select(m => m.LotId.Name).ToList();
                }
                if (productUom.Id == saleLineUom.Id)
                {
                    results.Add(item);
                    return results;
                }
                item["ProductUomQty"] = Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, item["ProductUomQty"], "s2p");
                item["QtyDelivered"] = Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, item["QtyDelivered"], "s2p");
                item["QtyInvoiced"] = Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, item["QtyInvoiced"], "s2p");
                item["QtyToInvoice"] = Env.Call("PosSale.SaleOrderLine", "_convert_qty", this, item["QtyToInvoice"], "s2p");
                item["PriceUnit"] = saleLineUom.ComputePrice(item["PriceUnit"], productUom);
                results.Add(item);
            }
            else if (this.DisplayType == "line_note")
            {
                if (results.Any())
                {
                    var lastItem = results.Last();
                    if (lastItem.ContainsKey("CustomerNote"))
                    {
                        lastItem["CustomerNote"] = lastItem["CustomerNote"] + "--" + this.Name;
                    }
                    else
                    {
                        lastItem["CustomerNote"] = this.Name;
                    }
                }
            }
            return results;
        }

        public virtual object ConvertQty(object qty, string direction)
        {
            var productUom = this.ProductId.UomId;
            var saleLineUom = this.ProductUom;
            if (direction == "s2p")
            {
                return saleLineUom.ComputeQuantity(qty, productUom, false);
            }
            else if (direction == "p2s")
            {
                return productUom.ComputeQuantity(qty, saleLineUom, false);
            }
            return null;
        }

        public virtual void Unlink()
        {
            // do not delete downpayment lines created from pos
            var posDownpaymentLines = this.Where(line => line.IsDownpayment && line.PosOrderLineIds.Any()).ToList();
            var linesToUnlink = this.Except(posDownpaymentLines).ToList();
            Env.Call("PosSale.SaleOrderLine", "unlink", linesToUnlink);
        }

        public virtual void ComputeUntaxedAmountInvoiced()
        {
            var superUntaxedAmountInvoiced = Env.Call("PosSale.SaleOrderLine", "_compute_untaxed_amount_invoiced", this);
            var posUntaxedAmountInvoiced = this.PosOrderLineIds.Sum(l => l.PriceSubtotal);
            this.UntaxedAmountInvoiced = superUntaxedAmountInvoiced + posUntaxedAmountInvoiced;
        }

        public virtual List<object> LoadPosDataDomain(object data)
        {
            return new List<object> { new[] { "OrderId", "in", data["SaleOrder"]["data"].Select(order => order["id"]) } };
        }

        public virtual List<object> LoadPosDataFields(int configId)
        {
            return new List<object> {
                "Discount",
                "DisplayName",
                "PriceTotal",
                "PriceUnit",
                "ProductId",
                "ProductUomQty",
                "QtyDelivered",
                "QtyInvoiced",
                "QtyToInvoice",
                "DisplayType",
                "Name",
                "TaxId"
            };
        }
    }
}
