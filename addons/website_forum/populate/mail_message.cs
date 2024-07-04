csharp
public partial class WebsiteForum.Message {

    public WebsiteForum.Message(BuviContext env)
    {
        Env = env;
    }
    
    public BuviContext Env { get; }

    public void _Populate(int size)
    {
        var records = base._Populate(size);
        var commentSubtype = Env.Ref("Mail.mt_comment");
        var hours = Enumerable.Range(1, 24).ToList();
        var random = new Random("comments_on_forum_posts");
        var users = Env["res.users"].Browse(Env.Registry.PopulatedModels["res.users"]);
        var valsList = new List<Dictionary<string, object>>();
        var questionIds = Env.Registry.PopulatedModels["forum.post"];
        var posts = Env["forum.post"].Search(new List<List<object>> {
            new List<object> {"|"},
            new List<object> {"id", "in", questionIds},
            new List<object> {"parent_id", "in", questionIds}
        });
        foreach (var post in random.Sample(posts.ToList(), (int)(questionIds.Count * 0.7)))
        {
            var nbComments = random.Choices(CP_WEIGHTS.Keys.ToList(), CP_WEIGHTS.Values.ToList())[0];
            for (var counter = 0; counter < nbComments; counter++)
            {
                valsList.Add(new Dictionary<string, object> {
                    {"AuthorId", random.Choice(users.PartnerId.Ids)},
                    {"Body", $"message_body_{counter}"},
                    {"Date", Env.Datetime.Add(post.CreateDate, hours: random.Choice(hours))},
                    {"MessageType", "comment"},
                    {"Model", "forum.post"},
                    {"ResId", post.Id},
                    {"Subtype", commentSubtype.Id}
                });
            }
        }
        var messages = Env["mail.message"].Create(valsList);
        Env.Logger.Info("mail.message: update comments create date and uid");
        Env.Logger.Info("forum.post: update last_activity_date for posts with comments and/or commented answers");
        var query = @"
            WITH comment_author AS(
                SELECT mm.id, mm.AuthorId, ru.id as user_id, ru.PartnerId
                  FROM mail_message mm
                  JOIN res_users ru
                    ON mm.AuthorId = ru.PartnerId
                 WHERE mm.id in @comment_ids
            ),
            updated_comments as (
                UPDATE mail_message mm
                   SET CreateDate = date,
                       CreateUid = ca.user_id
                  FROM comment_author ca
                 WHERE mm.id = ca.id
             RETURNING res_id as post_id, CreateDate as comment_date
            ),
            max_comment_dates AS (
                SELECT post_id, max(comment_date) as last_comment_date
                  FROM updated_comments
              GROUP BY post_id
            ),
            updated_posts AS (
                UPDATE forum_post fp
                   SET LastActivityDate = CASE  --on questions, answer could be more recent
                  WHEN fp.ParentId IS NOT NULL THEN greatest(LastActivityDate, last_comment_date)
                  ELSE last_comment_date END
                  FROM max_comment_dates
                 WHERE max_comment_dates.post_id = fp.id
             RETURNING fp.id as post_id, fp.LastActivityDate as last_activity_date, fp.ParentId as parent_id
            )
            UPDATE forum_post fp
               SET LastActivityDate = greatest(fp.LastActivityDate, up.last_activity_date)
              FROM updated_posts up
             WHERE up.parent_id = fp.id
    ";
        Env.Cr.Execute(query, new { comment_ids = messages.Ids.ToList() });
        return records.Concat(messages).ToList();
    }
    public List<object> _PopulateDependencies()
    {
        return base._PopulateDependencies().Concat(new List<string> { "res.users", "forum.post" }).ToList();
    }

    private static readonly Dictionary<int, int> CP_WEIGHTS = new Dictionary<int, int> { { 1, 35 }, { 2, 30 }, { 3, 25 }, { 4, 10 } };
}
