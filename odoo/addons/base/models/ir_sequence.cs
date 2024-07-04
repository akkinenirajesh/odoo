C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base
{
    public partial class IrSequence
    {
        // all the model methods are written here.
        public int GetNumberNextActual()
        {
            if (this.Implementation != "Standard")
            {
                return this.NumberNext;
            }
            else
            {
                string seqId = string.Format("{0:000}", this.Id);
                return PredictNextVal(seqId);
            }
        }

        public void SetNumberNextActual()
        {
            if (this.NumberNextActual == 0)
            {
                this.NumberNextActual = 1;
            }
            this.NumberNext = this.NumberNextActual;
        }

        public Base.IrSequence GetCurrentSequence(DateTime sequenceDate)
        {
            if (!this.UseDateRange)
            {
                return this;
            }
            Base.IrSequenceDateRange seqDate = Env.Get<Base.IrSequenceDateRange>().Search(
                new List<Tuple<string, object, object>>()
                {
                    Tuple.Create("Sequence", "=", this.Id),
                    Tuple.Create("DateFrom", "<=", sequenceDate),
                    Tuple.Create("DateTo", ">=", sequenceDate)
                },
                1);
            if (seqDate != null)
            {
                return seqDate;
            }
            return this.CreateDateRangeSeq(sequenceDate);
        }

        public Base.IrSequenceDateRange CreateDateRangeSeq(DateTime date)
        {
            string year = date.ToString("yyyy");
            DateTime dateFrom = DateTime.Parse($"{year}-01-01");
            DateTime dateTo = DateTime.Parse($"{year}-12-31");
            Base.IrSequenceDateRange dateRange = Env.Get<Base.IrSequenceDateRange>().Search(
                new List<Tuple<string, object, object>>()
                {
                    Tuple.Create("Sequence", "=", this.Id),
                    Tuple.Create("DateFrom", ">=", date),
                    Tuple.Create("DateFrom", "<=", dateTo)
                },
                1,
                "DateFrom DESC");
            if (dateRange != null)
            {
                dateTo = dateRange.DateFrom.AddDays(-1);
            }
            dateRange = Env.Get<Base.IrSequenceDateRange>().Search(
                new List<Tuple<string, object, object>>()
                {
                    Tuple.Create("Sequence", "=", this.Id),
                    Tuple.Create("DateTo", ">=", dateFrom),
                    Tuple.Create("DateTo", "<=", date)
                },
                1,
                "DateTo DESC");
            if (dateRange != null)
            {
                dateFrom = dateRange.DateTo.AddDays(1);
            }
            seqDateRange = Env.Get<Base.IrSequenceDateRange>().Create(new List<Tuple<string, object>>()
            {
                Tuple.Create("DateFrom", dateFrom),
                Tuple.Create("DateTo", dateTo),
                Tuple.Create("Sequence", this)
            });
            return seqDateRange;
        }

        public string GetNextChar(int numberNext)
        {
            string interpolatedPrefix, interpolatedSuffix;
            (interpolatedPrefix, interpolatedSuffix) = GetPrefixSuffix();
            return interpolatedPrefix + string.Format("{0:D" + this.Padding + "}", numberNext) + interpolatedSuffix;
        }

        public string Next(DateTime sequenceDate = default)
        {
            if (!this.UseDateRange)
            {
                return NextDo();
            }
            // date mode
            if (sequenceDate == default)
            {
                sequenceDate = DateTime.Today;
            }
            Base.IrSequenceDateRange seqDate = Env.Get<Base.IrSequenceDateRange>().Search(
                new List<Tuple<string, object, object>>()
                {
                    Tuple.Create("Sequence", "=", this.Id),
                    Tuple.Create("DateFrom", "<=", sequenceDate),
                    Tuple.Create("DateTo", ">=", sequenceDate)
                },
                1);
            if (seqDate == null)
            {
                seqDate = this.CreateDateRangeSeq(sequenceDate);
            }
            return seqDate.Next();
        }

        public string NextById(DateTime sequenceDate = default)
        {
            return this.Next(sequenceDate);
        }

        public string NextByCode(string sequenceCode, DateTime sequenceDate = default)
        {
            Base.IrSequence seq = Env.Get<Base.IrSequence>().Search(
                new List<Tuple<string, object, object>>()
                {
                    Tuple.Create("Code", "=", sequenceCode),
                    Tuple.Create("Company", "in", new List<object>() { Env.Company.Id, null })
                },
                1,
                "Company");
            if (seq == null)
            {
                return null;
            }
            return seq.Next(sequenceDate);
        }

        public int PredictNextVal(string seqId)
        {
            // Cannot use currval() as it requires prior call to nextval()
            string seqName = $"ir_sequence_{seqId}";
            string seqTable = $"ir_sequence_{seqId}";
            Tuple<object, object, object> result;
            if (Env.Cr.ServerVersion < 100000)
            {
                result = Env.Cr.ExecuteAndGetSingleTuple($"SELECT last_value, increment_by, is_called FROM {seqTable}");
            }
            else
            {
                result = Env.Cr.ExecuteAndGetSingleTuple(
                    $"SELECT last_value, (SELECT increment_by FROM pg_sequences WHERE sequencename = '{seqName}'), is_called FROM {seqTable}");
            }
            if (Convert.ToBoolean(result.Item3))
            {
                return Convert.ToInt32(result.Item1) + Convert.ToInt32(result.Item2);
            }
            return Convert.ToInt32(result.Item1);
        }
    }
}
