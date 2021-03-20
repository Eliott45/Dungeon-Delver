using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Работоспособность плитки
/// </summary>
public class Tile : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int x;
    public int y;
    public int tileNum;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eTileNum"> Необязательный параметр </param>
    public void SetTile(int eX, int eY, int eTileNum = -1)
    {
        x = eX;
        y = eY;
        transform.localPosition = new Vector3(x, y, 0);
        // Вернуть строку в заданном формате. "D" - строка должна представлять число в десятичной СС, "3" - строка должна содержать не менее 3-ех сиволов
        gameObject.name = x.ToString("D3") + "x" + y.ToString("D3"); 

        if (eTileNum == -1) // Если параметр не был передан..
        {
            eTileNum = TileCamera.GET_MAP(x, y); //.. то он будет получен вызовом GET_MAP
        }
        tileNum = eTileNum;
        GetComponent<SpriteRenderer>().sprite = TileCamera.SPRITES[tileNum]; // Присвоить спрайт плитке
    }
}
