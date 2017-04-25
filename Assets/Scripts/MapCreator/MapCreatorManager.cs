using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MapCreatorManager : MonoBehaviour {
    public static MapCreatorManager instance;

    public int mapSizeX = 20;
    public int mapSizeY = 11;
    public List<List<Tile>> map = new List<List<Tile>>();

    public TileType palletsSelection = TileType.Normal;

    Transform mapTransform;

    void Awake()
    {
        
    }

    void Start()
    {
        instance = this;

        mapTransform = transform.FindChild("Map");

        generatBlankeMap(mapSizeX, mapSizeY);
    }

    void generatBlankeMap(int mSizeX, int mSizeY)
    {
        mapSizeX = mSizeX;
        mapSizeY = mSizeY;

        //initially remove all children
        for (int i = 0; i < mapTransform.childCount; i++)
        {
            Destroy(mapTransform.GetChild(i).gameObject);
        }

        map = new List<List<Tile>>();
        for (int i = 0; i < mapSizeX; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < mapSizeY; j++)
            {
                Tile tile = ((GameObject)Instantiate(PrefabHolder.instance.BASE_TILE_PREFAB, new Vector3(i - Mathf.Floor(mapSizeX / 2), 0, -j + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
                tile.transform.parent = mapTransform;
                tile.gridPosition = new Vector2(i, j);
                tile.setType(TileType.Normal);
                row.Add(tile);
            }
            map.Add(row);
        }
    }

    void loadMapFromXml()
    {
        MapXmlContainer container = MapSaveLoad.Load("map.xml");

        mapSizeX = container.sizeX;
        mapSizeY = container.sizeY;

        //initially remove all children
        for (int i = 0; i < mapTransform.childCount; i++)
        {
            Destroy(mapTransform.GetChild(i).gameObject);
        }

        map = new List<List<Tile>>();
        for (int i = 0; i < mapSizeX; i++)
        {
            List<Tile> row = new List<Tile>();
            for (int j = 0; j < mapSizeY; j++)
            {
                Tile tile = ((GameObject)Instantiate(PrefabHolder.instance.BASE_TILE_PREFAB, new Vector3(i - Mathf.Floor(mapSizeX / 2), 0, -j + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
                tile.transform.parent = mapTransform;
                tile.gridPosition = new Vector2(i, j);
                tile.setType((TileType)container.tiles.Where(x => x.locX == i && x.locY == j).First().id);
                row.Add(tile);
            }
            map.Add(row);
        }
    }

    void saveMapToXml()
    {
        MapSaveLoad.Save(MapSaveLoad.CreateMapContainer(map), "map.xml");
    }

    void OnGUI()
    {
        Rect rect = new Rect(10, Screen.height - 80, 100, 60);

        if( GUI.Button(rect, "Normal"))
        {
            palletsSelection = TileType.Normal;
        }

        rect = new Rect(10 + (100 + 10) * 1, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Difficult"))
        {
            palletsSelection = TileType.Difficult;
        }

        rect = new Rect(10 + (100 + 10) * 2, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "VeryDifficult"))
        {
            palletsSelection = TileType.VeryDifficult;
        }

        rect = new Rect(10 + (100 + 10) * 3, Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Impassible"))
        {
            palletsSelection = TileType.Impassible;
        }

        rect = new Rect(Screen.width - (10 + (100 + 10) * 3), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Clear Map"))
        {
            generatBlankeMap(mapSizeX, mapSizeY);
        }

        rect = new Rect(Screen.width - (10 + (100 + 10) * 2), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Load Map"))
        {
            loadMapFromXml();
        }

        rect = new Rect(Screen.width - (10 + (100 + 10) * 1), Screen.height - 80, 100, 60);

        if (GUI.Button(rect, "Save Map"))
        {
            saveMapToXml();
        }
    }
}
