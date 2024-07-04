csharp
public partial class MailGroupMember {
    public void ComputeEmail() {
        if (this.PartnerId != null) {
            this.Email = this.PartnerId.Email;
        } else if (string.IsNullOrEmpty(this.Email)) {
            this.Email = null;
        }
    }

    public void ComputeEmailNormalized() {
        this.EmailNormalized = Env.NormalizeEmail(this.Email);
    }
}
