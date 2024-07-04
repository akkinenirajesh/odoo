csharp
public partial class WebsiteTwitter 
{
    public WebsiteTwitterTweetCollection FetchFavoriteTweets()
    {
        var websiteTweets = Env.Model("Website.WebsiteTwitterTweet").Collection<WebsiteTwitterTweet>();
        var tweetIds = new List<int>();
        foreach (var website in this)
        {
            if (!website.TwitterApiKey || !website.TwitterApiSecret || !website.TwitterScreenName)
            {
                Env.Logger.Debug("Skip fetching favorite posts for unconfigured website {0}", website);
                continue;
            }
            var params = new Dictionary<string, string>() { { "screen_name", website.TwitterScreenName } };
            var lastTweet = websiteTweets.Search(new[] { new[] { "WebsiteId", "=", website.Id }, new[] { "ScreenName", "=", website.TwitterScreenName } }, 1, "TweetId desc").FirstOrDefault();
            if (lastTweet != null)
            {
                params["since_id"] = lastTweet.TweetId.ToString();
            }
            Env.Logger.Debug("Fetching favorite posts using params {0}", params);
            var response = _Request(website, "https://api.twitter.com/1.1/favorites/list.json", params);
            foreach (var tweetDict in response)
            {
                var tweetId = tweetDict["id"];
                var tweetIdExists = websiteTweets.Search(new[] { new[] { "TweetId", "=", tweetId } }).Count > 0;
                if (!tweetIdExists)
                {
                    var newTweet = websiteTweets.Create(new WebsiteTwitterTweet()
                    {
                        WebsiteId = website.Id,
                        Tweet = JsonConvert.SerializeObject(tweetDict),
                        TweetId = (int)tweetId,
                        ScreenName = website.TwitterScreenName,
                    });
                    Env.Logger.Debug("Found new favorite: {0}, {1}", tweetId, tweetDict);
                    tweetIds.Add(newTweet.Id);
                }
            }
        }
        return websiteTweets;
    }

    private List<Dictionary<string, object>> _Request(WebsiteTwitter website, string url, Dictionary<string, string> params = null)
    {
        var accessToken = _GetAccessToken(website);
        try
        {
            var request = RestClient.Get(url, params, new Dictionary<string, string>() { { "Authorization", $"Bearer {accessToken}" } }, 10);
            if (!request.IsSuccessStatusCode)
            {
                Env.Logger.Debug("X API request failed with code: {0}, msg: {1}, content: {2}", request.StatusCode, request.ReasonPhrase, request.Content);
                throw new Exception($"X API request failed with code: {request.StatusCode}, msg: {request.ReasonPhrase}, content: {request.Content}");
            }
            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(request.Content);
        }
        catch (Exception e)
        {
            Env.Logger.Debug("X API request failed with code: {0}, msg: {1}, content: {2}", e.Message, e.InnerException, e.StackTrace);
            throw e;
        }
    }

    private string _GetAccessToken(WebsiteTwitter website)
    {
        var r = RestClient.Post("https://api.twitter.com/oauth2/token", new Dictionary<string, string>() { { "grant_type", "client_credentials" } }, new Dictionary<string, string>() { { "Authorization", $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes($"{website.TwitterApiKey}:{website.TwitterApiSecret}"))}" } }, 10);
        if (!r.IsSuccessStatusCode)
        {
            Env.Logger.Debug("X API request failed with code: {0}, msg: {1}, content: {2}", r.StatusCode, r.ReasonPhrase, r.Content);
            throw new Exception($"X API request failed with code: {r.StatusCode}, msg: {r.ReasonPhrase}, content: {r.Content}");
        }
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(r.Content);
        return data["access_token"].ToString();
    }
}
