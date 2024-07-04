csharp
public partial class Website_IrRule
{
    public Website_IrRule(Buvi.Env env)
    {
        this.Env = env;
    }

    public Buvi.Env Env { get; private set; }

    public object EvalContext()
    {
        var res = Env.Call("super", this, "_eval_context");

        // We need is_frontend to avoid showing website's company items in backend
        // (that could be different than current company). We can't use
        // `get_current_website(falback=False)` as it could also return a website
        // in backend (if domain set & match)..
        var isFrontend = Env.Call("ir_http", "get_request_website");
        var website = Env.Model("Website.Website");
        res["website"] = isFrontend ? website.Get("current_website") : website;
        return res;
    }

    public List<string> ComputeDomainKeys()
    {
        var keys = Env.Call("super", this, "_compute_domain_keys");
        keys.Add("website_id");
        return keys;
    }
}
