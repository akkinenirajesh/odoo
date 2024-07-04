csharp
public partial class L10nSaEdiAccountEdiFormat
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public bool Active { get; set; }
    public int Sequence { get; set; }
    public bool IsReverse { get; set; }
    public string EDIFileFormat { get; set; }
    public string EDIFileType { get; set; }
    public string EDIAttachmentFilenameTemplate { get; set; }
    public string EDIAttachmentFilenamePattern { get; set; }
    public string EDIAttachmentMimeType { get; set; }
    public string EDIAttachmentDescriptionTemplate { get; set; }
    public string EDIAttachmentDescriptionPattern { get; set; }
    public string EDIAttachmentDescription { get; set; }
    public bool InvoiceLine { get; set; }
    public bool Partner { get; set; }
    public bool PartnerAddress { get; set; }
    public bool InvoiceHeader { get; set; }
    public bool InvoiceTotals { get; set; }
    public bool InvoiceDates { get; set; }
    public bool InvoiceTaxes { get; set; }
    public bool InvoicePaymentTerms { get; set; }
    public bool InvoicePaymentMethods { get; set; }
    public bool InvoiceAttachments { get; set; }
    public bool InvoiceOther { get; set; }
    public string EDIWebServiceURL { get; set; }
    public string EDIWebServiceUsername { get; set; }
    public string EDIWebServicePassword { get; set; }
    public string EDISenderID { get; set; }
    public string EDIRecieverID { get; set; }
    public int EDIWebServiceTimeout { get; set; }
    public int EDIWebServiceRetryCount { get; set; }
    public int EDIWebServiceRetryDelay { get; set; }
    public int EDIWebServiceRetryInterval { get; set; }
    public int EDIWebServiceRetryBackoff { get; set; }
    
    // all other methods

    public virtual bool IsRequiredForInvoice(AccountMove invoice)
    {
        if (Code != "sa_zatca")
        {
            return Env.Call("account.edi.format", "_is_required_for_invoice", new object[] { invoice });
        }

        return invoice.IsSaleDocument() && invoice.CountryCode == "SA";
    }

    public virtual Dictionary<string, object> CheckMoveConfiguration(AccountMove invoice)
    {
        var errors = Env.Call("account.edi.format", "_check_move_configuration", new object[] { invoice });
        if (Code != "sa_zatca" || invoice.Company.Country.Code != "SA")
        {
            return errors;
        }

        if (invoice.CommercialPartner == invoice.Company.Partner.CommercialPartner)
        {
            errors.Add(Env.Translate("- You cannot post invoices where the Seller is the Buyer"));
        }

        if (!invoice.InvoiceLineIds.Where(line => line.DisplayType == "product").All(line => line.TaxIds.Count > 0))
        {
            errors.Add(Env.Translate("- Invoice lines should have at least one Tax applied."));
        }

        if (!invoice.Journal.L10nSaReadyToSubmitEinvoices())
        {
            errors.Add(Env.Translate("- Finish the Onboarding procees for journal %s by requesting the CSIDs and completing the checks.", invoice.Journal.Name));
        }

        if (!invoice.Company.L10nSaCheckOrganizationUnit())
        {
            errors.Add(Env.Translate("- The company VAT identification must contain 15 digits, with the first and last digits being '3' as per the BR-KSA-39 and BR-KSA-40 of ZATCA KSA business rule."));
        }
        if (invoice.Company.L10nSaPrivateKey == null)
        {
            errors.Add(Env.Translate("- No Private Key was generated for company %s. A Private Key is mandatory in order to generate Certificate Signing Requests (CSR).", invoice.Company.Name));
        }
        if (invoice.Journal.L10nSaSerialNumber == null)
        {
            errors.Add(Env.Translate("- No Serial Number was assigned for journal %s. A Serial Number is mandatory in order to generate Certificate Signing Requests (CSR).", invoice.Journal.Name));
        }

        var supplierMissingInfo = CheckSellerMissingInfo(invoice);
        var customerMissingInfo = CheckBuyerMissingInfo(invoice);

        if (supplierMissingInfo.Count > 0)
        {
            errors.Add(Env.Translate("- Please, set the following fields on the Supplier: %(missing_fields)s", new { missing_fields = string.Join(", ", supplierMissingInfo) }));
        }
        if (customerMissingInfo.Count > 0)
        {
            errors.Add(Env.Translate("- Please, set the following fields on the Customer: %(missing_fields)s", new { missing_fields = string.Join(", ", customerMissingInfo) }));
        }
        if (invoice.InvoiceDate > Env.Context.Today(invoice))
        {
            errors.Add(Env.Translate("- Please, make sure the invoice date is set to either the same as or before Today."));
        }
        if ((invoice.MoveType == "in_refund" || invoice.MoveType == "out_refund") && !invoice.L10nSaCheckRefundReason())
        {
            errors.Add(Env.Translate("- Please, make sure either the Reversed Entry or the Reversal Reason are specified when confirming a Credit/Debit note"));
        }
        return errors;
    }

    public virtual bool NeedsWebServices()
    {
        if (Code != "sa_zatca")
        {
            return Env.Call("account.edi.format", "_needs_web_services");
        }

        return true;
    }

    public virtual bool IsCompatibleWithJournal(AccountJournal journal)
    {
        if (Code != "sa_zatca")
        {
            return Env.Call("account.edi.format", "_is_compatible_with_journal", new object[] { journal });
        }

        return journal.Type == "sale" && journal.CountryCode == "SA";
    }

    public virtual byte[] GetInvoiceContentEdi(AccountMove invoice)
    {
        var doc = invoice.EDIDocumentIds.Where(d => d.EdiFormat.Code == "sa_zatca" && d.State == "sent").FirstOrDefault();
        if (doc != null && doc.Attachment != null)
        {
            return doc.Attachment.Raw;
        }
        return System.Text.Encoding.UTF8.GetBytes(GenerateZatcaTemplate(invoice));
    }

    public virtual Dictionary<string, object> GetMoveApplicability(AccountMove move)
    {
        if (Code != "sa_zatca" || move.CountryCode != "SA" || move.MoveType != "out_invoice" && move.MoveType != "out_refund")
        {
            return Env.Call("account.edi.format", "_get_move_applicability", new object[] { move });
        }
        return new Dictionary<string, object>()
        {
            {"post", new Action<AccountMove>(PostZatcaEdi)},
            {"edi_content", new Func<AccountMove, byte[]>(GetInvoiceContentEdi)},
        };
    }

    public virtual List<string> CheckSellerMissingInfo(AccountMove invoice)
    {
        var partner = invoice.Company.Partner.CommercialPartner;
        var fieldsToCheck = new List<Tuple<string, string, Func<ResPartner, string, bool>>>()
        {
            Tuple.Create("L10nSaEdiBuildingNumber", Env.Translate("Building Number for the Buyer is required on Standard Invoices"), null),
            Tuple.Create("Street2", Env.Translate("Neighborhood for the Seller is required on Standard Invoices"), null),
            Tuple.Create("L10nSaAdditionalIdentificationScheme", Env.Translate("Additional Identification Scheme is required for the Seller, and must be one of CRN, MOM, MLS, SAG or OTH"), (p, v) => v == "CRN" || v == "MOM" || v == "MLS" || v == "SAG" || v == "OTH"),
            Tuple.Create("Vat", Env.Translate("VAT is required when Identification Scheme is set to Tax Identification Number"), (p, v) => p.L10nSaAdditionalIdentificationScheme != "TIN"),
            Tuple.Create("State", Env.Translate("State / Country subdivision"), null),
        };
        return CheckPartnerMissingInfo(partner, fieldsToCheck);
    }

    public virtual List<string> CheckBuyerMissingInfo(AccountMove invoice)
    {
        var fieldsToCheck = new List<Tuple<string, string, Func<ResPartner, string, bool>>>()
        {
        };
        if (invoice.InvoiceLineIds.Where(line => line.DisplayType == "product").Any(line => line.TaxIds.Any(tax => tax.L10nSaExemptionReasonCode == "VATEX-SA-HEA" || tax.L10nSaExemptionReasonCode == "VATEX-SA-EDU")))
        {
            fieldsToCheck.Add(Tuple.Create("L10nSaAdditionalIdentificationScheme", Env.Translate("Additional Identification Scheme is required for the Buyer if tax exemption reason is either VATEX-SA-HEA or VATEX-SA-EDU, and its value must be NAT"), (p, v) => v == "NAT"));
            fieldsToCheck.Add(Tuple.Create("L10nSaAdditionalIdentificationNumber", Env.Translate("Additional Identification Number is required for commercial partners"), (p, v) => p.L10nSaAdditionalIdentificationScheme != "TIN"));
        }
        else if (invoice.CommercialPartner.L10nSaAdditionalIdentificationScheme == "TIN")
        {
            fieldsToCheck.Add(Tuple.Create("Vat", Env.Translate("VAT is required when Identification Scheme is set to Tax Identification Number")));
        }
        if (!invoice.L10nSaIsSimplified() && invoice.Partner.Country.Code == "SA")
        {
            fieldsToCheck.Add(Tuple.Create("L10nSaEdiBuildingNumber", Env.Translate("Building Number for the Buyer is required on Standard Invoices"), null));
            fieldsToCheck.Add(Tuple.Create("Street2", Env.Translate("Neighborhood for the Buyer is required on Standard Invoices"), null));
        }
        return CheckPartnerMissingInfo(invoice.CommercialPartner, fieldsToCheck);
    }

    public virtual List<string> CheckPartnerMissingInfo(ResPartner partner, List<Tuple<string, string, Func<ResPartner, string, bool>>> fieldsToCheck)
    {
        var missing = new List<string>();
        foreach (var field in fieldsToCheck)
        {
            var fieldValue = partner[field.Item1];
            if (fieldValue == null || (field.Item3 != null && !field.Item3(partner, fieldValue)))
            {
                missing.Add(field.Item2);
            }
        }
        return missing;
    }

    public virtual Dictionary<string, object> PostZatcaEdi(AccountMove invoice)
    {
        // Chain integrity check: chain head must have been REALLY posted, and did not time out
        // When a submission times out, we reset the chain index of the invoice to False, so it has to be submitted again
        // According to ZATCA, if we end up submitting the same invoice more than once, they will directly reach out
        // to the taxpayer for clarifications
        var chainHead = invoice.Journal.L10nSaGetLastPostedInvoice();
        if (chainHead != null && chainHead != invoice && !chainHead.L10nSaIsInChain())
        {
            return new Dictionary<string, object>()
            {
                {invoice, new Dictionary<string, object>()
                    {
                        {"error", Env.Translate(f"ZATCA: Cannot post invoice while chain head ({chainHead.Name}) has not been posted")},
                        {"blocking_level", "error"},
                        {"response", null},
                    }}
            };
        }

        byte[] xmlContent = null;
        if (invoice.L10nSaChainIndex == null)
        {
            // If the Invoice doesn't have a chain index, it means it either has not been submitted before,
            // or it was submitted and rejected. Either way, we need to assign it a new Chain Index and regenerate
            // the data that depends on it before submitting (UUID, XML content, signature)
            invoice.L10nSaChainIndex = invoice.Journal.L10nSaEdiGetNextChainIndex();
            xmlContent = invoice.L10nSaGenerateUnsignedData();
        }

        // Generate Invoice name for attachment
        var attachmentName = Env.Call("account.edi.xml.ubl_21.zatca", "_export_invoice_filename", new object[] { invoice });

        // Generate XML, sign it, then submit it to ZATCA
        var responseData = new Dictionary<string, object>();
        var submittedXml = new byte[0];
        try
        {
            responseData = ExportZatcaInvoice(invoice, xmlContent);
            submittedXml = (byte[])responseData["signed_xml"];
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>()
            {
                {invoice, new Dictionary<string, object>()
                    {
                        {"error", Env.Translate(ex.Message)},
                        {"blocking_level", "error"},
                        {"response", xmlContent},
                    }}
            };
        }

        // Check for submission errors
        if (responseData.ContainsKey("error"))
        {
            // If the request was rejected, we save the signed xml content as an attachment
            if (responseData.ContainsKey("rejected") && (bool)responseData["rejected"])
            {
                invoice.L10nSaLogResults(submittedXml, responseData, true);
            }

            // If the request returned an exception (Timeout, ValueError... etc.) it means we're not sure if the
            // invoice was successfully cleared/reported, and thus we keep the Index Chain.
            // Else, we recalculate the submission Index (ICV), UUID, XML content and Signature
            if (!responseData.ContainsKey("excepted") || !(bool)responseData["excepted"])
            {
                invoice.L10nSaChainIndex = null;
            }

            return new Dictionary<string, object>()
            {
                {invoice, new Dictionary<string, object>()
                    {
                        {"error", responseData["error"]},
                        {"rejected", responseData["rejected"]},
                        {"response", submittedXml},
                        {"blocking_level", responseData["blocking_level"]},
                    }}
            };
        }

        // Once submission is done with no errors, check submission status
        var clearedXml = PostprocessEinvoiceSubmission(invoice, submittedXml, responseData);

        // Save the submitted/returned invoice XML content once the submission has been completed successfully
        invoice.L10nSaLogResults(clearedXml, responseData);
        return new Dictionary<string, object>()
        {
            {invoice, new Dictionary<string, object>()
                {
                    {"success", true},
                    {"response", clearedXml},
                    {"message", ""},
                    {"attachment", Env.Call("ir.attachment", "create", new object[]
                        {
                            new Dictionary<string, object>()
                            {
                                {"name", attachmentName},
                                {"raw", clearedXml},
                                {"res_model", "account.move"},
                                {"res_id", invoice.Id},
                                {"mimetype", "application/xml"}
                            }
                        })},
                }}
        };
    }

    public virtual Dictionary<string, object> ExportZatcaInvoice(AccountMove invoice, byte[] xmlContent = null)
    {
        // Prepare UBL invoice values and render XML file
        var unsignedXml = xmlContent != null ? System.Text.Encoding.UTF8.GetString(xmlContent) : GenerateZatcaTemplate(invoice);

        // Load PCISD data and X509 certificate
        var pcsidData = new Dictionary<string, object>();
        try
        {
            pcsidData = invoice.Journal.L10nSaApiGetPcsid();
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>()
            {
                {"error", Env.Translate("Could not generate PCSID values: \n") + ex.Message},
                {"blocking_level", "error"},
                {"response", unsignedXml},
            };
        }
        var x509Cert = (string)pcsidData["binarySecurityToken"];

        // Apply Signature/QR code on the generated XML document
        var signedXml = new byte[0];
        try
        {
            signedXml = GetSignedXml(invoice, unsignedXml, x509Cert);
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>()
            {
                {"error", Env.Translate("Could not generate signed XML values: \n") + ex.Message},
                {"blocking_level", "error"},
                {"response", unsignedXml},
            };
        }

        // Once the XML content has been generated and signed, we submit it to ZATCA
        var result = SubmitEinvoice(invoice, System.Text.Encoding.UTF8.GetString(signedXml), pcsidData);

        return new Dictionary<string, object>()
        {
            {"signed_xml", signedXml},
            {"error", result.ContainsKey("error") ? result["error"] : null},
            {"rejected", result.ContainsKey("rejected") ? result["rejected"] : false},
            {"response", result.ContainsKey("response") ? result["response"] : null},
            {"blocking_level", result.ContainsKey("blocking_level") ? result["blocking_level"] : null},
        };
    }

    public virtual string GenerateZatcaTemplate(AccountMove invoice)
    {
        var xmlContent = (string)Env.Call("account.edi.xml.ubl_21.zatca", "_export_invoice", new object[] { invoice });
        if (xmlContent == null)
        {
            return null;
        }
        return PostprocessZatcaTemplate(xmlContent);
    }

    public virtual string PostprocessZatcaTemplate(string xmlContent)
    {
        // Append UBLExtensions to the XML content
        var ublExtensions = (string)Env.Call("ir.qweb", "_render", new object[] { "l10n_sa_edi.export_sa_zatca_ubl_extensions" });
        var root = System.Xml.Linq.XElement.Parse(xmlContent);
        root.AddBeforeSelf(System.Xml.Linq.XElement.Parse(ublExtensions));

        // Force xmlns:ext namespace on UBl file
        var nsMap = new Dictionary<string, string>()
        {
            {"ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"},
        };
        System.Xml.Linq.XNamespace.Register("ext", nsMap["ext"]);

        return root.ToString();
    }

    public virtual string GetSignedXml(AccountMove invoice, string unsignedXml, string x509Cert)
    {
        var signedXml = SignXml(unsignedXml, x509Cert, invoice.L10nSaInvoiceSignature);
        if (invoice.L10nSaIsSimplified())
        {
            return ApplyQrCode(invoice, signedXml);
        }
        return signedXml;
    }

    public virtual string SignXml(string xmlContent, string certificateStr, string signature)
    {
        // TODO: Implement XML signing logic
        throw new NotImplementedException();
    }

    public virtual string ApplyQrCode(AccountMove invoice, string xmlContent)
    {
        // TODO: Implement QR code application logic
        throw new NotImplementedException();
    }

    public virtual Dictionary<string, object> SubmitEinvoice(AccountMove invoice, string signedXml, Dictionary<string, object> pcsidData)
    {
        // TODO: Implement ZATCA API call logic
        throw new NotImplementedException();
    }

    public virtual string PostprocessEinvoiceSubmission(AccountMove invoice, byte[] signedXml, Dictionary<string, object> clearanceData)
    {
        if (invoice.L10nSaIsSimplified())
        {
            // if invoice is B2C, it is a SIMPLIFIED invoice, and thus it is only reported and returns
            // no signed invoice. In this case, we just return the original content
            return System.Text.Encoding.UTF8.GetString(signedXml);
        }
        return System.Text.Encoding.UTF8.GetString((byte[])clearanceData["clearedInvoice"]);
    }

    public virtual string _l10n_sa_get_zatca_datetime(DateTime timestamp)
    {
        // TODO: Implement ZATCA datetime conversion logic
        throw new NotImplementedException();
    }

    public virtual string _l10n_sa_xml_node_content(System.Xml.Linq.XElement root, string xpath, Dictionary<string, string> namespaces = null)
    {
        // TODO: Implement XML node content retrieval logic
        throw new NotImplementedException();
    }

    public virtual string _l10n_sa_calculate_signed_properties_hash(string issuerName, string serialNumber, string signingTime, string publicKey)
    {
        // TODO: Implement SignedProperties hash calculation logic
        throw new NotImplementedException();
    }

    public virtual string _l10n_sa_assert_clearance_status(AccountMove invoice, Dictionary<string, object> clearanceData)
    {
        // TODO: Implement Clearance status assertion logic
        throw new NotImplementedException();
    }

    public virtual string _l10n_sa_generate_invoice_xml_hash(string xmlContent, string algorithm)
    {
        // TODO: Implement invoice XML hash generation logic
        throw new NotImplementedException();
    }

    public virtual void _l10n_sa_log_results(byte[] xmlContent, Dictionary<string, object> responseData, bool error = false)
    {
        // TODO: Implement result logging logic
        throw new NotImplementedException();
    }

    public virtual bool _l10n_sa_is_simplified(AccountMove invoice)
    {
        // TODO: Implement Simplified invoice check logic
        throw new NotImplementedException();
    }

    public virtual byte[] _l10n_sa_generate_unsigned_data(AccountMove invoice)
    {
        // TODO: Implement unsigned data generation logic
        throw new NotImplementedException();
    }

    public virtual bool _l10n_sa_check_refund_reason(AccountMove invoice)
    {
        // TODO: Implement Refund reason check logic
        throw new NotImplementedException();
    }

    public virtual bool _l10n_sa_check_organization_unit(Company company)
    {
        // TODO: Implement Organization Unit check logic
        throw new NotImplementedException();
    }
}
