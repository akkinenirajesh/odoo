csharp
public partial class WebsiteSlides.ResUsers
{
    public WebsiteSlides.ResUsers Create(List<Dictionary<string, object>> valsList)
    {
        var users = Env.Model("WebsiteSlides.ResUsers").Create(valsList);
        foreach (var user in users)
        {
            Env.Model("Slide.Channel").Search(new List<List<object>> { new List<object> { "EnrollGroupIds", "in", user.GroupsId.Select(g => g.Id).ToList() } }).ActionAddMembers(user.PartnerId);
        }
        return users;
    }

    public WebsiteSlides.ResUsers Write(Dictionary<string, object> vals)
    {
        var res = Env.Model("WebsiteSlides.ResUsers").Write(vals);
        var sanitizedVals = _RemoveReifiedGroups(vals);
        if (sanitizedVals.ContainsKey("GroupsId"))
        {
            var addedGroupIds = new List<int>();
            foreach (var command in sanitizedVals["GroupsId"] as List<object>)
            {
                if ((int)command[0] == 4)
                {
                    addedGroupIds.Add((int)command[1]);
                }
                else if ((int)command[0] == 6)
                {
                    addedGroupIds.AddRange((List<int>)command[2]);
                }
            }
            Env.Model("Slide.Channel").Search(new List<List<object>> { new List<object> { "EnrollGroupIds", "in", addedGroupIds } }).ActionAddMembers(this.PartnerId);
        }
        return res;
    }

    public List<Dictionary<string, object>> GetGamificationRedirectionData()
    {
        var res = Env.Model("WebsiteSlides.ResUsers").GetGamificationRedirectionData();
        res.Add(new Dictionary<string, object>
        {
            { "Url", "/slides" },
            { "Label", "See our eLearning" }
        });
        return res;
    }

    private Dictionary<string, object> _RemoveReifiedGroups(Dictionary<string, object> vals)
    {
        // Implement logic to remove reified groups from vals
        return vals;
    }
}
