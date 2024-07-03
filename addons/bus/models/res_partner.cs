csharp
public partial class ResPartner
{
    public string ComputeImStatus()
    {
        var disconnectionTimer = Env.Get<BusPresence>().DISCONNECTION_TIMER;
        var awayTimer = Env.Get<BusPresence>().AWAY_TIMER;

        var query = @"
            SELECT
                U.PartnerId as Id,
                CASE WHEN max(B.LastPoll) IS NULL THEN 'offline'
                    WHEN DATEDIFF(SECOND, max(B.LastPoll), GETUTCDATE()) > @DisconnectionTimer THEN 'offline'
                    WHEN DATEDIFF(SECOND, max(B.LastPresence), GETUTCDATE()) > @AwayTimer THEN 'away'
                    ELSE 'online'
                END as Status
            FROM BusPresence B
            RIGHT JOIN ResUsers U ON B.UserId = U.Id
            WHERE U.PartnerId = @PartnerId AND U.Active = 1
            GROUP BY U.PartnerId";

        var parameters = new Dictionary<string, object>
        {
            { "@DisconnectionTimer", disconnectionTimer },
            { "@AwayTimer", awayTimer },
            { "@PartnerId", this.Id }
        };

        var result = Env.Cr.Query(query, parameters).FirstOrDefault();

        return result?.Status ?? "im_partner";
    }
}
