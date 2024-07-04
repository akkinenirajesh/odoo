csharp
public partial class LunchCashmoveReport
{
    public void Init()
    {
        Env.DropViewIfExists(this._table);

        Env.Execute("""
            CREATE or REPLACE view %s as (
                SELECT
                    lc.id as id,
                    lc.amount as amount,
                    lc.date as date,
                    lc.currency_id as currency_id,
                    lc.user_id as user_id,
                    lc.description as description
                FROM lunch_cashmove lc
                UNION ALL
                SELECT
                    -lol.id as id,
                    -lol.price as amount,
                    lol.date as date,
                    lol.currency_id as currency_id,
                    lol.user_id as user_id,
                    format('Order: %%s x %%s %%s', lol.quantity::text, lp.name->>'en_US', lol.display_toppings) as description
                FROM lunch_order lol
                JOIN lunch_product lp ON lp.id = lol.product_id
                WHERE
                    lol.state in ('ordered', 'confirmed')
                    AND lol.active = True
            );
        """ % this._table);
    }

    public void ComputeDisplayName()
    {
        this.DisplayName = string.Format("{0} #{1}", Env.Translate("Lunch Cashmove"), this.Id);
    }
}
