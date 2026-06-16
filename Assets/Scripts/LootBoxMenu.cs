using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootBoxMenu : MonoBehaviour
{
    public TextMeshProUGUI QuantityMarkText;
    public GameObject MarkRedCircle;
    public LootBoxAnimation openLootBoxAnimation;

    public Image chestImage;
    public Button mainButton;

    private int chestsQuantity = 0;
    private Sequence chestReadyToOpenSeq;

    

    void Start()
    {
        updateMark();
    }

    // Update is called once per frame
    void Update()
    {
        if (openLootBoxAnimation != null) {
            if (openLootBoxAnimation.isReadyToOpen && Input.GetMouseButtonDown(0)) {
                openLootBoxAnimation.openChestAnimation();
            }
            else if (openLootBoxAnimation.isOpened && Input.GetMouseButtonDown(0))
            {
                if (openLootBoxAnimation.lastDroppedBonus != null) { 
                    openLootBoxAnimation.lastDroppedBonus.stopDropAnimation();
                    openLootBoxAnimation.lastDroppedBonus.goToSlotOrDestroy();
                }
                openLootBoxAnimation.endDropPocidure();
            }
        }
    }

    public void addChest(int quantity)
    {
        chestsQuantity += 1;
        UpdateCounterAnimation();
        updateMark();
        
    }

    public void openChest() {
        //дописать тут иф обязательно
        chestsQuantity -= 1;
        updateMark();
        openLootBoxAnimation.dropChestAnimation();
    }

    public void blockChestButton()
    {
        mainButton.interactable = false;
    }

    public void unblockChestButton()
    {
        mainButton.interactable = true;
    }

    void updateMark() {
        if (chestsQuantity == 0)
        {
            QuantityMarkText.gameObject.SetActive(false);
            MarkRedCircle.gameObject.SetActive(false);
            returnChestToDefaultRotation();
        }
        else {
            if (chestsQuantity == 1) {
                QuantityMarkText.gameObject.SetActive(true);
                MarkRedCircle.gameObject.SetActive(true);
                chestReadyToOpenAnimation();
            }
            QuantityMarkText.text = chestsQuantity.ToString();
        }
    }

    private void chestReadyToOpenAnimation() {
        chestReadyToOpenSeq.Kill();
        chestReadyToOpenSeq = DOTween.Sequence()
           .Append(chestImage.transform.DORotate(new Vector3(0, 0, 20f), 0.2f).SetEase(Ease.InOutSine))
           .Append(chestImage.transform.DORotate(new Vector3(0, 0, -20f), 0.2f).SetEase(Ease.InOutSine))
           .Append(chestImage.transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.InOutSine))
           .AppendInterval(1f)
           .SetLoops(-1); // Бесконечное повторение
    }

    private void returnChestToDefaultRotation() {
        if (chestReadyToOpenSeq != null) {
            chestReadyToOpenSeq.Kill();
            chestReadyToOpenSeq = DOTween.Sequence();
            chestReadyToOpenSeq.Append(chestImage.transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.InOutSine))
                .OnComplete(() => chestReadyToOpenSeq.Kill());
        }
        
    }

    private void UpdateCounterAnimation()
    {
        // Создаем последовательность анимаций
        Sequence sequence = DOTween.Sequence();

        // 1. Увеличиваем масштаб
        sequence.Append(QuantityMarkText.transform.DOScale(Vector3.one * 1.01f, 0.3f));

        // 2. Добавляем небольшую тряску
        sequence.Append(QuantityMarkText.transform.DOShakePosition(0.4f, 0.2f));

        // 3. Возвращаем масштаб к нормальному с небольшим "отскоком"
        sequence.Append(QuantityMarkText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce));

        sequence.Join(QuantityMarkText.DOColor(Color.yellow, 0.2f));
        sequence.Append(QuantityMarkText.DOColor(Color.white, 0.2f));
    }
}

