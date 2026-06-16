using DG.Tweening;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Pickaxe : InteractiveBonus
{
    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        usingNowListener();
    }
    
    override protected void Action()
    {
        inAction = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = mergeObjectToDestroy.transform.Find("Body").position - new Vector3(0.5f, 0f, 2f);
        Vector3 controlPoint = (startPos + endPos) / 2 + Vector3.up;

        Vector3[] pathPoints = new Vector3[] { startPos, controlPoint, endPos };

        // Создаем последовательность

        mainSeq.Kill();

        mainSeq = DOTween.Sequence();

        // Движение по кривой Безье
        mainSeq.Append(transform.DOPath(pathPoints, 1.0f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine));

        mainSeq.Append(bodyCanvas.transform.DORotate(new Vector3(0f, 0f, -360.0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack));
        mainSeq.OnComplete(() =>
        {
            AudioSource.PlayClipAtPoint(endActionSound, bodyCanvas.transform.position);

            frame.removeInteractiveBonus(this);

            minusOneUsageQuantity();

            if (usageQuantity != 0)
            {
                PutInteractiveBonusInSlot();
            }

            mergeObjectToDestroy.DestroyWithExp();
        });
    }
    

    override public void Activate()
    {
        activate = true;
    }
}
