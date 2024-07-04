csharp
public partial class StockMove {
    public virtual void _GenerateSerialMoveLineCommands(object fieldData, object locationDestId, object originMoveLine) {
        var moveLinesCommands = Env.Call("Stock.StockMove", "_GenerateSerialMoveLineCommands", this, fieldData, locationDestId, originMoveLine);
        if (this.Product.UseExpirationDate) {
            var date = Env.Now() + TimeSpan.FromDays(this.Product.ExpirationTime);
            foreach (var moveLineCommand in moveLinesCommands) {
                var moveLineVals = moveLineCommand[2];
                if (!moveLineVals.ContainsKey("ExpirationDate")) {
                    moveLineVals.Add("ExpirationDate", date);
                }
            }
        }
        return moveLinesCommands;
    }

    public virtual object _ConvertStringIntoFieldData(string stringValue, object options) {
        var res = Env.Call("Stock.StockMove", "_ConvertStringIntoFieldData", this, stringValue, options);
        if (res == null) {
            try {
                var datetime = DateTime.Parse(stringValue, (System.Globalization.CultureInfo)options);
                if (this != null && !this.UseExpirationDate) {
                    return "ignore";
                }
                return new { ExpirationDate = datetime };
            } catch (Exception) {
            }
        }
        return res;
    }

    public virtual object _GetFormatingOptions(object strings) {
        var options = Env.Call("Stock.StockMove", "_GetFormatingOptions", this, strings);
        var separators = "-/ ";
        var dateRegex = $"[^{separators}]+";
        foreach (var stringValue in (IEnumerable<string>)strings) {
            var dateData = Regex.Matches(stringValue, dateRegex);
            if (dateData.Count < 2) {
                continue;
            }
            var value1 = dateData[0].Value;
            var value2 = dateData[1].Value;
            if (Regex.IsMatch(value1, "[a-zA-Z]")) {
                break;
            }
            if (int.Parse(value1) > 31) {
                options.Add("yearfirst", true);
                break;
            } else if (int.Parse(value1) > 12 && (Regex.IsMatch(value2, "[a-zA-Z]") || int.Parse(value2) <= 12)) {
                options.Add("dayfirst", true);
                break;
            } else {
                var userLangFormat = Env.Call("Res.Lang", "GetDateFormat", Env.User.Lang);
                if (Regex.IsMatch(userLangFormat, "^%[mbB]")) {
                    return options;
                } else if (Regex.IsMatch(userLangFormat, "^%[djaA]")) {
                    options.Add("dayfirst", true);
                    break;
                } else if (Regex.IsMatch(userLangFormat, "^%[yY]")) {
                    options.Add("yearfirst", true);
                    break;
                }
            }
        }
        return options;
    }

    public virtual object _UpdateReservedQuantity(object need, object locationId, object lotId, object packageId, object ownerId, bool strict) {
        if (this.Product.UseExpirationDate) {
            return Env.Call("Stock.StockMove", "_UpdateReservedQuantity", this.WithContext(new { with_expiration = this.Date }), need, locationId, lotId, packageId, ownerId, strict);
        }
        return Env.Call("Stock.StockMove", "_UpdateReservedQuantity", this, need, locationId, lotId, packageId, ownerId, strict);
    }

    public virtual object _GetAvailableQuantity(object locationId, object lotId, object packageId, object ownerId, bool strict, bool allowNegative) {
        if (this.Product.UseExpirationDate) {
            return Env.Call("Stock.StockMove", "_GetAvailableQuantity", this.WithContext(new { with_expiration = this.Date }), locationId, lotId, packageId, ownerId, strict, allowNegative);
        }
        return Env.Call("Stock.StockMove", "_GetAvailableQuantity", this, locationId, lotId, packageId, ownerId, strict, allowNegative);
    }
}
