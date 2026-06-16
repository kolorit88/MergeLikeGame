using UnityEngine;

public class ItemsLevelButton : Bonus
{
    SpawnOnSquareLine spawnLine;
    override protected void Start()
    {
        defaultSize = transform.localScale;
        spawnLine = FindAnyObjectByType<SpawnOnSquareLine>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void levelUpHand()
    {

        if (spawnLine.getItemsLevel() < 3)
        {
            spawnLine.handItemsLevelUp();
        }

    }

    public override void BonusLevelUp()
    {
        base.BonusLevelUp();
        levelUpHand();
    }
}
