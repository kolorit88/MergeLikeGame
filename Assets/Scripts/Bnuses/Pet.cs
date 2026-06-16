using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using System;
public class Pet : Bonus
{
    public AudioClip eatSound;
    public AudioClip barckSound;
    public GameObject squareMergeObjectsCollider;
    public float defaultCooldown = 5f;
    public float cooldown;
    private Vector3 mainBucketBottomPos;
    private List<MergeObjects> allMergeObjectsInCollider;

    private float additionalAnimationTimer = 0f;
    private float nextTriggerTime = 0f;

    private bool dontInterruptAnimationNow = true;

    override protected void Start()
    {
        base.Start();
        cooldown = defaultCooldown;
        expOrbPrefab.GetComponent<ExpOrb>().expVlue = 5;
        mainBucketBottomPos = GameObject.FindWithTag("Bucket").transform.Find("Bottom").transform.position;
        allMergeObjectsInCollider = new List<MergeObjects>();
        StartIdleAnimation();
    }

    override protected void Update()
    {
        base.Update();

        additionalAnimationTimer += Time.deltaTime;

        if (additionalAnimationTimer >= nextTriggerTime)
        {
            Action additionalAnimationAction = new List<Action> {PlayBarkAnimation, PlayLookAroundAnimation}[UnityEngine.Random.Range(0, 2)];

            if (timer < cooldown && !dontInterruptAnimationNow) // проверка, что анимация включена до выполнения способности этого бонуса
            {
                additionalAnimationAction();
                additionalAnimationTimer = 0f;
                nextTriggerTime = UnityEngine.Random.Range(15f, 25f); // случайный интервал 15-25 секунд
            }
            
        }

        if (activate)
        {
            ForWithInterval(-1, cooldown, Action, false);
        }

        squareMergeObjectsCollider.transform.position = mainBucketBottomPos - new Vector3(0, 0.5f, 0);
        squareMergeObjectsCollider.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    override public void Activate()
    {
        activate = true;
    }

    protected override void Action()
    {
        dontInterruptAnimationNow = true;
        Collider2D squareCollider = squareMergeObjectsCollider.gameObject.GetComponent<Collider2D>();

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
        squareCollider.bounds.center,
        squareCollider.bounds.size,
        transform.rotation.eulerAngles.z
        );        

        foreach (var hitCollider in hitColliders)
        {
            MergeObjects mergeObject = hitCollider.GetComponentInParent<MergeObjects>();

            if (mergeObject != null && hitCollider != squareCollider && mergeObject.getMergeActive() && mergeObject.level < 4)
            {
                allMergeObjectsInCollider.Add(mergeObject);
            }
        }

        if (allMergeObjectsInCollider.Count != 0) {
            MergeObjects randomMergeObject = allMergeObjectsInCollider[UnityEngine.Random.Range(0, allMergeObjectsInCollider.Count)];
            EatObject(randomMergeObject);
        }

        allMergeObjectsInCollider.Clear();
    }

    protected override void CalculateImpactFactor()
    {
        impactFactor = (1 - (level / 8));
        cooldown = defaultCooldown * impactFactor;
    }

    public override void BonusLevelUp()
    {
        base.BonusLevelUp();
        CalculateImpactFactor();
    }

    protected void EatObject(MergeObjects randomMergeObject)
    {
        dontInterruptAnimationNow = true;
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();

        Transform mergeObjectBody = randomMergeObject.transform.Find("Body");

        Vector3[] path = new Vector3[] {
                    randomMergeObject.transform.position,
                    randomMergeObject.transform.position + Vector3.up,
                    bodyCanvas.transform.position
                };

        randomMergeObject.setMergeActive(false);
        mergeObjectBody.GetComponent<Rigidbody2D>().simulated = false;
        // Фаза подготовки - сжатие
        mainSeq
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 1.1f,
                defaultSize.y * 0.9f,
                defaultSize.z * 1.1f), 0.5f)
                .SetEase(Ease.OutBack))
            .Join(mergeObjectBody.DOPath(path, 1f, PathType.CatmullRom).SetEase(Ease.OutQuad))
            // Фаза "поглощения" - резкое сжатие
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 0.7f,
                defaultSize.y * 0.3f,
                defaultSize.z * 0.7f), 0.2f)
                .SetEase(Ease.InCirc))
            .JoinCallback(() => AudioSource.PlayClipAtPoint(eatSound, transform.position))
            .JoinCallback(() => randomMergeObject.DestroyWithExp())

            // Фаза "переваривания" - резиновое восстановление
            .Append(transform.DOScale(defaultSize * 1.35f, 0.4f)
                .SetEase(Ease.OutElastic))

            // Возврат к нормальному размеру
            .Append(transform.DOScale(defaultSize, 0.3f)
                .SetEase(Ease.InOutSine))
            .AppendCallback(() => PlayBarkAnimation())
            .OnComplete(() => { dontInterruptAnimationNow = false; });
    }

    public void StartIdleAnimation()
    {
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();
        mainSeq
        .Append(transform.DOScale(new Vector3(
            defaultSize.x * 1.08f,
            defaultSize.y * 0.92f,
            defaultSize.z * 1.02f), 1.2f).SetEase(Ease.InOutSine))
        .Join(transform.DORotate(new Vector3(0, 0, -3f), 1.2f).SetEase(Ease.InOutSine))

        // Фаза 2: Разжатие по Y и сжатие по X + наклон в другую сторону
        .Append(transform.DOScale(new Vector3(
            defaultSize.x * 0.95f,
            defaultSize.y * 1.05f,
            defaultSize.z * 0.98f), 1.5f).SetEase(Ease.InOutSine))
        .Join(transform.DORotate(new Vector3(0, 0, 2f), 1.5f).SetEase(Ease.InOutSine))

        // Фаза 3: Возврат к нейтральному положению с небольшим перелетом
        .Append(transform.DOScale(new Vector3(
            defaultSize.x * 1.03f,
            defaultSize.y * 0.97f,
            defaultSize.z * 1.01f), 1f).SetEase(Ease.InOutSine))
        .Join(transform.DORotate(new Vector3(0, 0, -1f), 1f).SetEase(Ease.InOutSine))

        // Фаза 4: Завершающее сжатие-разжатие
        .Append(transform.DOScale(new Vector3(
            defaultSize.x * 0.98f,
            defaultSize.y * 1.03f,
            defaultSize.z * 0.99f), 0.8f).SetEase(Ease.InOutSine))
        .Join(transform.DORotate(new Vector3(0, 0, 1f), 0.8f).SetEase(Ease.InOutSine))

        .SetLoops(-1, LoopType.Yoyo);
    }

    public void PlayEatAnimation()
    {
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();

        // Фаза подготовки - сжатие
        mainSeq
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 1.2f,
                defaultSize.y * 0.8f,
                defaultSize.z * 1.2f), 0.5f)
                .SetEase(Ease.OutBack))

            // Фаза "поглощения" - резкое сжатие
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 0.7f,
                defaultSize.y * 0.3f,
                defaultSize.z * 0.7f), 0.2f)
                .SetEase(Ease.InCirc))

            // Фаза "переваривания" - резиновое восстановление
            .Append(transform.DOScale(defaultSize * 1.1f, 0.4f)
                .SetEase(Ease.OutElastic))

            // Возврат к нормальному размеру
            .Append(transform.DOScale(defaultSize, 0.3f)
                .SetEase(Ease.InOutSine))

            .OnComplete(() => StartIdleAnimation());
    }

    public void PlayBarkAnimation()
    {
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();

        // Быстрое сжатие для "зарядки" лая
        mainSeq
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 1.1f,
                defaultSize.y * 0.9f,
                defaultSize.z * 1.1f), 0.3f)
                .SetEase(Ease.OutQuad))

            // Резкое расширение - сам "гав"
            .Append(transform.DOScale(new Vector3(
                defaultSize.x * 0.9f,
                defaultSize.y * 1.3f,
                defaultSize.z * 0.9f), 0.2f)
                .SetEase(Ease.OutBack))
            .JoinCallback(() => AudioSource.PlayClipAtPoint(barckSound, transform.position))
            // Резиновое восстановление
            .Append(transform.DOScale(defaultSize, 0.3f)
                .SetEase(Ease.OutElastic))

            .OnComplete(() => StartIdleAnimation());
    }

    public void PlayLookAroundAnimation()
    {
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();

        // Посмотреть налево с пружинящим эффектом
        mainSeq

        // Поворот направо с пружинным эффектом
        .Append(transform.DORotate(new Vector3(0, 180, 0), 1f).SetEase(Ease.OutBack))

        // Возврат с перелетом (пружинный эффект)
        .Append(transform.DORotate(new Vector3(0, -180, 0), 1f).SetEase(Ease.InBack))

        //// Финальное выравнивание
        //.Append(transform.DORotate(new Vector3(0, 0, 0f), 0.5f).SetEase(Ease.InOutSine))

        // Восстановление масштаба
        .Append(transform.DOScale(defaultSize, 0.5f).SetEase(Ease.InOutSine))

        .OnComplete(() => StartIdleAnimation());
    }
}
