using UnityEngine;

public class OutOfBoundsTrigger : SquareTrigger
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    override protected void HandleCollisionAction(MergeObjects mergeObjects)
    {
        mergeObjects.DestroyWithExp();
    }
}
