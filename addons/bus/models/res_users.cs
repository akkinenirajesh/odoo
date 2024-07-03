csharp
public partial class ResUsers
{
    public string ComputeImStatus()
    {
        var disconnectionTimer = Env.Get<BusPresence>().DISCONNECTION_TIMER;
        var awayTimer = Env.Get<BusPresence>().AWAY_TIMER;

        var query = @"
            SELECT
                user_id as id,
                CASE WHEN age(now() AT TIME ZONE 'UTC', last_poll) > interval @disconnectionTimer THEN 'offline'
                     WHEN age(now() AT TIME ZONE 'UTC', last_presence) > interval @awayTimer THEN 'away'
                     ELSE 'online'
                END as status
            FROM bus_presence
            WHERE user_id = @userId";

        var parameters = new
        {
            disconnectionTimer = $"{disconnectionTimer} seconds",
            awayTimer = $"{awayTimer} seconds",
            userId = this.Id
        };

        var result = Env.Cr.QuerySingle<ImStatusResult>(query, parameters);
        return result?.Status ?? "offline";
    }

    private class ImStatusResult
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
