C#
public partial class Spreadsheet.ResLang
{
    public virtual Spreadsheet.ResLang GetLocalesForSpreadsheet()
    {
        // Return the list of locales available for a spreadsheet.
        var langs = Env.Search<Spreadsheet.ResLang>(new[] { "Active" }, new[] { true }, null);
        var spreadsheetLocales = new List<Spreadsheet.ResLang>();
        foreach (var lang in langs)
        {
            spreadsheetLocales.Add(lang.OdooLangToSpreadsheetLocale());
        }
        return spreadsheetLocales[0];
    }

    public virtual Spreadsheet.ResLang GetUserSpreadsheetLocale()
    {
        // Convert the odoo lang to a spreadsheet locale.
        var lang = Env.GetLang(Env.User.Lang);
        return lang.OdooLangToSpreadsheetLocale();
    }

    public virtual Spreadsheet.ResLang OdooLangToSpreadsheetLocale()
    {
        // Convert an odoo lang to a spreadsheet locale.
        return new Spreadsheet.ResLang
        {
            Name = this.Name,
            Code = this.Code,
            ThousandsSeparator = this.ThousandsSep,
            DecimalSeparator = this.DecimalPoint,
            DateFormat = Spreadsheet.Utils.Formatting.StrftimeFormatToSpreadsheetDateFormat(this.Dateformat),
            TimeFormat = Spreadsheet.Utils.Formatting.StrftimeFormatToSpreadsheetTimeFormat(this.Timeformat),
            FormulaArgSeparator = this.DecimalPoint == "," ? ";" : ",",
        };
    }
}
