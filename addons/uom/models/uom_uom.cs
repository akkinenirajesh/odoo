C#
public partial class UoM
{
    public virtual void ComputeFactorInv()
    {
        this.FactorInv = this.Factor != 0 ? (1.0 / this.Factor) : 0.0;
    }

    public virtual void ComputeRatio()
    {
        if (this.UomType == "reference")
        {
            this.Ratio = 1;
        }
        else if (this.UomType == "bigger")
        {
            this.Ratio = this.FactorInv;
        }
        else
        {
            this.Ratio = this.Factor;
        }
    }

    public virtual void SetRatio()
    {
        if (this.Ratio == 0)
        {
            throw new Exception("The value of ratio could not be Zero");
        }
        if (this.UomType == "reference")
        {
            this.Factor = 1;
        }
        else if (this.UomType == "bigger")
        {
            this.Factor = 1 / this.Ratio;
        }
        else
        {
            this.Factor = this.Ratio;
        }
    }

    public virtual void ComputeColor()
    {
        if (this.UomType == "reference")
        {
            this.Color = 7;
        }
        else
        {
            this.Color = 0;
        }
    }

    public virtual void OnChangeUomType()
    {
        if (this.UomType == "reference")
        {
            this.Factor = 1;
        }
    }

    public virtual void OnChangeCriticalFields()
    {
        if (this.FilterProtectedUoms() && this.CreateDate < (Env.DateTime.Now - new TimeSpan(1, 0, 0, 0)))
        {
            // Implement warning logic here
        }
    }

    public virtual void CheckCategoryReferenceUniqueness()
    {
        // Implement logic to check uniqueness based on CategoryId and UomType
    }

    private bool FilterProtectedUoms()
    {
        // Implement logic to filter protected UoMs
        return false;
    }
}
