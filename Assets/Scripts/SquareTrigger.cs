using UnityEngine;

public class SquareTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MergeObjects mergeObject = other.GetComponentInParent<MergeObjects>();
        Debug.Log(mergeObject);

        if (mergeObject != null)
        {
            HandleCollisionAction(mergeObject);
        }
    }

    protected virtual void HandleCollisionAction(MergeObjects mergeObjects)
    {

    }
}
