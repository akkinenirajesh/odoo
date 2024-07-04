csharp
public partial class ImLivechatReportChannel
{
    public override string ToString()
    {
        return $"{TechnicalName} - {StartDate}";
    }

    // Note: The Init method from the original Python code would typically be
    // handled differently in C#, likely as part of the data access layer or
    // through database migrations. The SQL query would need to be adapted for
    // the specific database system being used.
}
