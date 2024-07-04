csharp
public partial class ImLivechatReportOperator
{
    public override string ToString()
    {
        return $"Operator: {Partner?.Name}, Channel: {LivechatChannel?.Name}";
    }

    public void Init()
    {
        // Note: This method would typically be called by the framework to initialize the model
        // The SQL query would be handled differently in C#, possibly using Entity Framework or another ORM
        string sql = @"
            SELECT
                ROW_NUMBER() OVER () AS Id,
                C.LivechatOperator AS Partner,
                C.LivechatChannel AS LivechatChannel,
                COUNT(DISTINCT C.Id) AS NbrChannel,
                C.Id AS Channel,
                C.CreateDate AS StartDate,
                C.RatingLastValue AS Rating,
                DATEDIFF(SECOND, MIN(M.CreateDate), MAX(M.CreateDate)) AS Duration,
                DATEDIFF(SECOND, MIN(M.CreateDate), MIN(MO.CreateDate)) AS TimeToAnswer
            FROM Discuss.Channel C
            JOIN Mail.Message M ON M.ResId = C.Id AND M.Model = 'Discuss.Channel'
            LEFT JOIN Mail.Message MO ON MO.ResId = C.Id AND MO.Model = 'Discuss.Channel' AND MO.Author = C.LivechatOperator
            WHERE C.LivechatChannel IS NOT NULL
            GROUP BY C.Id, C.LivechatOperator";

        // Execute the SQL query and populate the model
        // This would typically be handled by the ORM or data access layer
    }
}
