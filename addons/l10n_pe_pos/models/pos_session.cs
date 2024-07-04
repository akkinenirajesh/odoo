csharp
public partial class l10n_pe_pos.PosSession {
    public object _load_pos_data_models(object configId) {
        object data = Env.Call("super", "_load_pos_data_models", configId);
        if (Env.Company.Country.Code == "PE") {
            data = (List<object>)data;
            data.Add("l10n_pe.res.city.district");
            data.Add("l10n_latam.identification.type");
            data.Add("res.city");
        }
        return data;
    }

    public object _load_pos_data(object data) {
        data = Env.Call("super", "_load_pos_data", data);
        if (Env.Company.Country.Code == "PE") {
            data["data"][0]["_default_l10n_latam_identification_type_id"] = Env.Ref("l10n_pe.it_DNI").Id;
            data["data"][0]["_consumidor_final_anonimo_id"] = Env.Ref("l10n_pe_pos.partner_pe_cf").Id;
        }
        return data;
    }
}
