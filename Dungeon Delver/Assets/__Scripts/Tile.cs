using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Плитка.
/// </summary>
public class Tile : MonoBehaviour
{
    [Header("Set Dynamically")]
    public int x;
    public int y;
    public int tileNum;
    private BoxCollider _bColl;

    private void Awake()
    {
        _bColl = GetComponent<BoxCollider>();
    }

    /// <summary>
    /// Установить свойство плитки.
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
        } else
        {
            TileCamera.SET_MAP(x, y, eTileNum); // Заменить плитку, если необходимо
        }
        tileNum = eTileNum;
        GetComponent<SpriteRenderer>().sprite = TileCamera.SPRITES[tileNum]; // Присвоить спрайт плитке

        SetCollider();
    }

    /// <summary>
    /// Настроить коллайдер для плитки.
    /// </summary>
    void SetCollider()
    {
        // Получить информацию о коллайдере из Collider DelverCollisions.txt
        _bColl.enabled = true;
        char c = TileCamera.COLLISIONS[tileNum]; // Извлечь символ, определяющиий вид столконвения
        switch (c)
        {
            case 'S': // Вся плитка
                _bColl.center = Vector3.zero;
                _bColl.size = Vector3.one;
                break;
            case 'W': // Верхняя половина
                _bColl.center = new Vector3 (0, 0.25f, 0);
                _bColl.size = new Vector3(1,0.5f,1);
                break;
            case 'A': // Левая половина
                _bColl.center = new Vector3(-0.25f, 0, 0);
                _bColl.size = new Vector3(0.5f, 1, 1);
                break;
            case 'D': // Правая половина
                _bColl.center = new Vector3(0.25f, 0, 0);
                _bColl.size = new Vector3(0.5f, 1, 1);
                break;

            //----- Дополнительные коды -----\\
            case 'Q': // Левая верхняя четверть
                _bColl.center = new Vector3(-0.25f, 0.25f, 0);
                _bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'E': // Правая верхняя четверть
                _bColl.center = new Vector3(0.25f, 0.25f, 0);
                _bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'Z': // Левая нижняя четверть
                _bColl.center = new Vector3(-0.25f, -0.25f, 0);
                _bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;
            case 'X': // Нижняя половина
                _bColl.center = new Vector3(0, -0.25f, 0);
                _bColl.size = new Vector3(1, 0.5f, 1);
                break;
            case 'С': // Правая нижняя четверть
                _bColl.center = new Vector3(0.25f, -0.25f, 0);
                _bColl.size = new Vector3(0.5f, 0.5f, 1);
                break;

            default: // Все остальное: _, |, и др.
                _bColl.enabled = false;
                break;
        }
    }
}
