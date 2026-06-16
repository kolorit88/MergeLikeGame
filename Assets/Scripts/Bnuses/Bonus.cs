using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading;


public class Bonus : MonoBehaviour
{
    public ParticleSystem particles;
    public AudioClip levelUpSound;
    public AudioClip destroySound;

    public GameObject expOrbPrefab;
    public bool testActivate = false;

    protected Sequence dropAnimationSeq;
    protected Sequence highlightAnimation;
    public Sequence mainSeq;

    protected int level = 0;
    [SerializeField] protected int categoryLevel = 1;
    protected float impactFactor;

    protected Vector3 _originalPosition;
    protected float timer = 0f;
    protected int i = 0;
    protected bool activate = false;
    protected Canvas bodyCanvas;

    protected float defaultZ;
    protected Vector3 defaultSize;

    protected GameObject timerObject;
    public Vector3 defaultTimerSize;



    protected virtual void Start()
    {
        timerObject = new GameObject("timerObject");
        timerObject.transform.SetParent(transform);

        SpriteRenderer spriteRenderer = timerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.gray;

        spriteRenderer.sprite = Sprite.Create(
         Texture2D.whiteTexture,
         new Rect(0, 0, 1, 1),
         new Vector2(0.5f, 0.5f)
           );

        defaultTimerSize = new Vector3(80f, 8f, 1f);
        timerObject.transform.localPosition = new Vector3(0f, -0.7f, -0.3f);
        timerObject.transform.localScale = defaultTimerSize;
        timerObject.SetActive(false);
        
            

        bodyCanvas = gameObject.GetComponentInChildren<Canvas>();
        _originalPosition = transform.position;
        particles.Stop();
        defaultSize = transform.localScale;
        CalculateImpactFactor();
        dropAnimation();
    }

    protected virtual void Update()
    {
        if (testActivate)
        {
            stopDropAnimation();
            goToSlotOrDestroy();
            Activate();
            testActivate = false;
        }
        
    }

    public void dropAnimation()
    {
        _originalPosition = transform.position;
        // 1 Разворот на 360° по Y Ease.InOutBack
        dropAnimationSeq = DOTween.Sequence();

        dropAnimationSeq.Append(transform.DORotate(new Vector3(0, 360, 0), 0.7f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

        // 2️ Одновременный подлёт вверх
        dropAnimationSeq.Join(transform.DOMoveY(_originalPosition.y + 0.5f, 0.7f).SetEase(Ease.OutQuad));


        // 3 Левитация (бесконечная анимация)
        dropAnimationSeq.Append(transform.DOLocalMoveY(transform.position.y + 0.3f, 2f))
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine); // Yoyo = вверх-вниз

    }

    public void stopDropAnimation()
    {
        if (dropAnimationSeq != null)
        {
            dropAnimationSeq.Kill();
            transform.eulerAngles = Vector3.zero;
        }
        
    }

    public virtual void Activate()
    {

    }

    protected virtual void Action()
    {

    }

    public virtual void grabInteractiveBonusOrGoToSlot() { }
    public virtual void DestroyWithAnimation(bool playDestroySound=false, bool leaveExpOrbs=false) {
        Sequence destroyAnimation = DOTween.Sequence();

        destroyAnimation.Append(transform.DORotate(new Vector3(0, 360, 0), 0.7f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad))
            .OnComplete(() => {
                
                if (playDestroySound)
                {
                    AudioSource.PlayClipAtPoint(destroySound, transform.position);
                }

                if (leaveExpOrbs)
                {
                    for (int i = 0; i < 5 * categoryLevel; i++) {
                        Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
                    }
                    
                }

                Destroy(gameObject);
            });
        
    }

    public virtual void removeFromSlotsBar()
    {
        FindAnyObjectByType<BonuseSlotsBar>().removeFromSlot(this);
    }

    public virtual void goToSlotOrDestroy()
    {
        particles.Play();

        Bonus sameBonus = FindAnyObjectByType<BonuseSlotsBar>().bonusIfAlreadyExists(this);
        Vector3 pos = FindAnyObjectByType<BonuseSlotsBar>().putInSlot(this);

        Vector3[] path = new Vector3[] {
            transform.position,
            transform.position + Vector3.up * 2f,
            pos,
        };

        Ease ease = Ease.OutQuad;
        float duration = 2f;

        if (sameBonus != null) { 
            ease = Ease.InQuart;
            duration = 0.5f;
        }

        if (sameBonus == null || sameBonus.level < 3 || sameBonus.level == 4)
        {
            mainSeq = DOTween.Sequence();
            mainSeq
                .Append(transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(ease))
                .OnComplete(() =>
                {
                    particles.Stop();

                    if (sameBonus != null)
                    {
                        sameBonus.BonusLevelUp();
                        DestroyWithAnimation();
                    }

                    else
                    {
                       Activate();
                    }

                });
        }

        else
        {
            mainSeq.Kill();
            mainSeq = DOTween.Sequence();

            mainSeq.Append(transform.DOShakePosition(1f, 0.3f, 15));
            mainSeq.AppendCallback(() => DestroyWithAnimation(true, true));
        }

        
    }

    protected virtual void ForWithInterval(int iterations, float delay, Action func, bool destroyOnComplete = true)
    {
        timerObject.SetActive(true);
        timer += Time.deltaTime; // Увеличиваем таймер

        if (i < iterations || iterations == -1)
        {
            updateItmerObject(delay);
            if (timer >= delay)
            {
                func();
                i++;
                timer = 0f; // Сбрасываем таймер
            }

        }
        else
        {
            if (destroyOnComplete)
            {
                activate = false;
                DestroyWithAnimation();
            }
        }
    }

    protected void updateItmerObject(float delay_)
    {
        timerObject.transform.localScale = new Vector3(defaultTimerSize.x * (timer / delay_), defaultTimerSize.y, defaultTimerSize.z);
    }

    protected virtual void CalculateImpactFactor() {
        impactFactor = level;
    }



    public virtual void BonusLevelUp()
    {
        if (level < 3)
        {
            level += 1;
            transform.Find("BonusLevel").gameObject.transform
                .Find("Canvas").gameObject.transform
                .Find("level" + level.ToString()).gameObject.SetActive(true);
            AudioSource.PlayClipAtPoint(levelUpSound, transform.position);
        }
        
    }

    public void levelUpStarGoToMe()
    {
        List<LevelUpStar> allStars = FindObjectsOfType<LevelUpStar>().ToList();
        foreach (LevelUpStar star in allStars)
        {
            if (star != null && level < 3 && !star.isUsed() && !star.waitUnblockInterface)
            {
                star.goToBonusAndUpgrade(this);
                break;
            }
        }
        
    }

    public virtual void highlightBonus() {
        if (level == 3)
        {
            return;
        }

        defaultZ = transform.position.z;

        transform.position = new Vector3(transform.position.x, transform.position.y, -7);
        
        if (highlightAnimation == null)
        {
            highlightAnimation = DOTween.Sequence();
            highlightAnimation.Append(transform.DOScale(1.2f, 1f).SetEase(Ease.OutSine));
            highlightAnimation.Append(transform.DOScale(1f, 1f).SetEase(Ease.OutSine));
            highlightAnimation.SetLoops(-1);
        }
        
    }

    public virtual void stopHighlightBonus() {

        if (highlightAnimation != null)
        {
            if (highlightAnimation.IsPlaying())
            {
                highlightAnimation.Kill();
                highlightAnimation = null;
            }
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, defaultZ);
        transform.localScale = defaultSize;
    }

    public virtual int getLevel() { 
        return level;
    }

    public virtual int getCategoryLevel()
    {
        return categoryLevel;
    }

    public virtual void setLevel(int _level) {
        level = _level;
    }
}