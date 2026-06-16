using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class GamePrograssBar : MonoBehaviour
{
    public GameObject progressLine;
    public List<Canvas> questionmarks;
    public LevelUpStar rewardPrefab;

    private float lineShift = 1.8f;
    private Vector3 progressLineStartPos;
    private List<MergeObjects> objectsToSpawn;
    private int maxObjectLevel = 0;
    private int progressLevel = 0; // <= 3
    private LevelUpStar rewardForThreeCollected = null;

    public List<GameObject> cellsList;
    private List<MergeObjects> objectsInCellsList;
    public AudioClip threeСollectedSound;
    public AudioClip newItemUnlockedSound;


    void Start()
    {
        objectsInCellsList = new List<MergeObjects> { null, null, null };
        progressLineStartPos = progressLine.transform.position;
        objectsToSpawn = FindAnyObjectByType<SpawnOnSquareLine>().objectsToSpawn;
        putObjectsInCell();
        checkObjectIfNextProgress(1);
    }

    void Update()
    {
        
    }

    public void checkObjectIfNextProgress(int MergedObjectLevel)
    {
        if (MergedObjectLevel > maxObjectLevel)
        {
            maxObjectLevel = MergedObjectLevel;
            goToNextProgress();
        }
    }

    private void goToNextProgress()
    {
        Sequence animSequence = DOTween.Sequence();
        progressLevel++;

        AudioSource.PlayClipAtPoint(newItemUnlockedSound, Vector2.zero, 2f);

        questionmarks[progressLevel - 1].gameObject.SetActive(false);

        animSequence.Append(progressLine.transform.DOLocalMoveY(progressLine.transform.localPosition.y + lineShift, 3f)
            .SetEase(Ease.OutBack));

        if (progressLevel >= 3)
        {
            progressLevel = 0;
            animSequence.Append(progressLine.transform.DOMove(progressLineStartPos, 1.5f)
                       .SetEase(Ease.OutQuad)).OnComplete(() =>
                       {
                           AudioSource.PlayClipAtPoint(threeСollectedSound, Vector2.zero, 2f);

                           rewardForThreeCollected = Instantiate(rewardPrefab, transform.position, Quaternion.identity);
                           rewardForThreeCollected.stopDropAnimation();
                           rewardForThreeCollected.goToSlotOrDestroy();
                           
                           for (int i = 0; i < questionmarks.Count; i++)
                           {
                               GameObject questionmark = questionmarks[i].gameObject;
                               questionmark.SetActive(true);
                               PlayAppearAnimationQuestions(questionmark.transform);
                           }

                           putObjectsInCell();
                           animSequence.Kill();
                       });
        }

        else {
            animSequence.OnComplete(() => animSequence.Kill());
        }
    }

    public void putObjectsInCell() {
        for (int i = 0; i <= 2; i++) {
            if (maxObjectLevel + i < objectsToSpawn.Count) {
                if (objectsInCellsList[i] != null)
                {
                    Destroy(objectsInCellsList[i].gameObject);
                    objectsInCellsList[i] = null;
                }

                MergeObjects newShadow = Instantiate(objectsToSpawn[i + maxObjectLevel], cellsList[i].transform.position, Quaternion.identity);
                objectsInCellsList[i] = newShadow;
                newShadow.setMergeActive(false);
                ScaleToWorldSize(newShadow.transform.Find("Body").gameObject);
            }
        }
    }

    public void ScaleToWorldSize(GameObject targetObject)
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Объект не задан!");
            return;
        }

        // Получаем рендерер или коллайдер для определения текущих размеров
        Renderer renderer = targetObject.GetComponent<Renderer>();
        Collider collider = targetObject.GetComponent<Collider>();

        Bounds bounds;
        if (renderer != null)
        {
            bounds = renderer.bounds;
        }
        else if (collider != null)
        {
            bounds = collider.bounds;
        }
        else
        {
            Debug.LogWarning("У объекта нет Renderer или Collider — невозможно определить размер!");
            return;
        }

        // Вычисляем текущий размер по наибольшей оси (X, Y или Z)
        float currentSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        if (currentSize <= Mathf.Epsilon) // Защита от деления на ноль
        {
            Debug.LogWarning("Размер объекта слишком мал для масштабирования!");
            return;
        }

        // Вычисляем множитель масштаба, чтобы привести размер к 1 метру
        float scaleFactor = 0.9f / currentSize;

        // Применяем масштабирование
        targetObject.transform.localScale *= scaleFactor;
    }

    public void PlayAppearAnimationQuestions(Transform transform)
    {
        float duration = 1.0f;
        // Сохраняем оригинальный масштаб
        Vector3 originalScale = transform.localScale;

        // Устанавливаем начальный маленький масштаб
        transform.localScale = Vector3.zero;

        // Анимация появления с тряской
        Sequence sequence = DOTween.Sequence();

        // Увеличиваем масштаб
        sequence.Append(transform.DOScale(originalScale * 1.2f, duration * 0.3f).SetEase(Ease.OutBack));

        // Легкая тряска
        //sequence.Append(transform.DOShakeScale(duration * 0.4f, 0.3f, 3));

        // Возвращаем к нормальному масштабу
        sequence.Append(transform.DOScale(originalScale, duration * 0.3f).SetEase(Ease.OutElastic));
    }
}
