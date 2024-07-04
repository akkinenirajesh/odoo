csharp
public partial class PosSession
{
    public virtual void Create(List<Dictionary<string, object>> valsList)
    {
        var sessions = Env.Call("Pos.PosSession", "Create", valsList);
        var createdSessions = CreatePosSelfSessionsSequence(sessions);
    }

    public virtual List<Dictionary<string, object>> CreatePosSelfSessionsSequence(List<Dictionary<string, object>> sessions)
    {
        var companyId = Env.Get("Res.Company", "Id");

        foreach (var session in sessions)
        {
            Env.Call("Ir.Sequence", "Create", new Dictionary<string, object>
            {
                { "Name", "PoS Order by Session" },
                { "Padding", 4 },
                { "Code", $"pos.order_{session["Id"]}" },
                { "NumberNext", 1 },
                { "NumberIncrement", 1 },
                { "CompanyId", companyId }
            });
        }

        return sessions;
    }

    public virtual List<Dictionary<string, object>> LoadPosSelfDataDomain(Dictionary<string, object> data)
    {
        return new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "ConfigId", data["pos.config"]["data"][0]["Id"] },
                { "State", "opened" }
            }
        };
    }

    public virtual Dictionary<string, object> LoadPosData(Dictionary<string, object> data)
    {
        var sessions = Env.Call("Pos.PosSession", "LoadPosData", data);
        var selfOrderingCount = Env.Call("Pos.PosConfig", "SearchCount", new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "Company", Env.Get("Res.Company", "Id") }
            },
            new Dictionary<string, object>
            {
                { "Or", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "SelfOrderingMode", "kiosk" } },
                    new Dictionary<string, object> { { "SelfOrderingMode", "mobile" } }
                }}
            }
        }, 1);
        sessions["data"][0]["_self_ordering"] = selfOrderingCount > 0;
        return sessions;
    }
}
