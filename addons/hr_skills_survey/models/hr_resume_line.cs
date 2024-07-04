csharp
using System;

public partial class ResumeLine
{
    public void ComputeExpirationStatus()
    {
        ExpirationStatus = ExpirationStatus.Valid;

        if (DateEnd.HasValue)
        {
            if (DateEnd.Value <= DateTime.Today)
            {
                ExpirationStatus = ExpirationStatus.Expired;
            }
            else if (DateEnd.Value.AddMonths(-3) <= DateTime.Today)
            {
                ExpirationStatus = ExpirationStatus.Expiring;
            }
        }
    }
}
