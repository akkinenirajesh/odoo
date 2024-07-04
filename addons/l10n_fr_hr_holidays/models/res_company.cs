csharp
public partial class ResCompany
{
    public HrLeaveType GetFrReferenceLeaveType()
    {
        if (L10nFrReferenceLeaveType == null)
        {
            throw new ValidationException("You must first define a reference time off type for the company.");
        }
        return L10nFrReferenceLeaveType;
    }
}
