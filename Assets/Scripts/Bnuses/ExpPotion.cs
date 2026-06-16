using UnityEngine;

public class ExpPotion : Bonus
{

    override protected void Start()
    {
        base.Start();
        expOrbPrefab.GetComponent<ExpOrb>().expVlue = 5;
    }

    override protected void Update()
    {
        base.Update();
        if (activate)
        {
            ForWithInterval(-1, 8f, Action, false);
        }
    }

    override public void Activate()
    {
        activate = true;
    }

    protected override void Action()
    {
        for (int i = 0; i < level + 1; i++) {
            Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
        }
        
    }

    protected override void CalculateImpactFactor()
    {
        //impactFactor = (1 - (level / 8));
    }
}
