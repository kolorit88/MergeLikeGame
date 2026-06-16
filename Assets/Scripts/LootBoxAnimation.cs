using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class LootBoxAnimation : MonoBehaviour
{
    public Image chestImage;
    public Image openedChest;
    public GameObject backGround;
    public System.Collections.Generic.List<AudioClip> openSounds;
    public System.Collections.Generic.List<Bonus> bonusesToDrop;
    public AudioClip crackingSound;
    public AudioClip fallSound;
    public GameObject bonusSpawnPoint;
    public LootBoxMenu lootBoxMenuButton;

    public ParticleSystem chestOpenLoopedParticles;
    public ParticleSystem bonusCategoryEffectShine;

    public InterfaceBlocker interfaceBlocker;

    public int firstCategoryChancePercent = 40;
    public int secondCategoryChancePercent = 30;
    public int thirdCategoryChancePercent = 20;
    public int fourthCategoryChancePercent = 10;

    // 1. Серый градиент
    public Gradient grayGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
        new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 0f),
        new GradientColorKey(new Color(1f, 1f, 1f), 1f),
        },
        alphaKeys = new GradientAlphaKey[]
        {
        new GradientAlphaKey(1f, 0f)
        },
        mode = GradientMode.Blend
    };

    // 2. Синий градиент  
    public Gradient blueGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
        new GradientColorKey(new Color(0.0f, 0.4f, 0.92f), 0f),
        new GradientColorKey(new Color(0.0f, 0.96f, 1f), 0.5f),
        new GradientColorKey(new Color(0.0f, 0.4f, 0.92f), 1f)
        },
        alphaKeys = new GradientAlphaKey[]
        {
        new GradientAlphaKey(1f, 0f),
        new GradientAlphaKey(1f, 0.5f),
        new GradientAlphaKey(1f, 1f)
        },
        mode = GradientMode.Blend
    };

    // 3. Фиолетовый градиент (исправлена ошибка в позиции)
    public Gradient purpleGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
        new GradientColorKey(new Color(0.65f, 0.11f, 0.63f), 0f),
        new GradientColorKey(new Color(0.89f, 0.41f, 0.98f), 0.5f),
        new GradientColorKey(new Color(0.65f, 0.11f, 0.63f), 1f) // исправлено с 0f на 1f
        },
        alphaKeys = new GradientAlphaKey[]
        {
        new GradientAlphaKey(1f, 0f),
        new GradientAlphaKey(1f, 0.5f),
        new GradientAlphaKey(1f, 1f)
        },
        mode = GradientMode.Blend
    };

    // 4. Желтый градиент
    public Gradient yellowGradient = new Gradient()
    {
        colorKeys = new GradientColorKey[]
        {
        new GradientColorKey(new Color(0.96f, 0.84f, 0.1f), 0f),
        new GradientColorKey(new Color(1f, 0.92f, 0.46f), 1f)
        },
        alphaKeys = new GradientAlphaKey[]
        {
        new GradientAlphaKey(1f, 0f),
        new GradientAlphaKey(1f, 1f)
        },
        mode = GradientMode.Blend
    };

    private Dictionary<int, Gradient> categoryColors = new Dictionary<int, Gradient>();
        

    public bool isReadyToOpen = false;
    public bool isOpened = false;

    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private float openEffectDuration = 1f;
    public Bonus lastDroppedBonus = null;

    private Sequence openChestPulseSeq;


    void Start()
    {
        hide();

        categoryColors.Add(1, grayGradient);
        categoryColors.Add(2, blueGradient);
        categoryColors.Add(3, purpleGradient);
        categoryColors.Add(4, yellowGradient);
        _originalPosition = chestImage.transform.position;
        _originalScale = chestImage.transform.localScale;

        returnChestOnTop();
        
    }

    void Update()
    {
        
    }

    public void dropChestAnimation()
    {
        lootBoxMenuButton.blockChestButton();
        returnChestOnTop();

        interfaceBlocker.BlockInterfaceWithBackground();
        chestImage.gameObject.SetActive(true);

        Sequence dropSequence = DOTween.Sequence();

        // 1. Падение сундука
        dropSequence.Append(chestImage.transform.DOMoveY(_originalPosition.y, 0.1f)
            .SetEase(Ease.Linear)
            .OnStart(() => {
                AudioSource.PlayClipAtPoint(fallSound, Vector2.zero, 2f);
            }));

        // 2. Небольшая тряска при приземлении
        dropSequence.Append(chestImage.transform.DOShakePosition(0.3f, 0.3f, 10)).OnComplete(() => { isReadyToOpen = true;});
    }

    public void openChestAnimation()
    {
        isReadyToOpen = false;
        Sequence openSequence = DOTween.Sequence();

        // 1. Эффект "вспышки" перед сменой изображения
        openSequence.AppendCallback(() => AudioSource.PlayClipAtPoint(crackingSound, Vector2.zero, 2f));
        openSequence.Join(chestImage.transform.DOShakePosition(openEffectDuration, 1f, 10));
        openSequence.Append(chestImage.transform.DOScale(_originalScale * 1.2f, openEffectDuration / 4));
        openSequence.Append(chestImage.DOFade(0.1f, openEffectDuration / 5));

        // 2. Смена изображения в середине анимации
        openSequence.AppendCallback(() => chestImage.gameObject.SetActive(false));
        openSequence.AppendCallback(() => openedChest.gameObject.SetActive(true));
        openSequence.AppendCallback(() => chestOpenLoopedParticles.Play());
        openSequence.AppendCallback(() => dropItem());
        // 3. Завершающие эффекты
        openSequence.Append(chestImage.DOFade(1f, 0.1f));
        openSequence.Append(chestImage.transform.DOScale(_originalScale, 0.1f))
            .AppendCallback(() => {
                Gradient categoryGradient = categoryColors[lastDroppedBonus.getCategoryLevel()];
                //var smokeParticle = bonusCategoryEffect.transform.Find("Smoke").GetComponent<ParticleSystem>();
                //var mainModule = smokeParticle.main;
                //mainModule.startColor = new ParticleSystem.MinMaxGradient(categoryGradient);

                var ellowParticle = bonusCategoryEffectShine.transform.GetComponent<ParticleSystem>();
                var ellowMainModule = ellowParticle.main;
                ellowMainModule.startColor = new ParticleSystem.MinMaxGradient(categoryGradient);

                var shadowParticle = bonusCategoryEffectShine.transform.Find("shadow").GetComponent<ParticleSystem>();
                var colorModule = shadowParticle.colorOverLifetime;
                colorModule.color = new ParticleSystem.MinMaxGradient(categoryGradient);

                var circlesParticle = bonusCategoryEffectShine.transform.Find("circles").GetComponent<ParticleSystem>();
                var colorModuleCircles = circlesParticle.colorOverLifetime;
                colorModuleCircles.color = new ParticleSystem.MinMaxGradient(categoryGradient);
            })
            .OnComplete(() => { 
                isOpened = true;

                int ifSecondLegendarySoundIndex = 0; //Звук дропа, зависящий от категории
                if (lastDroppedBonus.getCategoryLevel() == 4)
                {
                    ifSecondLegendarySoundIndex = Random.Range(0, 2);
                  
                }
                AudioClip dropSound = openSounds[lastDroppedBonus.getCategoryLevel() - 1 + ifSecondLegendarySoundIndex];
                AudioSource.PlayClipAtPoint(dropSound, Vector2.zero, 2f);

                bonusCategoryEffectShine.Stop();//Эффект сияния, зависящий от категории
                bonusCategoryEffectShine.Clear();
                bonusCategoryEffectShine.Play();

                openSequence.Kill();
            });

        if (openChestPulseSeq != null) {
            if (openChestPulseSeq.IsPlaying())
            {
                openChestPulseSeq.Kill();
            }
        }
        openChestPulseSeq = DOTween.Sequence();
        openChestPulseSeq.Append(openedChest.transform.DOScale(1.02f, 1f).SetEase(Ease.OutSine));
        openChestPulseSeq.Append(openedChest.transform.DOScale(1f, 1f).SetEase(Ease.OutSine));
        openChestPulseSeq.SetLoops(-1);
    }

    public void endDropPocidure() {
        Gradient categoryGradient = categoryColors[1];
        //var smokeParticle = bonusCategoryEffect.transform.Find("Smoke").GetComponent<ParticleSystem>();
        //var mainModule = smokeParticle.main;
        //mainModule.startColor = new ParticleSystem.MinMaxGradient(categoryGradient);

        var ellowParticle = bonusCategoryEffectShine.transform.GetComponent<ParticleSystem>();
        var ellowMainModule = ellowParticle.main;
        ellowMainModule.startColor = new ParticleSystem.MinMaxGradient(categoryGradient);

        var shadowParticle = bonusCategoryEffectShine.transform.Find("shadow").GetComponent<ParticleSystem>();
        var colorModule = shadowParticle.colorOverLifetime;
        colorModule.color = new ParticleSystem.MinMaxGradient(categoryGradient);

        var circlesParticle = bonusCategoryEffectShine.transform.Find("circles").GetComponent<ParticleSystem>();
        var colorModuleCircles = circlesParticle.colorOverLifetime;
        colorModuleCircles.color = new ParticleSystem.MinMaxGradient(categoryGradient);

        lootBoxMenuButton.unblockChestButton();
        openChestPulseSeq.Kill();
        openChestPulseSeq = null;
        
        lastDroppedBonus = null;

        isOpened = false;
        isReadyToOpen = false;
        hide();
    }

    private void returnChestOnTop() {
        chestImage.transform.position = new Vector3(
            _originalPosition.x,
            _originalPosition.y + 10f,
            _originalPosition.z
        );
    }

    private void dropItem()
    {
        // Старый принцып дропа, без категорий
        //float totalChance = 0f;
        //foreach (var item in bonusesToDrop)
        //{
        //    totalChance += item.chance;
        //}

        //// Генерируем случайное число от 0 до общей суммы шансов
        //float randomValue = UnityEngine.Random.Range(0f, totalChance);
        //float currentSum = 0f;

        //// Проходим по всем элементам и проверяем, попадает ли случайное число в их диапазон
        //foreach (var item in bonusesToDrop)
        //{
        //    currentSum += item.chance;
        //    if (randomValue <= currentSum)
        //    {
        //        lastDroppedBonus = Instantiate(item, bonusSpawnPoint.transform.position, Quaternion.identity);
        //        return;
        //    }
        //}
        ////Если ошибка округления
        //lastDroppedBonus = Instantiate(bonusesToDrop[Random.Range(0, bonusesToDrop.Count)],
        //    bonusSpawnPoint.transform.position, Quaternion.identity);

        int randomPercent = UnityEngine.Random.Range(0, 100);
        int randomCategory;

        if (randomPercent <= firstCategoryChancePercent)
        {
            randomCategory = 1;
        }
        else if (randomPercent <= secondCategoryChancePercent + firstCategoryChancePercent)
        {
            randomCategory = 2;
        }
        else if (randomPercent <= thirdCategoryChancePercent + secondCategoryChancePercent + firstCategoryChancePercent)
        {
            randomCategory = 3;
        }
        else
        {
            randomCategory = 4;
        }
        Debug.Log(randomCategory);
        Debug.Log(randomPercent);

        List<Bonus> allItemsOfRandomCategory = bonusesToDrop.Where(item => item.getCategoryLevel() == randomCategory).ToList();
        if (allItemsOfRandomCategory.Count == 0) return;
        
        var Bonus = allItemsOfRandomCategory[Random.Range(0, allItemsOfRandomCategory.Count)];
        lastDroppedBonus = Instantiate(Bonus, bonusSpawnPoint.transform.position, Quaternion.identity);
        return;

    }

    

    private void hide() {
        chestImage.gameObject.SetActive(false);
        openedChest.gameObject.SetActive(false);
        interfaceBlocker.UnblockInterfaceWithBackground();
    }
}
