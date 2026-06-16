using UnityEngine;

public class ExpPack : Bonus
{

    override protected void Start()
    {
        base.Start();
        
    }

    void Update()
    {
        if (activate) {
            ForWithInterval(10 * categoryLevel, 0.1f, Action, true);
        }
        
    }

    override public void Activate()
    {
        activate = true;
    }

    override protected void Action()
    {
        Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
    }


}
