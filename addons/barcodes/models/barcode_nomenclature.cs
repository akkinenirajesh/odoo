csharp
using System;
using System.Text.RegularExpressions;

namespace Barcodes
{
    public partial class BarcodeNomenclature
    {
        public string SanitizeEan(string ean)
        {
            ean = ean.Substring(0, Math.Min(13, ean.Length)).PadLeft(13, '0');
            return ean.Substring(0, 12) + GetBarcodeCheckDigit(ean).ToString();
        }

        public string SanitizeUpc(string upc)
        {
            return SanitizeEan("0" + upc).Substring(1);
        }

        public MatchResult MatchPattern(string barcode, string pattern)
        {
            var match = new MatchResult
            {
                Value = 0,
                BaseCode = barcode,
                Match = false
            };

            barcode = Regex.Escape(barcode);
            var numericalContent = Regex.Match(pattern, @"\{[ND]*\}");

            if (numericalContent.Success)
            {
                int numStart = numericalContent.Index;
                int numEnd = numericalContent.Index + numericalContent.Length;
                string valueString = barcode.Substring(numStart, numEnd - numStart - 2);

                var wholePart = Regex.Match(numericalContent.Value, @"\{[N]*[D\}]");
                var decimalPart = Regex.Match(numericalContent.Value, @"\{N[D]*\}");
                
                string wholePartValue = valueString.Substring(0, wholePart.Length - 2);
                string decimalPartValue = "0." + valueString.Substring(decimalPart.Index, decimalPart.Length - 1);

                if (string.IsNullOrEmpty(wholePartValue))
                    wholePartValue = "0";

                if (int.TryParse(wholePartValue, out int wholeNumber))
                {
                    match.Value = wholeNumber + float.Parse(decimalPartValue);
                    match.BaseCode = barcode.Substring(0, numStart) + new string('0', numEnd - numStart - 2) + barcode.Substring(numEnd - 2);
                    match.BaseCode = Regex.Unescape(match.BaseCode);
                    pattern = pattern.Substring(0, numStart) + new string('0', numEnd - numStart - 2) + pattern.Substring(numEnd);
                }
            }

            match.Match = Regex.IsMatch(match.BaseCode.Substring(0, Math.Min(match.BaseCode.Length, pattern.Length)), "^" + pattern);

            return match;
        }

        public ParseResult ParseBarcode(string barcode)
        {
            var result = new ParseResult
            {
                Encoding = "",
                Type = "error",
                Code = barcode,
                BaseCode = barcode,
                Value = 0
            };

            foreach (var rule in Rules)
            {
                string curBarcode = barcode;
                if (rule.Encoding == "ean13" && CheckBarcodeEncoding(barcode, "upca") && (UpcEanConv == UpcEanConversion.Upc2Ean || UpcEanConv == UpcEanConversion.Always))
                {
                    curBarcode = "0" + curBarcode;
                }
                else if (rule.Encoding == "upca" && CheckBarcodeEncoding(barcode, "ean13") && barcode[0] == '0' && (UpcEanConv == UpcEanConversion.Ean2Upc || UpcEanConv == UpcEanConversion.Always))
                {
                    curBarcode = curBarcode.Substring(1);
                }

                if (!CheckBarcodeEncoding(barcode, rule.Encoding))
                    continue;

                var match = MatchPattern(curBarcode, rule.Pattern);
                if (match.Match)
                {
                    if (rule.Type == "alias")
                    {
                        barcode = rule.Alias;
                        result.Code = barcode;
                    }
                    else
                    {
                        result.Encoding = rule.Encoding;
                        result.Type = rule.Type;
                        result.Value = match.Value;
                        result.Code = curBarcode;
                        if (rule.Encoding == "ean13")
                            result.BaseCode = SanitizeEan(match.BaseCode);
                        else if (rule.Encoding == "upca")
                            result.BaseCode = SanitizeUpc(match.BaseCode);
                        else
                            result.BaseCode = match.BaseCode;
                        return result;
                    }
                }
            }

            return result;
        }

        // Helper methods (to be implemented)
        private int GetBarcodeCheckDigit(string barcode) { /* Implementation */ }
        private bool CheckBarcodeEncoding(string barcode, string encoding) { /* Implementation */ }
    }

    public class MatchResult
    {
        public decimal Value { get; set; }
        public string BaseCode { get; set; }
        public bool Match { get; set; }
    }

    public class ParseResult
    {
        public string Encoding { get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string BaseCode { get; set; }
        public decimal Value { get; set; }
    }
}
