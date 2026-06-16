using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class LavaChicken : Bonus
{
    [Header("Audio Settings")]
    public AudioSource audioSource;

    [Header("Reaction Settings")]
    public float reactionMultiplier = 30f;
    public float smoothTime = 1f;

    [Header("Volume Settings")]
    public bool useVolumeInfluence = true;
    public float volumeThreshold = 1f;
    public float volumeMultiplier = 10f;

    [Header("Scale Settings")]
    public Vector3 baseScale = Vector3.one;
    public float scaleMultiplier = 10f;

    [Header("Rotation Settings")]
    public float rotationIntensity = 20f;

    [Header("Position Settings")]
    public float positionIntensity = 10f;

    [Header("Pingwing Animation Settings")]
    public float clickDuration = 0.2f;
    public float maxScale = 2f;

    private float[] spectrumData = new float[64];
    private float currentReactionValue;
    private float velocity;
    private float currentVolume;
    private float volumeVelocity;

    private Vector3 originalButtonScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 pingwingOriginalScale;

    private Sequence clickSequence;

    public bool isClickCountTime = false;
    public int maxCountToSpawnChicken = 3;
    public int counter = 0;

    public Button mainButton;
    public MergeObjects chickenPrefab;
    public GameObject objectCenterPoint;

    public SpawnOnSquareLine hand;
    public GameObject background;
    public GameObject pingwing;

    public InterfaceBlocker interfaceBlocker;


    override protected void Start()
    {
        base.Start();
        CreateClickSequence();
        originalButtonScale = mainButton.transform.localScale;
        originalRotation = transform.rotation;

        pingwingOriginalScale = pingwing.transform.localScale;
        pingwing.SetActive(false);

        interfaceBlocker = FindAnyObjectByType<InterfaceBlocker>();
    }

    void Update()
    {
        base.Update();
        if (isClickCountTime) {
            AnalyzeAudio();
            ReactToAudio();
        }
        
    }

    override public void Activate()
    {
        activate = true;
        Action();
    }

    override protected void Action()
    {
        
        Canvas bodyCanvas = GetComponentInChildren<Canvas>();
        mainSeq.Kill();
        mainSeq = DOTween.Sequence();

        mainSeq
            .SetUpdate(true)
            .AppendCallback(() =>
            {
                pingwing.SetActive(true);
            })
            .Append(mainButton.transform.DOMove(new Vector3(0, 0, -7), 2f)) // Этап, когда курица должна вылететь на середину
            .SetEase(Ease.OutBack)
            .AppendCallback(() =>
            {
                pingwing.transform.position = mainButton.transform.position + new Vector3(0.5f, -0.5f, -0.1f); 
                interfaceBlocker.BlockInterfaceWithBackground();
            })
            .AppendCallback(() =>
            {
                audioSource.Play();
                isClickCountTime = true; // С этого момента объект начинает считать клики
            })
            .AppendInterval(60f) // Задержка равная длине музыки
            .AppendCallback(() =>
            {
                isClickCountTime = false;// С этого момента объект не считает клики
                interfaceBlocker.UnblockInterfaceWithBackground();
                pingwing.SetActive(false);
                mainButton.transform.DOScale(originalButtonScale, 1f);
                mainButton.transform.DOMove(objectCenterPoint.transform.position, 1f);
                foreach (MergeObjects mergeObject in FindObjectsOfType<MergeObjects>().ToList())
                {
                    if (mergeObject.getMergeActive())
                    {
                        mergeObject.transform.position = new Vector3(mergeObject.transform.position.x, mergeObject.transform.position.y, 0);
                    }
                }
            })
            .SetEase(Ease.OutBack);
    }

    //public override void goToSlotOrDestroy()
    //{

    //}

    public void buttonBehaviour()
    {
        if (isClickCountTime) // Случай, когда анимация запустилась и кнопка должна считать клики
        {
            counter += 1;
            if (counter >= maxCountToSpawnChicken)
            {
                counter = 0;
                randomSpawnChicken();
            }
        }

        else // Поведение при прокачке, просто притягивает звезду как остальные бонусы
        {
            levelUpStarGoToMe();
        }
    }

    public void randomSpawnChicken()
    {
        Instantiate(chickenPrefab,new Vector3(Random.Range(-6f, 6f), 6f, -6f), Quaternion.identity)
            .transform.Find("Body")
            .GetComponent<Rigidbody2D>()
            .AddForce(Vector2.down * 3f, ForceMode2D.Impulse); ;
    }

    public override void BonusLevelUp()
    {
        Activate();
    }


    void AnalyzeAudio()
    {
        // Получаем спектр аудио
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);

        // Вычисляем среднюю амплитуду
        float sum = 0f;
        for (int i = 0; i < spectrumData.Length; i++)
        {
            sum += spectrumData[i];
        }
        float rawReactionValue = sum / spectrumData.Length * reactionMultiplier;

        // Сглаживаем значение спектра
        currentReactionValue = Mathf.SmoothDamp(currentReactionValue, rawReactionValue, ref velocity, smoothTime);

        // Анализируем громкость
        AnalyzeVolume();
    }

    void AnalyzeVolume()
    {
        if (!useVolumeInfluence) return;

        // Получаем текущую громкость (RMS)
        float[] samples = new float[256];
        audioSource.GetOutputData(samples, 0);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        float rms = Mathf.Sqrt(sum / samples.Length);

        // Сглаживаем значение громкости
        currentVolume = Mathf.SmoothDamp(currentVolume, rms, ref volumeVelocity, smoothTime);
    }

    float GetCombinedReactionValue()
    {
        float reaction = currentReactionValue;

        // Учитываем громкость если включено
        if (useVolumeInfluence && currentVolume > volumeThreshold)
        {
            reaction *= (1f + currentVolume * volumeMultiplier);
        }

        return reaction;
    }

    void ReactToAudio()
    {
        float combinedReaction = GetCombinedReactionValue();

        // Реакция на основе аудио и громкости
        ScaleReaction(combinedReaction);
        RotationReaction(combinedReaction);
        PositionReaction(combinedReaction);
    }

    void ScaleReaction(float reactionValue)
    {
        // Пульсация масштаба с учетом громкости
        float scaleValue = baseScale.x + reactionValue * scaleMultiplier;
        Vector3 targetScale = originalButtonScale * scaleValue;
        mainButton.transform.DOScale(targetScale, smoothTime);

    }

    void RotationReaction(float reactionValue)
    {
        // Покачивание вращением с учетом громкости
        float rotationValue = Mathf.Sin(Time.time * 10f) * reactionValue * rotationIntensity;
        Vector3 targetRotation = new Vector3(0, 0, rotationValue);

        mainButton.transform.DORotate(targetRotation, smoothTime);
    }

    void PositionReaction(float reactionValue)
    {
        // Легкое движение позиции с учетом громкости
        float posValue = Mathf.Sin(Time.time * 8f) * reactionValue * positionIntensity;

        mainButton.transform.DOMove(new Vector3(posValue, 0, mainButton.transform.position.z), smoothTime);
    }


    // Метод для визуализации громкости (для дебага)
    public float GetCurrentVolume()
    {
        return currentVolume;
    }

    public float GetCurrentReactionValue()
    {
        return currentReactionValue;
    }

    // Метод для сброса к оригинальному состоянию
    public void ResetTransform()
    {
        mainButton.transform.DOKill(); // Останавливаем все твины
        mainButton.transform.DOScale(originalButtonScale, 0.5f);
        mainButton.transform.DORotate(originalRotation.eulerAngles, 0.5f);
    }

    // Метод для настройки влияния громкости в runtime
    public void SetVolumeInfluence(bool enabled, float multiplier = 1f)
    {
        useVolumeInfluence = enabled;
        volumeMultiplier = multiplier;
    }

    void OnDestroy()
    {
        // Очищаем твины при уничтожении объекта
        transform.DOKill();
    }

    private void CreateClickSequence()
    {
        clickSequence = DOTween.Sequence();

        // Анимация клика: уменьшение -> увеличение -> возврат
        clickSequence
            .Append(pingwing.transform.DOScale(pingwingOriginalScale, clickDuration))
            .SetLoops(-1, LoopType.Yoyo);
    }

}

