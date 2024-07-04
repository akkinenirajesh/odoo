csharp
public partial class ResPartner
{
    public void OnChangePhoneValidation()
    {
        if (this.Phone != null)
        {
            this.Phone = PhoneFormat(fname: "Phone", forceFormat: "INTERNATIONAL") ?? this.Phone;
        }
    }

    public void OnChangeMobileValidation()
    {
        if (this.Mobile != null)
        {
            this.Mobile = PhoneFormat(fname: "Mobile", forceFormat: "INTERNATIONAL") ?? this.Mobile;
        }
    }

    private string PhoneFormat(string fname, string forceFormat)
    {
        // Implement PhoneFormat logic here using C#
        // You can use external libraries or custom code
        return null; // Replace with actual formatted phone number
    }
}
