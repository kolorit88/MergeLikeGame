using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public GameObject bottomImage;
    public GameObject cardPattern;

    protected List<GameObject> ShopPointsCellsList;

    public int cellRowSize = 6;
    public int rows = 2;
    public float padding = 0.2f;


    void Start()
    {
        FillShopCellsList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FillShopCellsList()
    {
        ShopPointsCellsList = new List<GameObject>();
        for (int i = 0; i < rows; i++)
        {
            float y_coord = bottomImage.transform.localScale.y / rows;
            float x_coord = padding + cardPattern.transform.localScale.x / 2;

            for (int j = 0; j < cellRowSize; j++)
            {
                // доработать координаты
                GameObject cellCentrePoint = new GameObject();
                cellCentrePoint.transform.position = new Vector3(y_coord, x_coord, bottomImage.transform.position.z);
                ShopPointsCellsList.Add(cellCentrePoint);
                Debug.Log(cellCentrePoint);
            }
        }
    }
}
