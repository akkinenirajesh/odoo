csharp
public partial class WebsiteResPartner {

    public void ComputeWebsiteUrl() {
        // access the record from the env
        var self = Env.Context.ActiveRecord;
        // use the slug method to generate a slug for the record
        self.WebsiteUrl = "/partners/" + Slug(self);
    }

    private string Slug(WebsiteResPartner partner) {
        // This is a simplified version of the slug method
        // You will need to implement the actual slug logic in your C# code
        return partner.Name.Replace(" ", "-").ToLower();
    }
}
