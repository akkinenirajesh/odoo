csharp
public partial class Course 
{
    public virtual string Name { get; set; }
    public virtual List<Lesson> LessonIds { get; set; }
    public virtual Person AuthorId { get; set; }
    public virtual string PrivateField { get; set; }
    public virtual object Reference { get; set; }
    public virtual Lesson M2oReferenceId { get; set; }
    public virtual string M2oReferenceModel { get; set; }

    public object _SelectionReferenceModel()
    {
        return new List<object>() { new { id = "TestNewApi.Lesson", name = "" } };
    }

}

public partial class Lesson 
{
    public virtual string Name { get; set; }
    public virtual Course CourseId { get; set; }
    public virtual List<Person> AttendeeIds { get; set; }
    public virtual Person TeacherId { get; set; }
    public virtual DateTime TeacherBirthdate { get; set; }
    public virtual DateTime Date { get; set; }

    public void _ComputeDisplayName()
    {
        if (Env.Context.ContainsKey("special"))
        {
            this.Name = "special " + this.Name;
        }
        else
        {
            this.Name = this.Name;
        }
    }
}

public partial class Person 
{
    public virtual string Name { get; set; }
    public virtual List<Lesson> LessonIds { get; set; }
    public virtual Employer EmployerId { get; set; }
    public virtual DateTime Birthday { get; set; }

    public void _ComputeDisplayName()
    {
        string particular = Env.Context.ContainsKey("particular") ? "particular " : "";
        string special = Env.Context.ContainsKey("special") ? " special" : "";
        this.Name = $"{particular}{this.Name}{special}";
    }
}

public partial class Employer 
{
    public virtual string Name { get; set; }
    public virtual List<Person> EmployeeIds { get; set; }
}

public partial class PersonAccount 
{
    public virtual Person PersonId { get; set; }
    public virtual string Login { get; set; }
    public virtual DateTime ActivationDate { get; set; }
}
