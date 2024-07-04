csharp
public partial class EventTicket
{
    public override string ToString()
    {
        return Name;
    }

    public void ComputeIsExpired()
    {
        var currentDateTime = Env.Now();
        if (EndSaleDatetime != null)
        {
            IsExpired = EndSaleDatetime < currentDateTime;
        }
        else
        {
            IsExpired = false;
        }
    }

    public void ComputeIsLaunched()
    {
        var now = Env.Now();
        if (StartSaleDatetime == null)
        {
            IsLaunched = true;
        }
        else
        {
            IsLaunched = StartSaleDatetime <= now;
        }
    }

    public void ComputeSaleAvailable()
    {
        SaleAvailable = IsLaunched && !IsExpired && !IsSoldOut;
    }

    public void ComputeSeats()
    {
        SeatsReserved = 0;
        SeatsUsed = 0;
        SeatsAvailable = 0;

        var registrations = Env.Query<EventRegistration>()
            .Where(r => r.EventTicketId == this.Id && (r.State == "open" || r.State == "done") && r.Active)
            .GroupBy(r => r.State)
            .Select(g => new { State = g.Key, Count = g.Count() })
            .ToList();

        foreach (var reg in registrations)
        {
            if (reg.State == "open")
                SeatsReserved = reg.Count;
            else if (reg.State == "done")
                SeatsUsed = reg.Count;
        }

        if (SeatsMax > 0)
        {
            SeatsAvailable = SeatsMax - (SeatsReserved + SeatsUsed);
        }

        SeatsTaken = SeatsReserved + SeatsUsed;
    }

    public void ComputeIsSoldOut()
    {
        IsSoldOut = SeatsLimited && SeatsAvailable == 0;
    }

    public void CheckDatesCoherency()
    {
        if (StartSaleDatetime != null && EndSaleDatetime != null && StartSaleDatetime > EndSaleDatetime)
        {
            throw new UserException($"The stop date cannot be earlier than the start date. Please check ticket {Name}");
        }
    }

    public void CheckSeatsAvailability(int minimalAvailability = 0)
    {
        if (SeatsMax > 0 && SeatsAvailable < minimalAvailability)
        {
            throw new ValidationException($"There are not enough seats available for: the ticket \"{Name}\" ({EventId.Name}): Missing {-SeatsAvailable} seats.");
        }
    }

    public string GetTicketMultilineDescription()
    {
        return $"{ToString()}\n{EventId.Name}";
    }
}
