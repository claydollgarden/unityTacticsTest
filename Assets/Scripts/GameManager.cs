using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject TilePrefab;
    public GameObject UserPlayerPrefab;
    public GameObject AIPlayerPrefab;

    public int mapSizeX = 20;
    public int mapSizeY = 11;
    Transform mapTransform;

    public List<List<Tile>> map = new List<List<Tile>>();
    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;

    public bool gameStartFlg = false;

    public GameObject hitEffect;

    void Awake()
    {
        instance = this;

        mapTransform = transform.FindChild("Map");
    }

    // Use this for initialization
    void Start()
    {
        generateMap();
        generatePlayers();
    }

    void Update()
    {
        if(gameStartFlg)
        {
            if (players[currentPlayerIndex].HP > 0)
            {
                players[currentPlayerIndex].TurnUpdate();
            }
            else
            {
                nextTurn();
            }
        }
    }

    void OnGUI()
    {
        if (players[currentPlayerIndex].HP > 0)
        {
            players[currentPlayerIndex].TurnOnGUI();
        }
    }

    public void nextTurn()
    {
        Destroy(GameObject.Find(hitEffect.name + "(Clone)"));

        if (currentPlayerIndex + 1 < players.Count)
        {
            currentPlayerIndex++;
        }
        else
        {
            currentPlayerIndex = 0;
        }
    }

    public void highlightTilesAt(Vector2 originLocation, Color highlightColor, int distance, bool ignorePlayers = true)
    {
        List<Tile> highlightedTiles = new List<Tile>();

        if( ignorePlayers )
        {
            highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance, highlightColor == Color.red);
        }
        else
        {
            highlightedTiles = TileHighlight.FindHighlight(map[(int)originLocation.x][(int)originLocation.y], distance, players.Where(x => x.gridPosition != originLocation).Select(x => x.gridPosition).ToArray(), highlightColor == Color.red);
        }

        foreach (Tile t in highlightedTiles)
        {
            t.visual.GetComponent<Renderer>().materials[0].color = highlightColor;
        }
    }

    public void removeTileHighlights()
    {
        for(int i = 0; i < mapSizeX; i ++)
        {
            for(int j = 0; j <mapSizeY; j++)
            {
                if (!map[i][j].impassible)
                {
                    map[i][j].visual.GetComponent<Renderer>().materials[0].color = Color.white;
                }
            }
        }
    }

    public void moveCurrentPlayer(Tile destTile)
    {
        if (destTile.visual.GetComponent<Renderer>().materials[0].color != Color.white && !destTile.impassible && players[currentPlayerIndex].positionQueue.Count == 0)
        {
            removeTileHighlights();
            players[currentPlayerIndex].moving = false;
            foreach (Tile t in TilePathFinder.FindPath(map[(int)players[currentPlayerIndex].gridPosition.x][(int)players[currentPlayerIndex].gridPosition.y], destTile, players.Where(x => x.gridPosition != destTile.gridPosition && x.gridPosition != players[currentPlayerIndex].gridPosition).Select(x => x.gridPosition).ToArray()))
            {
                players[currentPlayerIndex].positionQueue.Add(map[(int)t.gridPosition.x][(int)t.gridPosition.y].transform.position + 1.5f * Vector3.up);
                Debug.Log("(" + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].x + "," + players[currentPlayerIndex].positionQueue[players[currentPlayerIndex].positionQueue.Count - 1].y + ")");
            }
            players[currentPlayerIndex].gridPosition = destTile.gridPosition;

        }
        else {
            Debug.Log("destination invalid");
            destTile.visual.GetComponent<Renderer>().materials[0].color = Color.cyan;
        }
    }

    public void attackWithCurrentPlayer(Tile destTile)
    {
        if (destTile.visual.GetComponent<Renderer>().materials[0].color != Color.white && !destTile.impassible)
        {
            Player target = null;
            foreach (Player p in players)
            {
                if (p.gridPosition == destTile.gridPosition)
                {
                    target = p;
                }
            }

            if (target != null)
            {

                //players[currentPlayerIndex].actionPoints--;
                players[currentPlayerIndex].attacking = true;

                removeTileHighlights();

                //attack logic
                //roll to hit
                bool hit = Random.Range(0.0f, 1.0f) <= players[currentPlayerIndex].attackChance - target.evade;

                if (hit)
                {
                    //damage logic
                    int amountOfDamage = Mathf.Max(0, (int)Mathf.Floor(players[currentPlayerIndex].damageBase + Random.Range(0, players[currentPlayerIndex].damageRollSides)) - target.damageReduction);

                    target.HP -= amountOfDamage;

                    Debug.Log(players[currentPlayerIndex].playerName + " successfuly hit " + target.playerName + " for " + amountOfDamage + " damage!");
                    Instantiate(hitEffect, players[currentPlayerIndex].transform.position, players[currentPlayerIndex].transform.rotation);

                }
                else
                {
                     Debug.Log(players[currentPlayerIndex].playerName + " missed " + target.playerName + "!");
                }
            }
        }
    }


    void generateMap()
    {
        loadMapFromXml();
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

    void generatePlayers()
    {
        UserPlayer player;

        player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2), 1.5f, -0 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(0, 0);
        player.playerName = "ssh";
        player.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(player);

        player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3((mapSizeX - 1) - Mathf.Floor(mapSizeX / 2), 1.5f, -(mapSizeY - 1) + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(mapSizeX - 1, mapSizeY - 1);
        player.playerName = "ksh";
        player.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(player);

        player = ((GameObject)Instantiate(UserPlayerPrefab, new Vector3(4 - Mathf.Floor(mapSizeX / 2), 1.5f, -4 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<UserPlayer>();
        player.gridPosition = new Vector2(4, 4);
        player.playerName = "kch";
        player.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(player);

        AIPlayer aiplayer = ((GameObject)Instantiate(AIPlayerPrefab, new Vector3(0 - Mathf.Floor(mapSizeX / 2),1.5f, -10 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(0, 10);
        aiplayer.playerName = "kjh";
        aiplayer.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(aiplayer);

        aiplayer = ((GameObject)Instantiate(AIPlayerPrefab, new Vector3(1 - Mathf.Floor(mapSizeX / 2), 1.5f, -10 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(1, 10);
        aiplayer.playerName = "cyh";
        aiplayer.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(aiplayer);

        aiplayer = ((GameObject)Instantiate(AIPlayerPrefab, new Vector3(4 - Mathf.Floor(mapSizeX / 2), 1.5f, -8 + Mathf.Floor(mapSizeY / 2)), Quaternion.Euler(new Vector3()))).GetComponent<AIPlayer>();
        aiplayer.gridPosition = new Vector2(4, 8);
        aiplayer.playerName = "jyj";
        aiplayer.handWeapons.Add(Weapon.FromKey(WeaponKey.LongSword));
        players.Add(aiplayer);
    }

    public void SetGameStart()
    {
        gameStartFlg = true;
    }

    public void ResetGame()
    {
        int scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }
}
