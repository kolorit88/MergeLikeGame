
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class MergeObjects : MonoBehaviour
{
    [Header("Merge Settings")]
    public GameObject mergedObjectPrefab; // Префаб нового объекта
    public float mergeJumpForce = 3f; // Сила подбрасывания при слиянии
    public float spinTorqueMax = 0.5f;
    public int level = 0;

    [Header("Effects")]
    public ParticleSystem mergeParticles; // Эффект слияния (партиклы)
    public TrailRenderer trailTale;
    public Color effectsColor;              // Хвост

    [Header("ExpOrb")]
    public GameObject expOrbPrefab;
    public int expOrbQuantity = 1;

    private bool mergeActive = true;
    private GamePrograssBar progressBar;

    [Header("Настройки подсветки")]
    [SerializeField] private Color highlightColor = Color.white;
    [SerializeField] private float blinkDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Tween blinkTween;
    public bool isHighlighted = false;

    public void Start()
    {
        TrailRenderer trail = Instantiate(trailTale, new Vector2(0, 0), Quaternion.identity);
        trail.transform.localScale = new Vector2(1, 1);
        trail.transform.position = new Vector3(0, 0, 0.1f);
        trail.transform.SetParent(gameObject.transform.Find("Body"), false);
        trail.startColor = effectsColor;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        progressBar = FindAnyObjectByType<GamePrograssBar>();
    }


    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (mergeActive)
    //    {
    //        MergeObjects otherMerge = collision.gameObject.GetComponent<MergeObjects>();

    //        if (otherMerge != null && this.enabled && otherMerge.enabled && gameObject.name == otherMerge.name)
    //        {
    //            // Вычисляем позицию для нового объекта
    //            Vector2 mergePosition = (transform.position + collision.transform.position) / 2f;

    //            // Создаем новый объект и подбрасываем его
    //            CreateMergedObject(mergePosition);

    //            // Уничтожаем старые объекты
    //            Destroy(gameObject);
    //            Destroy(collision.gameObject);

    //            // Блокируем повторное слияние
    //            this.enabled = false;
    //            otherMerge.enabled = false;
    //        }
    //    }
    //}

    public void CreateMergedObject(Vector2 position)
    {
        if (mergedObjectPrefab != null)
        {
            FindFirstObjectByType<ComboManager>().PlayAnotherSound();

            MergeObjects newObj = Instantiate(mergedObjectPrefab, position, Quaternion.identity).gameObject.GetComponent<MergeObjects>();
            progressBar.checkObjectIfNextProgress(newObj.level);


            for (int i = 0; i < expOrbQuantity + FindFirstObjectByType<ComboManager>().getComboNumber(); i++) {
                Instantiate(expOrbPrefab, position, Quaternion.identity);
            }
            

            if (mergeParticles != null)
            {
                ParticleSystem particles = Instantiate(mergeParticles, new Vector2(0, 0), Quaternion.identity);

                particles.transform.Find("Shockwave").GetComponent<Renderer>().material.color = newObj.effectsColor;
                particles.transform.localScale = new Vector3(3, 3, 3);
                particles.transform.SetParent(newObj.transform.Find("Body"), false);

                particles.Play();
                Destroy(particles.gameObject, particles.main.duration);
            }

            // Добавляем импульс вверх и закрутку
            Rigidbody2D rb = newObj.transform.Find("Body").GetComponent<Rigidbody2D>();
            float direction = Random.Range(0, 2) == 0 ? -1f : 1f;
            float spinTorquePower = Random.Range(0, spinTorqueMax);

            if (rb != null)
            {
                rb.AddForce(Vector2.up * mergeJumpForce, ForceMode2D.Impulse);
                rb.AddTorque(spinTorquePower * direction, ForceMode2D.Impulse);
            }

        }

    }

    public void highlightForInteractiveUse()
    {
        if (isHighlighted || !mergeActive) return;

        isHighlighted = true;

        // Останавливаем предыдущую анимацию, если она была
        blinkTween?.Kill();

        // Создаем анимацию мигания
        blinkTween = spriteRenderer.DOColor(highlightColor, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo) // Бесконечное мигание
            .SetEase(Ease.InOutSine);

    }

    public void StopHighlightInteractiveUse()
    {
        if (spriteRenderer == null || !isHighlighted) return;

        isHighlighted = false;

        // Останавливаем анимацию
        blinkTween?.Kill();

        // Плавно возвращаем исходный цвет
        spriteRenderer.DOColor(originalColor, blinkDuration * 0.5f);
    }


    public void DestroyWithExp()
    {
        for (int i = 0; i < expOrbQuantity * FindFirstObjectByType<ComboManager>().getComboNumber() + 1 + level; i++)
        {
            Instantiate(expOrbPrefab, transform.Find("Body").transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        blinkTween?.Kill();
    }

    public void setMergeActive(bool mergeActive)
    {
        gameObject.transform.Find("Body").GetComponent<Rigidbody2D>().simulated = mergeActive;
        this.mergeActive = mergeActive;
    }

    public bool getMergeActive()
    {
        return mergeActive;
    }
}