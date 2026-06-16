using UnityEngine;

public class MergeHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (transform.parent.GetComponent<MergeObjects>().getMergeActive())
        {
            // Получаем родительские объекты
            MergeObjects thisMerge = transform.parent.GetComponent<MergeObjects>();
            MergeObjects otherMerge = collision.transform.parent.GetComponent<MergeObjects>();

            if (thisMerge != null && otherMerge != null &&
                thisMerge.enabled && otherMerge.enabled &&
                thisMerge.gameObject.name == otherMerge.gameObject.name)
            {
                // Вычисляем позицию между телами
                Vector2 mergePosition = (transform.position + collision.transform.position) / 2f;

                // Создаем новый объект
                thisMerge.CreateMergedObject(mergePosition);

                // Уничтожаем старые объекты (родителей)
                Destroy(thisMerge.gameObject);
                Destroy(otherMerge.gameObject);

                // Отключаем скрипты
                thisMerge.enabled = false;
                otherMerge.enabled = false;
            }
        }
    }
}
