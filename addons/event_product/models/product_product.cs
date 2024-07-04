csharp
public partial class Product
{
    public override string ToString()
    {
        // Assuming there's a Name field in the Product model
        return Name;
    }

    public void AddEventTicket(EventTicket ticket)
    {
        if (ticket != null && !EventTickets.Contains(ticket))
        {
            EventTickets.Add(ticket);
            ticket.Product = this;
        }
    }

    public void RemoveEventTicket(EventTicket ticket)
    {
        if (ticket != null && EventTickets.Contains(ticket))
        {
            EventTickets.Remove(ticket);
            ticket.Product = null;
        }
    }
}
