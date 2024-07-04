csharp
public partial class SaleOrder {

    public decimal ComputeRewardTotal()
    {
        decimal rewardAmount = 0;
        foreach (var line in this.OrderLines)
        {
            if (line.RewardId == null)
            {
                continue;
            }
            if (line.RewardId.RewardType != LoyaltyRewardType.Product)
            {
                rewardAmount += line.PriceSubtotal;
            }
            else
            {
                rewardAmount -= line.ProductId.ListPrice * line.ProductUomQty;
            }
        }
        return rewardAmount;
    }

    public void ActionConfirm()
    {
        foreach (var order in Env.Context.Current.Get<SaleOrder>())
        {
            var allCoupons = order.AppliedCouponIds.Union(order.CouponPointIds.Select(cp => cp.CouponId))
                .Union(order.OrderLines.Where(l => l.CouponId != null).Select(l => l.CouponId));
            if (allCoupons.Any(coupon => GetRealPointsForCoupon(coupon) < 0))
            {
                throw new ValidationException(Env.Translate("One or more rewards on the sale order is invalid. Please check them."));
            }
            order.UpdateProgramsAndRewards();
        }

        // Remove any coupon from 'current' program that don't claim any reward.
        // This is to avoid ghost coupons that are lost forever.
        // Claiming a reward for that program will require either an automated check or a manual input again.
        var rewardCoupons = this.OrderLines.Where(l => l.CouponId != null).Select(l => l.CouponId);
        this.CouponPointIds.Where(pe => pe.CouponId.ProgramId.AppliesOn == LoyaltyProgramAppliesOn.Current && !rewardCoupons.Contains(pe.CouponId))
            .Select(pe => pe.CouponId).ToList()
            .ForEach(c => c.Sudo().Unlink());

        // Add/remove the points to our coupons
        foreach (var coupon in this.Where(s => s.State != SaleState.Sale)._GetPointChanges().Keys)
        {
            coupon.Points += this.Where(s => s.State != SaleState.Sale)._GetPointChanges()[coupon];
        }
        var res = Env.Invokable("sale.saleorder").Call("ActionConfirm", this);
        this.SendRewardCouponMail();
        Env.Context.Current.Result = res;
    }

    public void ActionCancel()
    {
        var previouslyConfirmed = this.Where(s => s.State == SaleState.Sale);
        var res = Env.Invokable("sale.saleorder").Call("_ActionCancel", this);
        // Add/remove the points to our coupons
        foreach (var coupon in previouslyConfirmed.Where(s => s.State != SaleState.Sale)._GetPointChanges().Keys)
        {
            coupon.Points -= previouslyConfirmed.Where(s => s.State != SaleState.Sale)._GetPointChanges()[coupon];
        }
        // Remove any rewards
        this.OrderLines.Where(l => l.IsRewardLine).ToList().ForEach(l => l.Unlink());
        this.CouponPointIds.Where(c => !c.ProgramId.IsNominative && c.OrderId.Contains(this) && c.UseCount == 0).Select(c => c.CouponId).ToList()
            .ForEach(c => c.Sudo().Unlink());
        this.CouponPointIds.ToList().ForEach(cp => cp.Unlink());
        Env.Context.Current.Result = res;
    }

    public void ActionOpenRewardWizard()
    {
        this.UpdateProgramsAndRewards();
        var claimableRewards = GetClaimableRewards();
        if (claimableRewards.Count == 1)
        {
            var coupon = claimableRewards.First().Key;
            var rewards = claimableRewards[coupon];
            if (rewards.Count == 1 && !rewards.First().MultiProduct)
            {
                ApplyProgramReward(rewards.First(), coupon);
                Env.Context.Current.Result = true;
                return;
            }
        }
        else if (claimableRewards.Count == 0)
        {
            Env.Context.Current.Result = true;
            return;
        }
        var action = Env.Invokable("ir.actions.actions").Call("_ForXmlId", "sale_loyalty.sale_loyalty_reward_wizard_action");
        Env.Context.Current.Result = action;
    }

    public void SendRewardCouponMail()
    {
        var coupons = Env.Get<LoyaltyCard>();
        foreach (var order in this)
        {
            coupons |= order.GetRewardCoupons();
        }
        if (coupons.Any())
        {
            coupons._SendCreationCommunication(true);
        }
    }

    public decimal GetDiscountableAmount(List<LoyaltyReward> rewardsToIgnore = null)
    {
        decimal discountable = 0;

        foreach (var line in this.OrderLines.Where(l => !this.GetNoEffectOnThresholdLines().Contains(l)))
        {
            if (rewardsToIgnore != null && rewardsToIgnore.Contains(line.RewardId))
            {
                continue;
            }
            if (line.ProductUomQty == 0 || line.PriceUnit == 0)
            {
                continue;
            }
            var taxData = line.TaxId.ComputeAll(
                line.PriceUnit,
                line.ProductUomQty,
                line.ProductId,
                line.OrderPartnerId
            );
            var taxes = line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed);
            discountable += taxData.TotalExcluded + taxes.Sum(tax => taxData.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
        }
        return discountable;
    }

    public Tuple<decimal, Dictionary<AccountTax, decimal>> GetDiscountableOrder(LoyaltyReward reward)
    {
        decimal discountable = 0;
        var discountablePerTax = new Dictionary<AccountTax, decimal>();

        if (reward.ProgramId.IsPaymentProgram)
        {
            var lines = this.OrderLines;
            foreach (var line in lines)
            {
                if (line.ProductUomQty == 0 || line.PriceUnit == 0)
                {
                    continue;
                }
                var discountedPriceUnit = line.PriceUnit * (1 - (line.Discount ?? 0) / 100);
                var taxData = line.TaxId.ComputeAll(
                    discountedPriceUnit,
                    line.ProductUomQty,
                    line.ProductId,
                    line.OrderPartnerId
                );
                var taxes = line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed);
                discountable += taxData.TotalExcluded + taxes.Sum(tax => taxData.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
                var linePrice = line.PriceUnit * line.ProductUomQty * (1 - (line.Discount ?? 0) / 100);
                discountablePerTax[taxes] += linePrice - taxes.Where(tax => tax.PriceInclude && !taxes.Contains(tax)).Sum(tax => taxData.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
            }
        }
        else
        {
            var lines = this.OrderLines.Where(l => !this.GetNoEffectOnThresholdLines().Contains(l));
            foreach (var line in lines)
            {
                if (line.ProductUomQty == 0 || line.PriceUnit == 0)
                {
                    continue;
                }
                var discountedPriceUnit = line.PriceUnit * (1 - (line.Discount ?? 0) / 100);
                var taxData = line.TaxId.ComputeAll(
                    discountedPriceUnit,
                    line.ProductUomQty,
                    line.ProductId,
                    line.OrderPartnerId
                );
                var taxes = line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed);
                discountable += taxData.TotalExcluded + taxes.Sum(tax => taxData.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
                var linePrice = line.PriceUnit * line.ProductUomQty * (1 - (line.Discount ?? 0) / 100);
                discountablePerTax[taxes] += linePrice - taxes.Where(tax => tax.PriceInclude && !taxes.Contains(tax)).Sum(tax => taxData.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
            }
        }
        return new Tuple<decimal, Dictionary<AccountTax, decimal>>(discountable, discountablePerTax);
    }

    public SaleOrderLine GetCheapestLine()
    {
        SaleOrderLine cheapestLine = null;
        foreach (var line in this.OrderLines.Where(l => !this.GetNoEffectOnThresholdLines().Contains(l)))
        {
            if (line.RewardId != null || line.ProductUomQty == 0 || line.PriceUnit == 0)
            {
                continue;
            }
            if (cheapestLine == null || cheapestLine.PriceUnit > line.PriceUnit)
            {
                cheapestLine = line;
            }
        }
        return cheapestLine;
    }

    public Tuple<decimal, Dictionary<AccountTax, decimal>> GetDiscountableCheapest(LoyaltyReward reward)
    {
        var cheapestLine = this.GetCheapestLine();
        if (cheapestLine == null)
        {
            return null;
        }
        var discountable = cheapestLine.PriceTotal;
        var discountablePerTaxes = cheapestLine.PriceUnit * (1 - (cheapestLine.Discount ?? 0) / 100);
        var taxes = cheapestLine.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed);

        return new Tuple<decimal, Dictionary<AccountTax, decimal>>(discountable, new Dictionary<AccountTax, decimal>() { { taxes, discountablePerTaxes } });
    }

    public List<SaleOrderLine> GetSpecificDiscountableLines(LoyaltyReward reward)
    {
        var discountableLines = Env.Get<SaleOrderLine>();
        foreach (var line in this.OrderLines.Where(l => !this.GetNoEffectOnThresholdLines().Contains(l)))
        {
            var domain = reward._GetDiscountProductDomain();
            if (line.RewardId == null && line.ProductId.FilteredDomain(domain))
            {
                discountableLines |= line;
            }
        }
        return discountableLines.ToList();
    }

    public Tuple<decimal, Dictionary<AccountTax, decimal>> GetDiscountableSpecific(LoyaltyReward reward)
    {
        var linesToDiscount = Env.Get<SaleOrderLine>();
        var discountLines = new Dictionary<string, List<SaleOrderLine>>();
        var orderLines = this.OrderLines.Where(l => !this.GetNoEffectOnThresholdLines().Contains(l)).ToList();
        var remainingAmountPerLine = new Dictionary<SaleOrderLine, decimal>();
        foreach (var line in orderLines)
        {
            if (line.ProductUomQty == 0 || line.PriceTotal == 0)
            {
                continue;
            }
            remainingAmountPerLine[line] = line.PriceTotal;
            var domain = reward._GetDiscountProductDomain();
            if (line.RewardId == null && line.ProductId.FilteredDomain(domain))
            {
                linesToDiscount |= line;
            }
            else if (line.RewardId.RewardType == LoyaltyRewardType.Discount)
            {
                if (!discountLines.ContainsKey(line.RewardIdentifierCode))
                {
                    discountLines.Add(line.RewardIdentifierCode, new List<SaleOrderLine>());
                }
                discountLines[line.RewardIdentifierCode].Add(line);
            }
        }

        orderLines = orderLines.Where(l => l.RewardId == null).ToList();
        SaleOrderLine cheapestLine = null;
        foreach (var lines in discountLines.Values)
        {
            var lineReward = lines.First().RewardId;
            var discountedLines = orderLines;
            if (lineReward.DiscountApplicability == LoyaltyRewardDiscountApplicability.Cheapest)
            {
                cheapestLine = cheapestLine ?? this.GetCheapestLine();
                discountedLines = cheapestLine;
            }
            else if (lineReward.DiscountApplicability == LoyaltyRewardDiscountApplicability.Specific)
            {
                discountedLines = this.GetSpecificDiscountableLines(lineReward);
            }
            if (!discountedLines.Any())
            {
                continue;
            }
            var commonLines = discountedLines.Intersect(linesToDiscount).ToList();
            if (lineReward.DiscountMode == LoyaltyRewardDiscountMode.Percent)
            {
                foreach (var line in discountedLines)
                {
                    if (lineReward.DiscountApplicability == LoyaltyRewardDiscountApplicability.Cheapest)
                    {
                        remainingAmountPerLine[line] *= (1 - lineReward.Discount / 100 / line.ProductUomQty);
                    }
                    else
                    {
                        remainingAmountPerLine[line] *= (1 - lineReward.Discount / 100);
                    }
                }
            }
            else
            {
                var nonCommonLines = discountedLines.Except(linesToDiscount).ToList();
                var discountedAmounts = lines.ToDictionary(line => line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed), line => Math.Abs(line.PriceTotal));
                foreach (var line in nonCommonLines.Concat(commonLines))
                {
                    decimal discountedAmount;
                    if (lines.First().RewardId.ProgramId.IsPaymentProgram)
                    {
                        discountedAmount = discountedAmounts[lines.First().TaxId.Where(t => t.AmountType != TaxAmountType.Fixed)];
                    }
                    else
                    {
                        discountedAmount = discountedAmounts[line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed)];
                    }
                    if (discountedAmount == 0)
                    {
                        continue;
                    }
                    var remaining = remainingAmountPerLine[line];
                    var consumed = Math.Min(remaining, discountedAmount);
                    if (lines.First().RewardId.ProgramId.IsPaymentProgram)
                    {
                        discountedAmounts[lines.First().TaxId.Where(t => t.AmountType != TaxAmountType.Fixed)] -= consumed;
                    }
                    else
                    {
                        discountedAmounts[line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed)] -= consumed;
                    }
                    remainingAmountPerLine[line] -= consumed;
                }
            }
        }

        decimal discountable = 0;
        var discountablePerTax = new Dictionary<AccountTax, decimal>();
        foreach (var line in linesToDiscount)
        {
            discountable += remainingAmountPerLine[line];
            var lineDiscountable = line.PriceUnit * line.ProductUomQty * (1 - (line.Discount ?? 0) / 100);
            var taxes = line.TaxId.Where(t => t.AmountType != TaxAmountType.Fixed);
            discountablePerTax[taxes] += lineDiscountable * (remainingAmountPerLine[line] / line.PriceTotal);
        }
        return new Tuple<decimal, Dictionary<AccountTax, decimal>>(discountable, discountablePerTax);
    }

    public List<SaleOrderLine> GetRewardValuesDiscount(LoyaltyReward reward, LoyaltyCard coupon)
    {
        // Figure out which lines are concerned by the discount
        decimal discountable = 0;
        var discountablePerTax = new Dictionary<AccountTax, decimal>();
        var rewardAppliesOn = reward.DiscountApplicability;
        var sequence = this.OrderLines.Where(x => !x.IsRewardLine).Max(x => x.Sequence) + 1;
        if (rewardAppliesOn == LoyaltyRewardDiscountApplicability.Order)
        {
            var discountableOrder = this.GetDiscountableOrder(reward);
            discountable = discountableOrder.Item1;
            discountablePerTax = discountableOrder.Item2;
        }
        else if (rewardAppliesOn == LoyaltyRewardDiscountApplicability.Specific)
        {
            var discountableSpecific = this.GetDiscountableSpecific(reward);
            discountable = discountableSpecific.Item1;
            discountablePerTax = discountableSpecific.Item2;
        }
        else if (rewardAppliesOn == LoyaltyRewardDiscountApplicability.Cheapest)
        {
            var discountableCheapest = this.GetDiscountableCheapest(reward);
            discountable = discountableCheapest.Item1;
            discountablePerTax = discountableCheapest.Item2;
        }
        if (discountable == 0)
        {
            if (!reward.ProgramId.IsPaymentProgram && this.OrderLines.Any(line => line.RewardId.ProgramId.IsPaymentProgram))
            {
                return new List<SaleOrderLine>() {
                    new SaleOrderLine() {
                        Name = Env.Translate("TEMPORARY DISCOUNT LINE"),
                        ProductId = reward.DiscountLineProductId,
                        PriceUnit = 0,
                        ProductUomQty = 0,
                        ProductUom = reward.DiscountLineProductId.UomId,
                        RewardId = reward,
                        CouponId = coupon,
                        PointsCost = 0,
                        RewardIdentifierCode = GenerateRandomRewardCode(),
                        Sequence = sequence,
                        TaxId = new List<AccountTax>()
                    }
                };
            }
            throw new UserError(Env.Translate("There is nothing to discount"));
        }
        var maxDiscount = reward.CurrencyId.Convert(reward.DiscountMaxAmount, this.CurrencyId, this.Company, Env.Context.Today());
        maxDiscount = Math.Min(this.AmountTotal, maxDiscount);
        if (reward.DiscountMode == LoyaltyRewardDiscountMode.PerPoint)
        {
            var points = this.GetRealPointsForCoupon(coupon);
            if (!reward.ProgramId.IsPaymentProgram)
            {
                points = points / reward.RequiredPoints * reward.RequiredPoints;
            }
            maxDiscount = Math.Min(maxDiscount, reward.CurrencyId.Convert(reward.Discount * points, this.CurrencyId, this.Company, Env.Context.Today()));
        }
        else if (reward.DiscountMode == LoyaltyRewardDiscountMode.PerOrder)
        {
            maxDiscount = Math.Min(maxDiscount, reward.CurrencyId.Convert(reward.Discount, this.CurrencyId, this.Company, Env.Context.Today()));
        }
        else if (reward.DiscountMode == LoyaltyRewardDiscountMode.Percent)
        {
            maxDiscount = Math.Min(maxDiscount, discountable * (reward.Discount / 100));
        }
        var rewardCode = GenerateRandomRewardCode();
        var pointCost = reward.RequiredPoints;
        if (reward.DiscountMode == LoyaltyRewardDiscountMode.PerPoint && !reward.ClearWallet)
        {
            var convertedDiscount = this.CurrencyId.Convert(Math.Min(maxDiscount, discountable), reward.CurrencyId, this.Company, Env.Context.Today());
            pointCost = convertedDiscount / reward.Discount;
        }
        var rewardLines = new List<SaleOrderLine>();
        if (reward.ProgramId.IsPaymentProgram)
        {
            rewardLines.Add(new SaleOrderLine()
            {
                Name = reward.Description,
                ProductId = reward.DiscountLineProductId,
                PriceUnit = -Math.Min(maxDiscount, discountable),
                ProductUomQty = 1,
                ProductUom = reward.DiscountLineProductId.UomId,
                RewardId = reward,
                CouponId = coupon,
                PointsCost = pointCost,
                RewardIdentifierCode = rewardCode,
                Sequence = sequence,
                TaxId = new List<AccountTax>()
            });
            if (reward.ProgramId.ProgramType == LoyaltyProgramType.GiftCard)
            {
                var taxesToApply = reward.DiscountLineProductId.TaxesId.Where(t => t.CompanyId.Contains(this.Company)).ToList();
                if (taxesToApply.Any())
                {
                    var mappedTaxes = this.FiscalPositionId.MapTax(taxesToApply);
                    var priceInclTaxes = mappedTaxes.Where(t => t.PriceInclude);
                    var taxRes = mappedTaxes.WithContext(force_price_include: true, round: false, round_base: false).ComputeAll(
                        rewardLines.First().PriceUnit,
                        this.CurrencyId
                    );
                    var newPrice = taxRes.TotalExcluded;
                    newPrice += priceInclTaxes.Sum(tax => taxRes.Taxes.Where(t => t.Id == tax.Id).Sum(t => t.Amount));
                    rewardLines.First().PriceUnit = newPrice;
                    rewardLines.First().TaxId = mappedTaxes.ToList();
                }
            }
        }
        else
        {
            foreach (var tax in discountablePerTax.Keys)
            {
                if (discountablePerTax[tax] == 0)
                {
                    continue;
                }
                var mappedTaxes = this.FiscalPositionId.MapTax(tax);
                var taxDesc = "";
                if (mappedTaxes.Any(t => !string.IsNullOrEmpty(t.Name)))
                {
                    taxDesc = Env.Translate(" - On product with the following taxes: %(taxes)s", new Dictionary<string, object>() { { "taxes", string.Join(", ", mappedTaxes.Select(t => t.Name)) } });
                }
                rewardLines.Add(new SaleOrderLine()
                {
                    Name = Env.Translate("Discount: %(desc)s%(tax_str)s", new Dictionary<string, object>() { { "desc", reward.Description }, { "tax_str", taxDesc } }),
                    ProductId = reward.DiscountLineProductId,
                    PriceUnit = -(discountablePerTax[tax] * (maxDiscount / discountable)),
                    ProductUomQty = 1,
                    ProductUom = reward.DiscountLineProductId.UomId,
                    RewardId = reward,
                    CouponId = coupon,
                    PointsCost = 0,
                    RewardIdentifierCode = rewardCode,
                    Sequence = sequence,
                    TaxId = mappedTaxes.ToList()
                });
            }
            if (rewardLines.Any())
            {
                rewardLines.First().PointsCost = pointCost;
            }
        }
        return rewardLines;
    }

    public List<SaleOrderLine> GetRewardValuesProduct(LoyaltyReward reward, LoyaltyCard coupon, Product product = null)
    {
        var rewardProducts = reward.RewardProductIds;
        product = product ?? rewardProducts.FirstOrDefault();
        if (product == null || !rewardProducts.Contains(product))
        {
            throw new UserError(Env.Translate("Invalid product to claim."));
        }
        var taxes = this.FiscalPositionId.MapTax(product.TaxesId.Where(t => t.CompanyId.Contains(this.Company)).ToList());
        var points = this.GetRealPointsForCoupon(coupon);
        var claimableCount = (decimal)Math.Floor(points / reward.RequiredPoints);
        var cost = reward.ClearWallet ? points : claimableCount * reward.RequiredPoints;
        return new List<SaleOrderLine>() {
            new SaleOrderLine()
            {
                Name = Env.Translate("Free Product - %(product)s", new Dictionary<string, object>() { { "product", product.WithContext(display_default_code: false).DisplayName } }),
                ProductId = product,
                Discount = 100,
                ProductUomQty = reward.RewardProductQty * claimableCount,
                RewardId = reward,
                CouponId = coupon,
                PointsCost = cost,
                RewardIdentifierCode = GenerateRandomRewardCode(),
                ProductUom = product.UomId,
                Sequence = this.OrderLines.Where(x => !x.IsRewardLine).Max(x => x.Sequence) + 1,
                TaxId = taxes.ToList()
            }
        };
    }

    public List<SaleOrderLine> GetNoEffectOnThresholdLines()
    {
        return this.OrderLines.Where(l => l.RewardId == null).ToList();
    }

    public SaleOrder Copy(Dictionary<string, object> defaultValues = null)
    {
        var newOrders = Env.Invokable("sale.saleorder").Call("Copy", this, defaultValues);
        var rewardLines = newOrders.OrderLines.Where(l => l.IsRewardLine).ToList();
        if (rewardLines.Any())
        {
            rewardLines.ForEach(l => l.Unlink());
        }
        return newOrders;
    }

    public Dictionary<string, object> GetProgramDomain()
    {
        var today = Env.Context.Today();
        return new Dictionary<string, object>() {
            { "Active", true },
            { "SaleOk", true },
            { "Company", new List<long>() { this.Company.Id, 0 } },
            { "PriceListIds", new List<long>() { 0, this.PriceListId.Id } },
            { "DateFrom", new List<object>() { 0, today } },
            { "DateTo", new List<object>() { 0, today } }
        };
    }

    public Dictionary<string, object> GetTriggerDomain()
    {
        var today = Env.Context.Today();
        return new Dictionary<string, object>() {
            { "Active", true },
            { "ProgramId.SaleOk", true },
            { "Company", new List<long>() { this.Company.Id, 0 } },
            { "ProgramId.PriceListIds", new List<long>() { 0, this.PriceListId.Id } },
            { "ProgramId.DateFrom", new List<object>() { 0, today } },
            { "ProgramId.DateTo", new List<object>() { 0, today } }
        };
    }

    public Dictionary<LoyaltyProgram, decimal> GetApplicableProgramPoints(Dictionary<string, object> domain =