C#
public partial class WebsiteSnippetFilter
{
    public List<WebsiteSnippetFilter> _get_hardcoded_sample(string model)
    {
        var samples = base._get_hardcoded_sample(model);
        if (model == "blog.post")
        {
            var data = new List<WebsiteSnippetFilter>
            {
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_2.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("Islands"),
                    Subtitle = Env.Translate("Alone in the ocean"),
                    PostDate = DateTime.Today.AddDays(-1),
                    WebsiteUrl = ""
                },
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_3.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("With a View"),
                    Subtitle = Env.Translate("Awesome hotel rooms"),
                    PostDate = DateTime.Today.AddDays(-2),
                    WebsiteUrl = ""
                },
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_4.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("Skies"),
                    Subtitle = Env.Translate("Taking pictures in the dark"),
                    PostDate = DateTime.Today.AddDays(-3),
                    WebsiteUrl = ""
                },
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_5.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("Satellites"),
                    Subtitle = Env.Translate("Seeing the world from above"),
                    PostDate = DateTime.Today.AddDays(-4),
                    WebsiteUrl = ""
                },
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_6.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("Viewpoints"),
                    Subtitle = Env.Translate("Seaside vs mountain side"),
                    PostDate = DateTime.Today.AddDays(-5),
                    WebsiteUrl = ""
                },
                new WebsiteSnippetFilter
                {
                    CoverProperties = "{\"background-image\": \"url('/website_blog/static/src/img/cover_7.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0\"}",
                    Name = Env.Translate("Jungle"),
                    Subtitle = Env.Translate("Spotting the fauna"),
                    PostDate = DateTime.Today.AddDays(-6),
                    WebsiteUrl = ""
                }
            };

            var merged = new List<WebsiteSnippetFilter>();
            for (int index = 0; index < Math.Max(samples.Count, data.Count); index++)
            {
                merged.Add(new WebsiteSnippetFilter
                {
                    CoverProperties = samples[index % samples.Count].CoverProperties,
                    Name = samples[index % samples.Count].Name,
                    Subtitle = samples[index % samples.Count].Subtitle,
                    PostDate = samples[index % samples.Count].PostDate,
                    WebsiteUrl = samples[index % samples.Count].WebsiteUrl,
                    // Merge definitions
                });
            }

            return merged;
        }

        return samples;
    }
}
