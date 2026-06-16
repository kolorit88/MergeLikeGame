using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ExpBar : MonoBehaviour
{
    public GameObject maskBorder;
    public TextMeshProUGUI levelText;

    private int currentLevel = 1;
    private float currentXP = 0;
    private float xpToNextLevel = 500;

    float barMaxScaleY;
    float barSizeY;

    public System.Collections.Generic.List<AudioClip> levelUpSounds;

    void Start()
    {
        barMaxScaleY = Mathf.Abs(maskBorder.transform.localPosition.y * 2);
    }

    public void AddXP(float amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel) {
            LevelUp();
        }
        else
        {
            addPercentToBar((100 / xpToNextLevel) * amount);
        }
    }

    private void setPercentToBar(float percent)
    {
        barSizeY = (barMaxScaleY * percent) / 100;
        maskBorder.transform.localScale = new Vector3(maskBorder.transform.localScale.x, barSizeY, maskBorder.transform.localScale.z);
    }

    private void addPercentToBar(float percent) 
    {
        barSizeY = (barMaxScaleY * percent) / 100;
        maskBorder.transform.localScale += new Vector3(0, barSizeY, 0);
    }
    private void LevelUp()
    {
        setPercentToBar(0);
        currentXP = 0;

        currentLevel += 1;
        levelText.text = currentLevel.ToString() + " yp";

        FindAnyObjectByType<LootBoxMenu>().addChest(1);

        AudioSource.PlayClipAtPoint(levelUpSounds[Random.Range(0, levelUpSounds.Count)], Vector2.zero, 1f);
    }

}