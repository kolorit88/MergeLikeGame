using DG.Tweening;
using UnityEngine;

public class BonusInteractiveFrame : MonoBehaviour
{
    public InteractiveBonus currentBonus = null;

    [Header("Movement Settings")]
    public float movementDuration = 2f;
    public float arcHeight = 3f;
    

    private Vector3[] pathPoints;
    void Start()
    {

    }

    void Update()
    {

    }

    public void putInteractiveBonus(InteractiveBonus bonus)
    {
        if (currentBonus != null)
        {
            currentBonus.PutInteractiveBonusInSlot();
        }

        currentBonus = bonus;
        goToFrame(bonus);
    }

    public void removeInteractiveBonus(InteractiveBonus bonus)
    {
        if (bonus == currentBonus)
        {
            currentBonus = null;
        }
        
    }

    private void goToFrame(InteractiveBonus bonus)
    {
        // Создаем точки для кривой Безье
        Vector3 startPos = bonus.transform.position;
        Vector3 endPos = transform.position;
        Vector3 controlPoint = (startPos + endPos) / 2 + Vector3.up * arcHeight;

        pathPoints = new Vector3[] { startPos, controlPoint, endPos };

        // Создаем последовательность
        bonus.mainSeq.Kill();
        Debug.Log(0);
        bonus.mainSeq = DOTween.Sequence();

        // Движение по кривой Безье
        bonus.mainSeq.Append(bonus.transform.DOPath(pathPoints, movementDuration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine));
        bonus.mainSeq.OnComplete(() => Debug.Log(1));
    }
}
