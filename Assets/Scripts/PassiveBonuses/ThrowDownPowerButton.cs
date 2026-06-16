using UnityEngine;

public class ThrowDownPowerButton : Bonus
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

    private void levelUpHand() {

        if (spawnLine.getSpeedLevel() < 3)
        {
            spawnLine.handSpeedLevelUp();
        }
        
    }

    public override void BonusLevelUp()
    {
        base.BonusLevelUp();
        levelUpHand();
    }
}
