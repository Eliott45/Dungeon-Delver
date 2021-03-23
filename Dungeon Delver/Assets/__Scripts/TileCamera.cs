using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileSwap
{
    public int tileNum;
    public GameObject swapPrefab;
    public GameObject guranteedItemDrop;
    public int overrideTileNum = -1;
}
/// <summary>
/// Отвечает за сохранение и извлечение всех спрайтов из изображения, а так же за чтения файла для определение местоположений плиток.
/// </summary>
public class TileCamera : MonoBehaviour
{
    static private int W, H;
    static private int[,] MAP;
    static public Sprite[] SPRITES;
    static public Transform TILE_ANCHOR;
    static public Tile[,] TILES;
    static public string COLLISIONS;

    [Header("Set in Inspector")]
    public TextAsset mapData;
    public Texture2D mapTiles;
    public TextAsset mapCollisions;
    public Tile tilePrefab;
    public int defaultTileNum;
    public List<TileSwap> tileSwaps;

    private Dictionary<int, TileSwap> tileSwapDict;
    private Transform enemyAnchor, itemAnchor;


    private void Awake()
    {
        COLLISIONS = Utils.RemoveLineEndings(mapCollisions.text);
        PrepareTileSwapDict();
        enemyAnchor = (new GameObject("Enemy Anchor")).transform;
        itemAnchor = (new GameObject("Item Anchor")).transform;
        LoadMap();
    }

    public void LoadMap()
    {
        // Создать TILE_ANCHOR. Он будет играть роль родителя для всех плиток Tile.
        GameObject go = new GameObject("TILE_ANCHOR");
        TILE_ANCHOR = go.transform;

        // Загрузить все спрайты из mapTiles
        SPRITES = Resources.LoadAll<Sprite>(mapTiles.name); // Изображение находится в папке Resources

        // Прочитать информацию для карты
        string[] lines = mapData.text.Split('\n');
        H = lines.Length;
        string[] tileNums = lines[0].Split(' ');
        W = tileNums.Length;

        System.Globalization.NumberStyles hexNum; // Используется для преобразования строк с двузначными шестнадцатеричыми кодами в целые числа
        hexNum = System.Globalization.NumberStyles.HexNumber; 
        // Сохранить информациию для карты в двумерный массив для ускорения доступа
        MAP = new int[W, H];
        for (int j = 0; j < H; j++)
        {
            tileNums = lines[j].Split(' ');
            for (int i = 0; i < W; i++)
            {
                if(tileNums[i] == "..")
                {
                    MAP[i, j] = 0;
                } else
                {
                    MAP[i, j] = int.Parse(tileNums[i], hexNum); // hexNum - сообщает то что строка содержит 16-ный код
                }
                CheckTileSwaps(i, j);
            }
        }
        //print("Parse " + SPRITES.Length + " sprites.");
        //print("Map size: " + W + " wide by " + H + " high");

        ShowMap();
    }

    /// <summary>
    /// Генерирует плитки сразу для всей карты.
    /// </summary>
    void ShowMap()
    {
        TILES = new Tile[W, H];

        // Просмотреть всю карту и создать плитки, где необходимо 
        for (int j = 0; j < H; j++)
        {
            for (int i = 0; i < W; i++)
            {
                if (MAP[i,j] != 0)
                {
                    Tile ti = Instantiate<Tile>(tilePrefab);
                    ti.transform.SetParent(TILE_ANCHOR);
                    ti.SetTile(i, j);
                    TILES[i, j] = ti;
                }
            }
        }
    }

    void PrepareTileSwapDict()
    {
        tileSwapDict = new Dictionary<int, TileSwap>();
        foreach (TileSwap ts in tileSwaps)
        {
            tileSwapDict.Add(ts.tileNum, ts);
        }
    }

    void CheckTileSwaps(int i, int j)
    {
        int tNum = GET_MAP(i, j);
        if (!tileSwapDict.ContainsKey(tNum)) return;
        // Мы можем заменить плитку
        TileSwap ts = tileSwapDict[tNum];
        if (ts.swapPrefab != null)
        {
            GameObject go = Instantiate(ts.swapPrefab);
            Enemy e = go.GetComponent<Enemy>();
            if ( e != null)
            {
                go.transform.SetParent(enemyAnchor);
            } else
            {
                go.transform.SetParent(itemAnchor);
            }
            go.transform.position = new Vector3(i, j, 0);
            if(ts.guranteedItemDrop != null)
            {
                if (e != null)
                {
                    e.guranteedItemDrop = ts.guranteedItemDrop;
                }
            }
        }

        // Заменить другой плиткой 
        if (ts.overrideTileNum == -1)
        {
            SET_MAP(i, j, defaultTileNum);
        } else
        {
            SET_MAP(i, j, ts.overrideTileNum);
        }
    }

    /// <summary>
    /// Обеспечивает защищенный доступ для чтения MAP, предотвращая исключение IndexOutORangeSception.
    /// </summary>
    static public int GET_MAP(int x, int y)
    {
        if(x < 0 || x >= W || y < 0 || y>= H)
        {
            return -1; // Предотвратить исключение IndexOutORangeSception
        }
        return MAP[x, y];
    }

    /// <summary>
    /// Перегруженная float-версия GET_MAP()
    /// </summary>
    static public int GET_MAP(float x, float y)
    {
        int tX = Mathf.RoundToInt(x);
        int tY = Mathf.RoundToInt(y - 0.25f); // Это выражение учитывает сложную перспективу игры, когда верхняя половина тела персонажа игрока может находится за пределами плитки, но счиается, что он находится на этой плитке.
        return GET_MAP(tX, tY);
    }

    /// <summary>
    /// беспечивает защищенный доступ для записи MAP, предотвращая исключение IndexOutORangeSception.
    /// </summary>
    static public void SET_MAP(int x, int y, int tNum)
    {
        if (x < 0 || x >= W || y < 0 || y >= H)
        {
            return; // Предотвратить исключение IndexOutORangeSception
        }
        MAP[x, y]= tNum;
    }
}
