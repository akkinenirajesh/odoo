C#
public partial class StockPackageType {
    public void OnChangeCarrierType() {
        var carrierId = Env.Search<DeliveryCarrier>(x => x.DeliveryType == this.PackageCarrierType).FirstOrDefault();
        if (carrierId != null) {
            this.ShipperPackageCode = carrierId.GetDefaultCustomPackageCode();
        } else {
            this.ShipperPackageCode = null;
        }
    }

    public void ComputeLengthUomName() {
        if (this.PackageCarrierType != "none") {
            // FIXME This variable does not impact any logic, it is only used for the packaging display on the form view.
            //  However, it generates some confusion for the users since this UoM will be ignored when sending the requests
            //  to the carrier server: the dimensions will be expressed with another UoM and there won't be any conversion.
            //  For instance, with Fedex, the UoM used with the package dimensions will depend on the UoM of
            //  `fedex_weight_unit`. With UPS, we will use the UoM defined on `ups_package_dimension_unit`
            this.LengthUomName = "";
        } else {
            base.ComputeLengthUomName();
        }
    }
}
