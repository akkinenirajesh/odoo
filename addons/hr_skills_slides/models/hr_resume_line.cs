csharp
public partial class ResumeLine
{
    public string ComputeCourseUrl()
    {
        if (DisplayType == ResumeLineDisplayType.Course)
        {
            return Channel?.WebsiteUrl;
        }
        return null;
    }
}
