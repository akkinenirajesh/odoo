csharp
public partial class Stargate {
    public void ComputeHasGalaxyCrystal() {
        var milkyWay = Env.Ref("TestHttp.MilkyWay");
        this.HasGalaxyCrystal = this.GalaxyId == milkyWay;
    }
    public void ComputeName() {
        if (string.IsNullOrEmpty(this.Name)) {
            this.Name = this.SgcDesignation;
        }
    }
    public void ComputeSgcDesignation() {
        if (this.GalaxyId.Name != "Milky Way" && this.GalaxyId.Name != "Pegasus") {
            this.SgcDesignation = "";
            return;
        }
        var regionPart = this.GalaxyId.Name == "Pegasus" ? PEGASUS_REGIONS[this.Id % PEGASUS_REGIONS.Length] : MILKY_WAY_REGIONS[this.Id % MILKY_WAY_REGIONS.Length];
        var localPart = Convert.ToInt32(this.Address.ToByteArray(), 0).ToString().Substring(0, 3);
        this.SgcDesignation = $"{regionPart}-{localPart}";
    }
}
public partial class Galaxy {
    public string Render(int galaxyId) {
        return Env.QWeb.Render("TestHttp.TmplGalaxy", new { galaxy = Env.Browse<Galaxy>(galaxyId) });
    }
}
