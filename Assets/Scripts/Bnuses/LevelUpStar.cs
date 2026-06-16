using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class LevelUpStar : Bonus
{

    public float moveDuration = 5f; // Время движения между точками
    public float wanderRadius = 1f; // Радиус блуждания
    public float heightVariation = 1f; // Разброс по высоте
    public bool waitUnblockInterface = false;

    private bool starIsUsed = false;


    private AudioSource audioSource;
    private InterfaceBlocker interfaceBlocker;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
        interfaceBlocker = FindAnyObjectByType<InterfaceBlocker>();
    }

    void Update()
    {
        if (waitUnblockInterface)
        {
            if (!interfaceBlocker.IsInterfaceBlocked())
            {
                highlightAllBonuses();
                waitUnblockInterface = false;
            }
        }
    }

    public override void Activate() {
        audioSource.Play();
    }


    public void goToBonusAndUpgrade(Bonus bonus)
    {
        starIsUsed = true;
        stopDropAnimation();
        particles.Play();
        Vector3[] path = new Vector3[] {
            transform.position,
            transform.position,
            bonus.transform.position,
        };

        Ease ease = Ease.OutSine;
        float duration = 1.2f;

        transform.DOPath(path, duration, PathType.CatmullRom).SetEase(ease)
        .OnComplete(() =>
        {
            particles.Stop();
            bonus.BonusLevelUp();
            if (FindObjectsOfType<LevelUpStar>().ToList().Count == 1)
            {
                stophighlightAllBonuses();
            }
            Destroy(gameObject);
        });

    }

    public void OnDestroy()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop(); // Выключаем звук при удалении объекта
        }
    }

    public override void BonusLevelUp() { 
        
    }

    public override void goToSlotOrDestroy()
    {
        List<Bonus> allBonuses = FindObjectsOfType<Bonus>().ToList();

        bool noBonusesToLevelUp = true;

        foreach (Bonus bonus in allBonuses)
        {
            if (bonus.getLevel() < 3 && bonus.GetType().Name != "LevelUpStar")
            {
                noBonusesToLevelUp = false;
            }
        }

        Vector3[] path = new Vector3[] {
            transform.position,
            transform.position + Vector3.up * 2f,
            new Vector3(0, 3, -7)
        };

        transform.DOPath(path, 1f, PathType.CatmullRom).SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                if (noBonusesToLevelUp)
                {
                    DestroyWithAnimation(true, true);
                }
                else {
                    particles.Stop();
                    dropAnimation();

                    if (interfaceBlocker.IsInterfaceBlocked())
                    {
                        waitUnblockInterface = true;
                    }
                    else
                    {
                        highlightAllBonuses();
                    }
                }
                Activate();
            });
    }

    public void highlightAllBonuses()
    {
        List<Bonus> bonuses = FindObjectsOfType<Bonus>().ToList();
        for (int i = 0; i < bonuses.Count; i++) { 
            if (bonuses[i].getLevel() < 3)
            {
                bonuses[i].highlightBonus();
            }
            
        }
        interfaceBlocker.BlockInterfaceWithBackground();
    }

    public void stophighlightAllBonuses() {
        List<Bonus> bonuses = FindObjectsOfType<Bonus>().ToList();
        for (int i = 0; i < bonuses.Count; i++)
        {
            bonuses[i].stopHighlightBonus();
        }
        interfaceBlocker.UnblockInterfaceWithBackground();
    }

    public override void stopHighlightBonus()
    {
       
    }

    public override void highlightBonus()
    {
        
    }

    public bool isUsed()
    {
        return starIsUsed;
    }
}
