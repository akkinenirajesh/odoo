csharp
public partial class PosConfig {

    public PosConfig() {
        // constructor 
    }

    public void CheckBeforeCreatingNewSession() {
        // Check validity of programs before opening a new session
        string invalidRewardProductsMsg = "";
        foreach (var reward in Env.Search<Loyalty.Program>(p => p.PosOk == true && (p.PosConfigIds.Contains(this) || p.PosConfigIds.Count == 0)).RewardIds) {
            if (reward.RewardType == "product") {
                foreach (var product in reward.RewardProductIds) {
                    if (product.AvailableInPos) {
                        continue;
                    }
                    invalidRewardProductsMsg += "\n\t";
                    invalidRewardProductsMsg += $"Program: {reward.ProgramId.Name}, Reward Product: `{product.Name}`";
                }
            }
            foreach (var product in reward.ProgramId.RuleIds.ValidProductIds) {
                if (product.AvailableInPos) {
                    continue;
                }
                invalidRewardProductsMsg += "\n\t";
                invalidRewardProductsMsg += $"Program: {reward.ProgramId.Name}, Rule Product: `{product.Name}`";
            }
        }

        if (!string.IsNullOrEmpty(invalidRewardProductsMsg)) {
            throw new UserError("To continue, make the following reward products available in Point of Sale.\n" + invalidRewardProductsMsg);
        }

        var giftCardPrograms = Env.Search<Loyalty.Program>(p => p.ProgramType == "gift_card");
        foreach (var gcProgram in giftCardPrograms) {
            // Do not allow a gift card program with more than one rule or reward, and check that they make sense
            if (gcProgram.RewardIds.Count > 1) {
                throw new UserError("Invalid gift card program. More than one reward.");
            } else if (gcProgram.RuleIds.Count > 1) {
                throw new UserError("Invalid gift card program. More than one rule.");
            }

            var rule = gcProgram.RuleIds;
            if (rule.RewardPointAmount != 1 || rule.RewardPointMode != "money") {
                throw new UserError("Invalid gift card program rule. Use 1 point per currency spent.");
            }

            var reward = gcProgram.RewardIds;
            if (reward.RewardType != "discount" || reward.DiscountMode != "per_point" || reward.Discount != 1) {
                throw new UserError("Invalid gift card program reward. Use 1 currency per point discount.");
            }

            if (this.GiftCardSettings == "CreateSet") {
                if (gcProgram.MailTemplateId == null) {
                    throw new UserError("There is no email template on the gift card program and your pos is set to print them.");
                }
                if (gcProgram.PosReportPrintId == null) {
                    throw new UserError("There is no print report on the gift card program and your pos is set to print them.");
                }
            }
        }
    }

    public object UseCouponCode(string code, string creationDate, int partnerId, int pricelistId) {
        // Points desc so that in coupon mode one could use a coupon multiple times
        var coupon = Env.Search<Loyalty.Card>(c => c.ProgramId.IsIn(GetProgramIds().Select(p => p.Id)) &&
                                                 (c.PartnerId == null || c.PartnerId == partnerId) &&
                                                 c.ProgramType == "gift_card" && c.Code == code,
                                                 orderBy: "PartnerId, Points DESC").FirstOrDefault();
        var program = coupon.ProgramId;

        if (coupon == null || !program.Active) {
            return new { successful = false, payload = new { error_message = $"This coupon is invalid ({code})." } };
        }

        var checkDate = DateTime.ParseExact(creationDate.Substring(0, 11), "yyyy-MM-dd", null);
        var todayDate = DateTime.Now;
        string errorMessage = null;

        if ((coupon.ExpirationDate != null && coupon.ExpirationDate < checkDate) ||
            (program.DateTo != null && program.DateTo < todayDate) ||
            (program.LimitUsage && program.TotalOrderCount >= program.MaxUsage)) {
            errorMessage = $"This coupon is expired ({code}).";
        } else if (program.DateFrom != null && program.DateFrom > todayDate) {
            errorMessage = $"This coupon is not yet valid ({code}).";
        } else if (program.RewardIds.Count == 0 || !program.RewardIds.Any(r => r.RequiredPoints <= coupon.Points)) {
            errorMessage = "No reward can be claimed with this coupon.";
        } else if (program.PricelistIds.Count > 0 && !program.PricelistIds.Select(p => p.Id).Contains(pricelistId)) {
            errorMessage = "This coupon is not available with the current pricelist.";
        }

        if (errorMessage != null) {
            return new { successful = false, payload = new { error_message = errorMessage } };
        }

        return new { successful = true, payload = new { program_id = program.Id, coupon_id = coupon.Id, coupon_partner_id = coupon.PartnerId?.Id, points = coupon.Points, has_source_order = coupon.HasSourceOrder() } };
    }

    private List<Loyalty.Program> GetProgramIds() {
        return Env.Search<Loyalty.Program>(p => p.PosOk == true && (p.PosConfigIds.Contains(this) || p.PosConfigIds.Count == 0)).ToList();
    }
}
