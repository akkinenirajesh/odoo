csharp
public partial class ResCompany
{
    public bool AutoriseLockDateChanges(Dictionary<string, object> vals)
    {
        DateTime? periodLockDate = vals.ContainsKey("PeriodLockDate") ? (DateTime?)vals["PeriodLockDate"] : null;
        DateTime? fiscalyearLockDate = vals.ContainsKey("FiscalyearLockDate") ? (DateTime?)vals["FiscalyearLockDate"] : null;
        DateTime? taxLockDate = vals.ContainsKey("TaxLockDate") ? (DateTime?)vals["TaxLockDate"] : null;

        DateTime previousMonth = DateTime.Today.AddMonths(-1);
        int daysInPreviousMonth = DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month);
        DateTime lastDayOfPreviousMonth = new DateTime(previousMonth.Year, previousMonth.Month, daysInPreviousMonth);

        DateTime? oldFiscalyearLockDate = this.FiscalyearLockDate;
        DateTime? oldPeriodLockDate = this.PeriodLockDate;
        DateTime? oldTaxLockDate = this.TaxLockDate;

        // The user attempts to remove the tax lock date
        if (oldTaxLockDate.HasValue && !taxLockDate.HasValue && vals.ContainsKey("TaxLockDate"))
        {
            throw new UserException("The tax lock date is irreversible and can't be removed.");
        }

        // The user attempts to set a tax lock date prior to the previous one
        if (oldTaxLockDate.HasValue && taxLockDate.HasValue && taxLockDate < oldTaxLockDate)
        {
            throw new UserException("The new tax lock date must be set after the previous lock date.");
        }

        // In case of no new tax lock date in vals, fallback to the oldest
        taxLockDate = taxLockDate ?? oldTaxLockDate;
        // The user attempts to set a tax lock date prior to the last day of previous month
        if (taxLockDate.HasValue && taxLockDate > lastDayOfPreviousMonth)
        {
            throw new UserException("You cannot lock a period that has not yet ended. Therefore, the tax lock date must be anterior (or equal) to the last day of the previous month.");
        }

        // The user attempts to remove the lock date for accountants
        if (oldFiscalyearLockDate.HasValue && !fiscalyearLockDate.HasValue && vals.ContainsKey("FiscalyearLockDate"))
        {
            throw new UserException("The lock date for accountants is irreversible and can't be removed.");
        }

        // The user attempts to set a lock date for accountants prior to the previous one
        if (oldFiscalyearLockDate.HasValue && fiscalyearLockDate.HasValue && fiscalyearLockDate < oldFiscalyearLockDate)
        {
            throw new UserException("Any new All Users Lock Date must be posterior (or equal) to the previous one.");
        }

        // In case of no new fiscal year in vals, fallback to the oldest
        fiscalyearLockDate = fiscalyearLockDate ?? oldFiscalyearLockDate;
        if (!fiscalyearLockDate.HasValue)
        {
            return true;
        }

        // The user attempts to set a lock date for accountants prior to the last day of previous month
        if (fiscalyearLockDate > lastDayOfPreviousMonth)
        {
            throw new UserException("You cannot lock a period that has not yet ended. Therefore, the All Users Lock Date must be anterior (or equal) to the last day of the previous month.");
        }

        // In case of no new period lock date in vals, fallback to the one defined in the company
        periodLockDate = periodLockDate ?? oldPeriodLockDate;
        if (!periodLockDate.HasValue)
        {
            return true;
        }

        // The user attempts to set a lock date for accountants prior to the lock date for users
        if (periodLockDate < fiscalyearLockDate)
        {
            throw new UserException("You cannot set stricter restrictions on accountants than on users. Therefore, the All Users Lock Date must be anterior (or equal) to the Invoice/Bills Lock Date.");
        }

        return true;
    }

    public override bool Write(Dictionary<string, object> vals)
    {
        if (vals.ContainsKey("FiscalyearLockDate") || vals.ContainsKey("PeriodLockDate") || vals.ContainsKey("TaxLockDate"))
        {
            this.AutoriseLockDateChanges(vals);
        }
        return base.Write(vals);
    }
}
