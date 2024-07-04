csharp
using System;
using System.Linq;
using Buvi.Core;

namespace Buvi.L10nFrPosCert
{
    public partial class ResCompany
    {
        public override void OnCreate()
        {
            base.OnCreate();
            if (IsAccountingUnalterable())
            {
                var sequenceFields = new[] { "L10nFrPosCertSequenceId" };
                CreateSecureSequence(sequenceFields);
            }
        }

        public override void OnWrite()
        {
            base.OnWrite();
            if (IsAccountingUnalterable())
            {
                var sequenceFields = new[] { "L10nFrPosCertSequenceId" };
                CreateSecureSequence(sequenceFields);
            }
        }

        public void ActionCheckPosHashIntegrity()
        {
            var report = Env.Ref("l10n_fr_pos_cert.action_report_pos_hash_integrity");
            return report.ReportAction(this.Id);
        }

        public Dictionary<string, object> CheckPosHashIntegrity()
        {
            string BuildOrderInfo(PosOrder order)
            {
                var entryReference = "(Receipt ref.: {0})";
                var orderReferenceString = !string.IsNullOrEmpty(order.PosReference) 
                    ? string.Format(entryReference, order.PosReference) 
                    : "";
                return $"{CtxTz(order, "DateOrder")}, {order.L10nFrHash}, {order.Name}, {orderReferenceString}, {CtxTz(order, "WriteDate")}";
            }

            var msgAlert = "";
            var reportDict = new Dictionary<string, object>();

            if (IsAccountingUnalterable())
            {
                var orders = Env.Set<PosOrder>()
                    .Search(x => new[] { "paid", "done", "invoiced" }.Contains(x.State) &&
                                 x.CompanyId == this.Id &&
                                 x.L10nFrSecureSequenceNumber != 0)
                    .OrderBy(x => x.L10nFrSecureSequenceNumber);

                if (!orders.Any())
                {
                    msgAlert = $"There isn't any order flagged for data inalterability yet for the company {Env.Company.Name}. This mechanism only runs for point of sale orders generated after the installation of the module France - Certification CGI 286 I-3 bis. - POS";
                    throw new UserError(msgAlert);
                }

                var previousHash = "";
                var corruptedOrders = new List<string>();

                foreach (var order in orders)
                {
                    if (order.L10nFrHash != order.ComputeHash(previousHash))
                    {
                        corruptedOrders.Add(order.Name);
                        msgAlert = $"Corrupted data on point of sale order with id {order.Id}.";
                    }
                    previousHash = order.L10nFrHash;
                }

                var ordersSortedByDate = orders.OrderBy(x => x.DateOrder).ToList();
                var startOrderInfo = BuildOrderInfo(ordersSortedByDate.First());
                var endOrderInfo = BuildOrderInfo(ordersSortedByDate.Last());

                reportDict["first_order_name"] = startOrderInfo.Split(',')[2].Trim();
                reportDict["first_order_hash"] = startOrderInfo.Split(',')[1].Trim();
                reportDict["first_order_date"] = startOrderInfo.Split(',')[0].Trim();
                reportDict["last_order_name"] = endOrderInfo.Split(',')[2].Trim();
                reportDict["last_order_hash"] = endOrderInfo.Split(',')[1].Trim();
                reportDict["last_order_date"] = endOrderInfo.Split(',')[0].Trim();

                var corruptedOrdersString = string.Join(", ", corruptedOrders);

                return new Dictionary<string, object>
                {
                    { "result", reportDict.Any() ? reportDict : "None" },
                    { "msg_alert", string.IsNullOrEmpty(msgAlert) ? "None" : msgAlert },
                    { "printing_date", Env.FormatDate(DateTime.Today) },
                    { "corrupted_orders", string.IsNullOrEmpty(corruptedOrdersString) ? "None" : corruptedOrdersString }
                };
            }
            else
            {
                throw new UserError($"Accounting is not unalterable for the company {Env.Company.Name}. This mechanism is designed for companies where accounting is unalterable.");
            }
        }

        private bool IsAccountingUnalterable()
        {
            // Implement the logic to check if accounting is unalterable
            throw new NotImplementedException();
        }

        private void CreateSecureSequence(string[] sequenceFields)
        {
            // Implement the logic to create secure sequences
            throw new NotImplementedException();
        }

        private string CtxTz(PosOrder order, string field)
        {
            // Implement the logic to format date/time in the correct timezone
            throw new NotImplementedException();
        }
    }
}
