using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractiveBonus : Bonus
{
    protected int usageQuantity = 1;
    protected MergeObjects mergeObjectToDestroy;

    public bool usingNow = false;
    public bool inAction = false;

    public AudioClip endActionSound;
    public Button mainButton;
    public BonusInteractiveFrame frame;
    public SpawnOnSquareLine hand;

    public TextMeshProUGUI bonusesUsageQuantityText;

    void Start()
    {
        base.Start();
        mainSeq = DOTween.Sequence();
        hand = FindAnyObjectByType<SpawnOnSquareLine>();
        frame = FindAnyObjectByType<BonusInteractiveFrame>();
        level = 4; // чтобы не подсвечивалс€ дл€ прокачки звездой т.к там стоит проверка на == 3
    }

    protected virtual void Update()
    {
        base.Update();
        //if (!usingNow && !inAction)
        //{
        //    bonusesUsageQuantityText.gameObject.SetActive(true);
        //}
        //else
        //{
        //    bonusesUsageQuantityText.gameObject.SetActive(false);
        //}
    }

    protected virtual void usingNowListener()
    {
        if (usingNow && !inAction)
        {
            highlightObjectsForInteractiveUse();
            mergeObjectToDestroy = DetectObjectClick2D();
            if (mergeObjectToDestroy != null)
            {
                Action();
            }
        }
        
        if (!usingNow)
        {
            stophighlightObjectsForInteractiveUse();
        }
    }

    protected virtual void highlightObjectsForInteractiveUse()
    {
        List<MergeObjects> allMergeObjects = FindObjectsOfType<MergeObjects>().ToList();
        foreach (MergeObjects mergeObject in allMergeObjects)
        {
            if (!mergeObject.isHighlighted)
            {
                mergeObject.highlightForInteractiveUse();
            }
        }
    }

    protected virtual void stophighlightObjectsForInteractiveUse()
    {
        List<MergeObjects> allMergeObjects = FindObjectsOfType<MergeObjects>().ToList();
        foreach (MergeObjects mergeObject in allMergeObjects)
        {
            if (mergeObject.isHighlighted)
            {
                mergeObject.StopHighlightInteractiveUse();
            }
        }
    }

    public override void BonusLevelUp()
    {
        usageQuantity += 1;
        bonusesUsageQuantityText.text = usageQuantity.ToString();
    }

    protected MergeObjects DetectObjectClick2D()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MergeObjects mergeObj = null;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                mergeObj = hit.collider.GetComponentInParent<MergeObjects>();
            }
            if (mergeObj != null && mergeObj.getMergeActive())
            {
                return mergeObj;
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    public void ClickingButtonBehavior()
    {
        if (usingNow)
        {
            PutInteractiveBonusInSlot();
        }

        else
        {
            PutInteractiveBonusInFrame();
        }
    }

    public void PutInteractiveBonusInSlot()
    {
            stopDropAnimation();
            inAction = false;
            usingNow = false;
            frame.removeInteractiveBonus(this);
            OffReadyForUseAnimation();
            goToSlotOrDestroy();
            hand.EnableDropItems();
    }

    public void PutInteractiveBonusInFrame()
    {
        stopDropAnimation();
        inAction = false;
        usingNow = true;
        removeFromSlotsBar();
        frame.putInteractiveBonus(this);
        hand.DisableDropItems();
        PlayReadyForUseAnimation();
        bonusesUsageQuantityText.gameObject.SetActive(false);

    }

    public override void goToSlotOrDestroy()
    {
        base.goToSlotOrDestroy();
        bonusesUsageQuantityText.gameObject.SetActive(true);
    }

    public void PlayReadyForUseAnimation()
    {
        float pulsationScale = 1.1f;
        float pulsationSpeed = 0.3f;
        float wobbleStrength = 0.2f;

        //mainSeq.Kill();

        //mainSeq = DOTween.Sequence();

        // ѕульсаци€
        mainSeq.Append(transform.DOScale(transform.localScale * pulsationScale, pulsationSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo));
        
        // ѕокачивание
        mainSeq.Append(transform.DOShakeRotation(pulsationSpeed * 2, wobbleStrength, 2, 45)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo));
    }

    public void OffReadyForUseAnimation()
    {
        transform.localScale = defaultSize;
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();
    }

    public void minusOneUsageQuantity()
    {
        if (usageQuantity != 0)
        {
            usageQuantity -= 1;
            bonusesUsageQuantityText.text = usageQuantity.ToString();
        }
        
        if (usageQuantity == 0)
        {
            mainSeq.Kill();
            mainSeq = DOTween.Sequence();
            hand.EnableDropItems();
            stophighlightObjectsForInteractiveUse();
            DestroyWithAnimation(true, true);
        }
    }
}
