using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
public class ExpOrb : MonoBehaviour
{
    private Vector3 originalScale;

    [Header("Анимация")]
    [SerializeField] private float pulseSpeed = 1f;       // Скорость пульсации
    private float minScale;
    private float maxScale;

    [Header("Магнитизм к бару")]
    public float minMagnitMDelay = 0.5f;
    public float maxMagnitMDelay = 1f;
    public float delayCounter;
    public float speed = 10f;

    [Header("Отскок при спавне")]
    public float reboundRadius = 1f;
    public float reboundSpeed = 1f;
    private Vector3 randomClosePosition;

    [Header("Звук")]
    public List<AudioClip> expSounList;
    AudioClip randomExpSound;

    public int expVlue = 5;
   

    void Start()
    {
        minScale = transform.localScale.x * 0.9f;
        maxScale = transform.localScale.x * 1.1f;

        originalScale = transform.localScale;

        maxMagnitMDelay = Random.Range(minMagnitMDelay, maxMagnitMDelay);
        delayCounter = maxMagnitMDelay;

        randomClosePosition = gameObject.transform.position +
            new Vector3(Random.Range(-reboundRadius, reboundRadius), Random.Range(-reboundRadius, reboundRadius), 0);

        randomExpSound = expSounList[Random.Range(0, expSounList.Count)];
        randomExpSound.LoadAudioData();

    }

    // Update is called once per frame
    void Update()
    {
        delayTimerCount();
        updateAnimation();

        if (delayCounter == 0) {
            magnitToExpBar();
        }
        
    }

    private void magnitToExpBar()
    {
        ExpBar expBarTarget = FindFirstObjectByType<ExpBar>();

        if (expBarTarget != null) {

            // Движение с постоянной скоростью (без замедления)
            transform.position = Vector2.MoveTowards(
                transform.position,
                expBarTarget.transform.position,
                speed * Time.deltaTime
            );

            // Уничтожаем объект при достижении цели
            if (Vector2.Distance(transform.position, expBarTarget.transform.position) < 0.01f)
            {
                expBarTarget.AddXP(expVlue);
                AudioSource.PlayClipAtPoint(randomExpSound, Vector2.zero, 2f);
                Destroy(gameObject);
            }
        }
    }

    private void updateAnimation() {
        float scaleFactor = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(Time.time * pulseSpeed, 1f));
        transform.localScale = originalScale * scaleFactor;
    }

    private void delayTimerCount()
    {
        if (delayCounter > 0)
        {
            delayCounter -= Time.deltaTime;
            if (maxMagnitMDelay / 2 < delayCounter) {
                transform.position = Vector2.Lerp(
                    transform.position,
                    randomClosePosition,
                    speed * Time.deltaTime
                );
            }

            if (delayCounter <= 0)
            {
                delayCounter = 0;
            }
        }
    }
}
