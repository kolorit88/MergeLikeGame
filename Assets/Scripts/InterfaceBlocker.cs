using UnityEngine;

public class InterfaceBlocker : MonoBehaviour
{
    public GameObject background;
    public SpawnOnSquareLine hand;
    public LootBoxMenu LootBoxMenu;

    private bool isInterfaceBlocked;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsInterfaceBlocked()
    {
        return isInterfaceBlocked;
    }

    public void BlockInterfaceWithBackground()
    {
        isInterfaceBlocked = true;
        background.SetActive(true);
        hand.DisableDropItems();
        LootBoxMenu.blockChestButton();
    }

    public void UnblockInterfaceWithBackground()
    {
        isInterfaceBlocked = false;
        background.SetActive(false);
        hand.EnableDropItems();
        LootBoxMenu.unblockChestButton();
    }


}
