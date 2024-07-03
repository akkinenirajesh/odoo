csharp
public partial class OnboardingStep
{
    public ActionResult ActionOpenStepCompanyData()
    {
        var company = Env.Get<AccountJournal>().Browse(Context.GetValueOrDefault("JournalId", null))?.CompanyId ?? Env.Company;
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Set your company data",
            ResModel = "res.company",
            ResId = company.Id,
            Views = new List<object> { new List<object> { Env.Ref("account.res_company_form_view_onboarding").Id, "form" } },
            Target = "new"
        };
    }

    public ActionResult ActionOpenStepBaseDocumentLayout()
    {
        var viewId = Env.Ref("web.view_base_document_layout").Id;
        return new ActionResult
        {
            Name = "Configure your document layout",
            Type = "ir.actions.act_window",
            ResModel = "base.document.layout",
            Target = "new",
            Views = new List<object> { new List<object> { viewId, "form" } },
            Context = new Dictionary<string, object> { { "dialog_size", "extra-large" } }
        };
    }

    public bool ActionValidateStepBaseDocumentLayout()
    {
        var step = Env.Ref("account.onboarding_onboarding_step_base_document_layout", false);
        if (step == null || Env.Company.ExternalReportLayoutId == null)
        {
            return false;
        }
        return ActionValidateStep("account.onboarding_onboarding_step_base_document_layout");
    }

    public ActionResult ActionOpenStepBankAccount()
    {
        return Env.Company.SettingInitBankAccountAction();
    }

    public ActionResult ActionOpenStepCreateInvoice()
    {
        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Create first invoice",
            Views = new List<object> { new List<object> { Env.Ref("account.view_move_form").Id, "form" } },
            ResModel = "account.move",
            Context = new Dictionary<string, object> { { "default_move_type", "out_invoice" } }
        };
    }

    public ActionResult ActionOpenStepFiscalYear()
    {
        var company = Env.Get<AccountJournal>().Browse(Context.GetValueOrDefault("JournalId", null))?.CompanyId ?? Env.Company;
        var newWizard = Env.Get<AccountFinancialYearOp>().Create(new { CompanyId = company.Id });
        var viewId = Env.Ref("account.setup_financial_year_opening_form").Id;

        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Accounting Periods",
            ViewMode = "form",
            ResModel = "account.financial.year.op",
            Target = "new",
            ResId = newWizard.Id,
            Views = new List<object> { new List<object> { viewId, "form" } },
            Context = new Dictionary<string, object> { { "dialog_size", "medium" } }
        };
    }

    public ActionResult ActionOpenStepChartOfAccounts()
    {
        var company = Env.Get<AccountJournal>().Browse(Context.GetValueOrDefault("JournalId", null))?.CompanyId ?? Env.Company;
        this.Sudo().WithCompany(company).ActionValidateStep("account.onboarding_onboarding_step_chart_of_accounts");

        if (company.OpeningMovePosted())
        {
            return "account.action_account_form";
        }

        var viewId = Env.Ref("account.init_accounts_tree").Id;
        var domain = new List<object>
        {
            Env.Get<AccountAccount>().CheckCompanyDomain(company),
            new List<object> { "AccountType", "!=", "equity_unaffected" }
        };

        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Chart of Accounts",
            ResModel = "account.account",
            ViewMode = "tree",
            Limit = 99999999,
            SearchViewId = new List<object> { Env.Ref("account.view_account_search").Id },
            Views = new List<object> { new List<object> { viewId, "list" }, new List<object> { false, "form" } },
            Domain = domain
        };
    }

    public ActionResult ActionOpenStepSalesTax()
    {
        var viewId = Env.Ref("account.res_company_form_view_onboarding_sale_tax").Id;

        return new ActionResult
        {
            Type = "ir.actions.act_window",
            Name = "Sales tax",
            ResId = Env.Company.Id,
            ResModel = "res.company",
            Target = "new",
            ViewMode = "form",
            Views = new List<object> { new List<object> { viewId, "form" } }
        };
    }
}
