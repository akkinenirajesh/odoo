csharp
public partial class ProductImages.IrCronTrigger {
    public void CheckImageCronIsNotAlreadyTriggered() {
        var irCronFetchImage = Env.Ref<Ir.Cron>("product_images.ir_cron_fetch_image");
        if (irCronFetchImage != null && this.CronId.Id != irCronFetchImage.Id) {
            return;
        }

        var cronTriggersCount = Env.Model<IrCronTrigger>().SearchCount(new[] {
            new SearchCriteria("CronId", SearchOperator.Equals, irCronFetchImage.Id)
        });

        var maxCoexistingCronTriggers = this.Env.Context.Get<bool>("automatically_triggered") ? 2 : 1;
        if (cronTriggersCount > maxCoexistingCronTriggers) {
            throw new ValidationError(Env.Translate("This action is already scheduled. Please try again later."));
        }
    }
}
