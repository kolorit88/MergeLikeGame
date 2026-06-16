using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class SpawnOnSquareLine : MonoBehaviour
{
    [Header("Настройки спавна")]
    public List<MergeObjects> objectsToSpawn;  // Префабы объекта для спавна
    public bool useLocalScale = true; // Использовать localScale для определения длины
    public float handCooldown = 1f;
    public float handThrowDownPower = 1f;

    private float cooldownCointer;
    private float _lineLength;
    private float _lineLeftEdge;
    private Camera _mainCamera;
    private float speedLevel = 0f;
    private int itemsLevel = 0;
    private int expQuantituLevel = 0; 

    public MergeObjects currentObjectInHand;
    public bool enableDropItems = true;

    void Start()
    {
        cooldownCointer = handCooldown;

        _mainCamera = Camera.main;
        CalculateLineDimensions();
        spawnObjectInHand();
    }

    void Update()
    {
        updateCooldownHand();

        updateCurrentObjectInHandPosicion();
        listenTouch();
    }

    public void releaseCurrentObjectInHand() {
        if (currentObjectInHand != null)
        {
            currentObjectInHand.setMergeActive(true);
            currentObjectInHand.transform.Find("Body").GetComponent<Rigidbody2D>().AddForce(Vector2.down * handThrowDownPower, ForceMode2D.Impulse);
            currentObjectInHand = null;
        }
    }

    public void spawnObjectInHand() {
        Vector3 spawnPosition = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
        currentObjectInHand = Instantiate(objectsToSpawn[Random.Range(0, itemsLevel + 1)], spawnPosition, Quaternion.identity);
        currentObjectInHand.expOrbQuantity = expQuantituLevel + 1;
        currentObjectInHand.setMergeActive(false);
    }

    public void updateCurrentObjectInHandPosicion() {

        if (currentObjectInHand != null)
        {
            Vector3 touchPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

            float spawnX = Mathf.Clamp(
                touchPosition.x,
                _lineLeftEdge,
                _lineLeftEdge + _lineLength
            );

            Vector3 newObjectPosition = new Vector3(spawnX, transform.position.y, 0);

        
            currentObjectInHand.transform.position = newObjectPosition;
        }
        
    }

    public void listenTouch()
    {
        if (cooldownCointer != 0 || !enableDropItems)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            releaseCurrentObjectInHand();
            cooldownCointer = handCooldown;
        }

    }

    public void updateCooldownHand()
    {
        if (cooldownCointer > 0)
        {
            cooldownCointer -= Time.deltaTime;

            if (cooldownCointer <= 0)
            {
                cooldownCointer = 0;
            }
        }
        
        if (cooldownCointer == 0 && currentObjectInHand == null)
        {
            spawnObjectInHand();
        }
    }

    private void CalculateLineDimensions()
    {
        if (useLocalScale)
        {
            _lineLength = transform.localScale.x;
        }

        _lineLeftEdge = transform.position.x - _lineLength / 2f;
    }

    public void handSpeedLevelUp() {
        if (speedLevel < 3)
        {
            speedLevel += 1;
            handCooldown = 1.0f - (speedLevel / 10) * 1.5f;
            handThrowDownPower = 3 * speedLevel;
        }
    }

    public void handItemsLevelUp()
    {
        if (itemsLevel < 3) { 
            itemsLevel += 1;
        }
    }

    public void handExpQuantityLevelUp()
    {
        if (expQuantituLevel < 3)
        {
            expQuantituLevel += 1;
        }
    }

    public float getSpeedLevel() { 
        return speedLevel;
    }

    public int getItemsLevel()
    {
        return itemsLevel;
    }

    public int getExpQuantityLevel()
    {
        return expQuantituLevel;
    }

    public void DisableDropItems()
    {
        enableDropItems = false;
    }

    public void EnableDropItems()
    {
        enableDropItems = true;
    }
}