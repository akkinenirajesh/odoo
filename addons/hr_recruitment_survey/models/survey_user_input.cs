csharp
public partial class SurveyUserInput
{
    public void MarkDone()
    {
        var odoobot = Env.Ref("base.partner_root");
        
        if (this.ApplicantId != null)
        {
            string body = $"The applicant \"{this.ApplicantId.PartnerName}\" has finished the survey.";
            this.ApplicantId.MessagePost(body: body, authorId: odoobot.Id);
        }
        
        base.MarkDone();
    }
}
