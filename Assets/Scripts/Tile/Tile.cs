using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour
{
    GameObject PREFAB;

    public GameObject visual;
    public PrefabHolder prefabHolder;

    public TileType type = TileType.Normal;
    public Vector2 gridPosition = Vector2.zero;

    public int movementCost = 1;
    public bool impassible = false;

    public List<Tile> neighbors = new List<Tile>();

    // Use this for initialization
    void Start()
    {
        if(Application.loadedLevelName == "Battle")
        {
            GenerateNeighbors();
        }
    }

    void GenerateNeighbors()
    {
        neighbors = new List<Tile>();
        if(gridPosition.y > 0)
        {
            Vector2 n = new Vector2(gridPosition.x, gridPosition.y - 1);
            neighbors.Add(GameManager.instance.map[(int)n.x][(int)n.y]);
        }

        if (gridPosition.y < GameManager.instance.mapSizeY - 1)
        {
            Vector2 n = new Vector2(gridPosition.x, gridPosition.y + 1);
            neighbors.Add(GameManager.instance.map[(int)n.x][(int)n.y]);
        }

        if (gridPosition.x > 0)
        {
            Vector2 n = new Vector2(gridPosition.x-1, gridPosition.y );
            neighbors.Add(GameManager.instance.map[(int)n.x][(int)n.y]);
        }

        if (gridPosition.x < GameManager.instance.mapSizeX - 1)
        {
            Vector2 n = new Vector2(gridPosition.x + 1, gridPosition.y);
            neighbors.Add(GameManager.instance.map[(int)n.x][(int)n.y]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseEnter()
    {
        if (Application.loadedLevelName == "MapCreaterScene" && Input.GetMouseButton(0))
        {
            setType(MapCreatorManager.instance.palletsSelection);
        }

        /*
		if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving) {
			transform.renderer.material.color = Color.blue;
		} else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking) {
			transform.renderer.material.color = Color.red;
		}
		*/
        //Debug.Log("my position is (" + gridPosition.x + "," + gridPosition.y);
    }

    void OnMouseExit()
    {
        //transform.renderer.material.color = Color.white;
    }


    void OnMouseDown()
    {
        if (Application.loadedLevelName == "Battle")
        {
            if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].moving)
            {
                GameManager.instance.moveCurrentPlayer(this);
            }
            else if (GameManager.instance.players[GameManager.instance.currentPlayerIndex].attacking)
            {
                GameManager.instance.attackWithCurrentPlayer(this);
            }
            else
            {
                if (Input.GetMouseButton(1))
                {
                    impassible = impassible ? false : true;
                    if (impassible)
                    {
                        visual.transform.GetComponent<Renderer>().materials[0].color = new Color(.5f, .5f, 0.0f);
                    }
                    else
                    {
                        visual.transform.GetComponent<Renderer>().materials[0].color = Color.white;
                    }
                }
            }
        }
        else if(Application.loadedLevelName == "MapCreaterScene")
        {
            setType(MapCreatorManager.instance.palletsSelection);
        }
    }

    public void setType(TileType t)
    {
        type = t;

        switch(t)
        {
            case TileType.Normal:
                movementCost = 1;
                impassible = false;
                PREFAB = PrefabHolder.instance.TILE_NORMAL_PREFAB;
                break;
            case TileType.Difficult:
                movementCost = 2;
                impassible = false;
                PREFAB = PrefabHolder.instance.TILE_DIFFICULT_PREFAB;
                break;

            case TileType.VeryDifficult:
                movementCost = 4;
                impassible = false;
                PREFAB = PrefabHolder.instance.TILE_VERY_DIFFICULT_PREFAB;
                break;

            case TileType.Impassible:
                movementCost = 999;
                impassible = true;
                PREFAB = PrefabHolder.instance.TILE_IMPASSIBLE_PREFAB;
                break;
        }

        generateVisuals();
    }

    public void generateVisuals()
    {
        GameObject container = transform.Find("Visuals").gameObject;
        //initially remove all children
        for (int i = 0; i < container.transform.childCount; i++)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }

        GameObject newVisual = (GameObject)Instantiate(PREFAB, transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
        newVisual.transform.parent = container.transform;

        visual = newVisual;
    }

}
