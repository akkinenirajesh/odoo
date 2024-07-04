csharp
public partial class EventQuestionAnswer
{
    public override string ToString()
    {
        return Name;
    }

    [OnDelete(AtUninstall = false)]
    public void UnlinkExceptSelectedAnswer()
    {
        var count = Env.Set<Event.EventRegistrationAnswer>()
            .Search(a => a.ValueAnswer.In(this))
            .Count();

        if (count > 0)
        {
            throw new UserError("You cannot delete an answer that has already been selected by attendees.");
        }
    }
}
