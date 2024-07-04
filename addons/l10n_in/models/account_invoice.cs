csharp
public partial class AccountMove
{
    public void ComputeL10nInGstTreatment()
    {
        if (this.CountryCode == "IN" && this.State == "Draft")
        {
            var gstTreatment = this.Partner.L10nInGstTreatment;
            if (string.IsNullOrEmpty(gstTreatment))
            {
                gstTreatment = "Unregistered";
                if (this.Partner.Country.Code == "IN" && !string.IsNullOrEmpty(this.Partner.Vat))
                {
                    gstTreatment = "Regular";
                }
                else if (this.Partner.Country != null && this.Partner.Country.Code != "IN")
                {
                    gstTreatment = "Overseas";
                }
            }
            this.L10nInGstTreatment = gstTreatment;
        }
        else
        {
            this.L10nInGstTreatment = null;
        }
    }

    public void ComputeL10nInStateId()
    {
        if (this.CountryCode == "IN" && this.Journal.Type == "Sale")
        {
            var partnerState = (this.Partner.CommercialPartnerId == this.PartnerShipping.CommercialPartnerId
                && this.PartnerShipping.State != null)
                ? this.PartnerShipping.State
                : this.Partner.State;

            if (partnerState == null)
            {
                partnerState = this.Partner.CommercialPartner.State ?? this.Company.State;
            }

            string countryCode = partnerState?.Country?.Code ?? this.CountryCode;
            if (countryCode == "IN")
            {
                this.L10nInStateId = partnerState;
            }
            else
            {
                this.L10nInStateId = Env.Ref<Core.ResCountryState>("l10n_in.state_in_oc");
            }
        }
        else if (this.CountryCode == "IN" && this.Journal.Type == "Purchase")
        {
            this.L10nInStateId = this.Company.State;
        }
        else
        {
            this.L10nInStateId = null;
        }
    }

    public Dictionary<string, object> OnchangeNameWarning()
    {
        if (this.CountryCode == "IN" && this.Journal.Type == "Sale" && !string.IsNullOrEmpty(this.Name)
            && (this.Name.Length > 16 || !System.Text.RegularExpressions.Regex.IsMatch(this.Name, @"^[a-zA-Z0-9-\/]+$")))
        {
            return new Dictionary<string, object>
            {
                ["warning"] = new
                {
                    title = "Invalid sequence as per GST rule 46(b)",
                    message = "The invoice number should not exceed 16 characters\n" +
                              "and must only contain '-' (hyphen) and '/' (slash) as special characters"
                }
            };
        }
        return null;
    }

    public string GetNameInvoiceReport()
    {
        if (this.CountryCode == "IN")
        {
            return "l10n_in.l10n_in_report_invoice_document_inherit";
        }
        return base.GetNameInvoiceReport();
    }

    public void Post(bool soft = true)
    {
        base.Post(soft);

        if (this.CountryCode == "IN" && this.IsSaleDocument())
        {
            if (this.L10nInStateId != null && string.IsNullOrEmpty(this.L10nInStateId.L10nInTin))
            {
                throw new UserException($"Please set a valid TIN Number on the Place of Supply {this.L10nInStateId.Name}");
            }

            if (this.Company.State == null)
            {
                string msg = $"Your company {this.Company.Name} needs to have a correct address in order to validate this invoice.\n" +
                             "Set the address of your company (Don't forget the State field)";
                var action = new Dictionary<string, object>
                {
                    ["view_mode"] = "form",
                    ["res_model"] = "res.company",
                    ["type"] = "ir.actions.act_window",
                    ["res_id"] = this.Company.Id,
                    ["views"] = new[] { new[] { Env.Ref<Ir.Model.Data>("base.view_company_form").Id, "form" } }
                };
                throw new RedirectWarningException(msg, action, "Go to Company configuration");
            }

            this.L10nInGstin = this.Partner.Vat;
            if (string.IsNullOrEmpty(this.L10nInGstin) && new[] { "Regular", "Composition", "SpecialEconomicZone", "DeemedExport" }.Contains(this.L10nInGstTreatment))
            {
                throw new ValidationException($"Partner {this.Partner.Name} ({this.Partner.Id}) GSTIN is required under GST Treatment {this.L10nInGstTreatment}");
            }
        }
    }

    public Core.ResPartner L10nInGetWarehouseAddress()
    {
        // TO OVERRIDE
        return null;
    }

    public string GenerateQrCode(bool silentErrors = false)
    {
        if (this.Company.CountryCode == "IN")
        {
            string paymentUrl = $"upi://pay?pa={this.Company.L10nInUpiId}&pn={this.Company.Name}&am={this.AmountResidual}&tr={this.PaymentReference ?? this.Name}&tn=Payment for {this.Name}";
            byte[] barcode = Env.Get<Ir.Actions.Report>().Barcode("QR", paymentUrl, 120, 120);
            return ImageDataUri(Convert.ToBase64String(barcode));
        }
        return base.GenerateQrCode(silentErrors);
    }

    public Dictionary<string, object> L10nInGetHsnSummaryTable()
    {
        bool displayUom = Env.User.HasGroup("uom.group_uom");

        var baseLines = new List<Dictionary<string, object>>();
        foreach (var line in this.InvoiceLines.Where(x => x.DisplayType == "product"))
        {
            var taxesData = line.Taxes.ConvertToDictForTaxesComputation();
            var productValues = Env.Get<Account.Tax>().EvalTaxesComputationTurnToProductValues(
                taxesData,
                product: line.Product
            );

            baseLines.Add(new Dictionary<string, object>
            {
                ["l10n_in_hsn_code"] = line.L10nInHsnCode,
                ["quantity"] = line.Quantity,
                ["price_unit"] = line.PriceUnit,
                ["product_values"] = productValues,
                ["uom"] = new { id = line.ProductUom.Id, name = line.ProductUom.Name },
                ["taxes_data"] = taxesData
            });
        }
        return Env.Get<Account.Tax>().L10nInGetHsnSummaryTable(baseLines, displayUom);
    }
}
