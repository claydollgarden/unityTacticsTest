﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UserPlayer : Player
{
	public GameObject nowPlayingUI;

    // Use this for initialization
    void Start()
    {
		nowPlayingUI.SetActive(false);
    }

    // Update is called once per frame
    public override void Update()
    {
        if (GameManager.instance.players[GameManager.instance.currentPlayerIndex] == this)
        {
            transform.GetComponent<Renderer>().material.color = Color.green;
			nowPlayingUI.SetActive(true);
        }
        else 
		{
            transform.GetComponent<Renderer>().material.color = Color.white;
			nowPlayingUI.SetActive(false);
        }
        base.Update();
    }

    public override void TurnUpdate()
    {
        if (positionQueue.Count > 0)
        {
            transform.position += (positionQueue[0] - transform.position).normalized * moveSpeed * Time.deltaTime;
            if (Vector3.Distance(positionQueue[0], transform.position) <= 0.1f)
            {
				transform.rotation = Quaternion.LookRotation ((positionQueue [0] - transform.position).normalized, Vector3.up);
				Debug.Log( "forwardVector" + Quaternion.LookRotation((positionQueue[0] - transform.position).normalized, Vector3.up));
				
                transform.position = positionQueue[0];
                positionQueue.RemoveAt(0);
                if (positionQueue.Count == 0)
                {
                    moving = true;
                }
            }

        }
        else {
            //priority queue
            List<Tile> attacktilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], attackRange, true);
            //List<Tile> movementToAttackTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + attackRange);
            List<Tile> movementTilesInRange = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint + 1000);
            //attack if in range and with lowest HP
            if (!attacking && attacktilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(UserPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                var opponentsInRange = attacktilesInRange.Select(x => GameManager.instance.players.Where(y => y.GetType() != typeof(UserPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
                Player opponent = opponentsInRange.OrderBy(x => x != null ? -x.HP : 1000).First();

                GameManager.instance.removeTileHighlights();
                GameManager.instance.highlightTilesAt(gridPosition, Color.red, attackRange);

                GameManager.instance.attackWithCurrentPlayer(GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y]);
            }
            //move toward nearest opponent
            else if (!moving && movementTilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(UserPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() > 0)
            {
                var opponentsInRange = movementTilesInRange.Select(x => GameManager.instance.players.Where(y => y.GetType() != typeof(UserPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0 ? GameManager.instance.players.Where(y => y.gridPosition == x.gridPosition).First() : null).ToList();
                Player opponent = opponentsInRange.OrderBy(x => x != null ? -x.HP : 1000).ThenBy(x => x != null ? TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)x.gridPosition.x][(int)x.gridPosition.y]).Count() : 1000).First();

                GameManager.instance.removeTileHighlights();
                GameManager.instance.highlightTilesAt(gridPosition, Color.blue, movementPerActionPoint, false);

                List<Tile> path = TilePathFinder.FindPath(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], GameManager.instance.map[(int)opponent.gridPosition.x][(int)opponent.gridPosition.y], GameManager.instance.players.Where(x => x.gridPosition != gridPosition && x.gridPosition != opponent.gridPosition).Select(x => x.gridPosition).ToArray());
                if (path == null)
                {
                    GameManager.instance.nextTurn();
                }

                if (path.Count() > 1)
                {
                    List<Tile> actualMovement = TileHighlight.FindHighlight(GameManager.instance.map[(int)gridPosition.x][(int)gridPosition.y], movementPerActionPoint, GameManager.instance.players.Where(x => x.gridPosition != gridPosition).Select(x => x.gridPosition).ToArray());
                    path.Reverse();
                    if (path.Where(x => actualMovement.Contains(x)).Count() > 0) GameManager.instance.moveCurrentPlayer(path.Where(x => actualMovement.Contains(x)).First());
                }
            }
            else if(moving && attacktilesInRange.Where(x => GameManager.instance.players.Where(y => y.GetType() != typeof(UserPlayer) && y.HP > 0 && y != this && y.gridPosition == x.gridPosition).Count() > 0).Count() <= 0)
            {
                attacking = true;
            }
        }

        base.TurnUpdate();
    }

    public override void TurnOnGUI()
    {
        base.TurnOnGUI();
    }
}
