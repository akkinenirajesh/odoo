csharp
public partial class SaleOrder 
{
    public void ComputeSaleOrderTemplateId()
    {
        if (this.SaleOrderTemplateId == null)
        {
            var companyTemplate = Env.Company.SaleOrderTemplateId;
            if (companyTemplate != null && this.SaleOrderTemplateId != companyTemplate)
            {
                if (Env.WebsiteId != null)
                {
                    // Don't apply quotation template for order created via eCommerce
                    return;
                }
                this.SaleOrderTemplateId = Env.Company.SaleOrderTemplateId;
            }
        }
    }

    public void ComputeNote()
    {
        if (this.SaleOrderTemplateId != null)
        {
            var template = this.SaleOrderTemplateId.WithContext(lang: this.PartnerId.Lang);
            this.Note = template.Note;
        }
    }

    public void ComputeRequireSignature()
    {
        if (this.SaleOrderTemplateId != null)
        {
            this.RequireSignature = this.SaleOrderTemplateId.RequireSignature;
        }
    }

    public void ComputeRequirePayment()
    {
        if (this.SaleOrderTemplateId != null)
        {
            this.RequirePayment = this.SaleOrderTemplateId.RequirePayment;
        }
    }

    public void ComputePrepaymentPercent()
    {
        if (this.SaleOrderTemplateId != null && this.RequirePayment)
        {
            this.PrepaymentPercent = this.SaleOrderTemplateId.PrepaymentPercent;
        }
    }

    public void ComputeValidityDate()
    {
        if (this.SaleOrderTemplateId != null && this.SaleOrderTemplateId.NumberOfDays > 0)
        {
            this.ValidityDate = DateTime.Now.AddDays(this.SaleOrderTemplateId.NumberOfDays);
        }
    }

    public void ComputeJournalId()
    {
        if (this.SaleOrderTemplateId != null)
        {
            this.JournalId = this.SaleOrderTemplateId.JournalId;
        }
    }

    public void CheckOptionalProductCompanyId()
    {
        foreach (var option in this.SaleOrderOptionIds)
        {
            if (option.ProductId.CompanyId != null && option.ProductId.CompanyId != this.CompanyId)
            {
                var badProducts = this.SaleOrderOptionIds.Where(p => p.ProductId.CompanyId != null && p.ProductId.CompanyId != this.CompanyId).Select(p => p.ProductId.DisplayName);
                throw new Exception($"Your quotation contains products from company {string.Join(", ", badProducts)} whereas your quotation belongs to company {this.CompanyId.DisplayName}. Please change the company of your quotation or remove the products from other companies.");
            }
        }
    }

    public void OnChangeCompanyId()
    {
        if (this._OriginId != 0)
        {
            return;
        }
        ComputeSaleOrderTemplateId();
    }

    public void OnChangeSaleOrderTemplateId()
    {
        if (this.SaleOrderTemplateId == null)
        {
            return;
        }

        var saleOrderTemplate = this.SaleOrderTemplateId.WithContext(lang: this.PartnerId.Lang);
        this.OrderLine.Clear();

        foreach (var line in saleOrderTemplate.SaleOrderTemplateLineIds)
        {
            this.OrderLine.Add(line.PrepareOrderLineValues());
        }

        if (this.OrderLine.Count >= 2)
        {
            this.OrderLine[1].Sequence = -99;
        }

        this.SaleOrderOptionIds.Clear();

        foreach (var option in saleOrderTemplate.SaleOrderTemplateOptionIds)
        {
            this.SaleOrderOptionIds.Add(option.PrepareOptionLineValues());
        }
    }

    public void ActionConfirm()
    {
        var res = base.ActionConfirm();
        if (Env.IsSuperUser)
        {
            this = this.WithUser(Env.SuperUserId);
        }

        if (this.SaleOrderTemplateId != null && this.SaleOrderTemplateId.MailTemplateId != null)
        {
            this.MessagePostWithSource(this.SaleOrderTemplateId.MailTemplateId);
        }
        return res;
    }

    public void RecomputePrices()
    {
        base.RecomputePrices();
        this.SaleOrderOptionIds.ForEach(o => o.Discount = 0.0);
        this.SaleOrderOptionIds.ForEach(o => o.ComputePriceUnit());
        this.SaleOrderOptionIds.ForEach(o => o.ComputeDiscount());
    }

    public bool CanBeEditedOnPortal()
    {
        return this.State == "draft" || this.State == "sent";
    }
}
