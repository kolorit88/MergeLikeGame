using System.Collections.Generic;

using UnityEngine;

public class BonuseSlotsBar : MonoBehaviour
{
    public int slotsQuantity = 10;
    public float cellWidth;

    public List<Vector3> slotsPositionList = new List<Vector3>();
    public List<Bonus> bonuseInCellsList = new List<Bonus>();

    void Start()
    {
        for (int i = 0; i < slotsQuantity; i++) {
            bonuseInCellsList.Add(null);
        }

        CalculateSlotsPositions();
    }

    void Update()
    {
        
    }

    public void CalculateSlotsPositions()
    {
        float length = transform.localScale.x;

        // Вычисляем ширину одной ячейки
        float cellWidth = length / slotsQuantity;

        // Начальная точка (край прямоугольника)
        float startPosition = -length / 2f; // Центрируем относительно объекта

        // Добавляем позиции середин ячеек
        for (int i = 0; i < slotsQuantity; i++)
        {
            float xPos = startPosition + (cellWidth * i) + (cellWidth / 2f);
            Vector3 position = transform.position + transform.right * xPos;
            slotsPositionList.Add(position);
        }
    }

    public Vector3 putInSlot(Bonus bonus) {
        for (int i = 0; i < slotsQuantity; i++)
        {
            if (bonuseInCellsList[i] != null) {
                if (bonuseInCellsList[i].gameObject.name == bonus.gameObject.name)
                {
                    return slotsPositionList[i];
                }
            }
        }

        for (int i = 0; i < slotsQuantity; i++)
        {
            if(bonuseInCellsList[i] == null) {
                bonuseInCellsList[i] = bonus;
                return slotsPositionList[i];
            }
        }
        return new Vector3(0, 0, 0);
    }

    public void removeFromSlot(Bonus bonus) {
        for (int i = 0; i < slotsQuantity; i++)
        {
            if (bonuseInCellsList[i] != null && bonuseInCellsList[i].gameObject.name == bonus.gameObject.name)
            {
                bonuseInCellsList[i] = null;
                break;
            }
        }
    }

    public Bonus bonusIfAlreadyExists(Bonus bonus) {
        for (int i = 0; i < slotsQuantity; i++)
        {
            if (bonuseInCellsList[i] != null)
            {
                if (bonuseInCellsList[i].gameObject.name == bonus.gameObject.name)
                {
                    return bonuseInCellsList[i];
                }
            }
        }

        return null;
    }
}
