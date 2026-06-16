using UnityEngine;

public class ExpQuantity : Bonus
{
    SpawnOnSquareLine spawnLine;
    override protected void Start()
    {
        defaultSize = transform.localScale;
        spawnLine = FindAnyObjectByType<SpawnOnSquareLine>();
    }

    void Update()
    {

    }

    private void levelUpHand()
    {

        if (spawnLine.getExpQuantityLevel() < 3)
        {
            spawnLine.handExpQuantityLevelUp();
        }

    }

    public override void BonusLevelUp()
    {
        base.BonusLevelUp();
        levelUpHand();
    }
}
