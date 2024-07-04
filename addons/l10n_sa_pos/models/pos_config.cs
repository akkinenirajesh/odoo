C#
public partial class PosConfig {
  public PosConfig() {
  }

  public void OpenUi() {
    if (Env.Company.CountryId == null) {
      throw new UserError("You have to set a country in your company setting.");
    }
    // Call the super class OpenUi method
    base.OpenUi();
  }
}
