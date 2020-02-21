using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knifer : Player
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ProcessAction(PlayerAction action)
    {
        switch (action)
        {
            case PlayerAction.MoveToTile: // Player clicked Move button
                
                if(curState == PlayerState.FirstMove)
                {
                    curState = PlayerState.SecondMove;
                }
                else if(curState == PlayerState.SecondMove)
                {
                    curState = PlayerState.EndingTurn;
                }

                Tile clickedTile = board.GetTileFromID(currentTileIdx);
                
                foreach (Player other in Game.Instance.players) 
                {
                    if (clickedTile.Idx == other.currentTileIdx) 
                    {
                        Game.Instance.EndGame(this);
                    }
				}
                
                StartCoroutine(MoveToTile(clickedTile));
                
                break;
            case PlayerAction.Skip:
                Debug.Log("none of the above");
                break;
        }
    }

    public override void ExecuteState()
    {
        switch (curState)
        {
            case PlayerState.Idle:
                curState = PlayerState.FirstMove;
                StartCoroutine(StartTurn());
                break;
            case PlayerState.FirstMove:
                Debug.Log("first move");
                PresentMovementOptions();
                break;
            case PlayerState.SecondMove:
                Debug.Log("second move");
                PresentMovementOptions();
                break;
            case PlayerState.Rotating:
                Debug.Log("rotating");
                PresentRotatingOptions();
                break;
            case PlayerState.EndingTurn:
                Debug.Log("Ending turn...");
                curState = PlayerState.Idle;
                Game.Instance.NextTurn();
                //StartCoroutine(EndTurn(clickedTile));
                break;
        }
    }

    IEnumerator StartTurn()
    {
        uint waitFlags = 0b0000;
        //Show player start transition
        /*
        TransitionHandler.ShowTransition(this.playerName, () =>
        {
            waitFlags |= 0b0001;
        });
        */

        //TODO: Hide Knifers


        //Move camera to player
        waitFlags = 0b0001;
        board.PanCameraToPlayerTile(currentTileIdx, () =>
        {
            waitFlags |= 0b0010;
        });
        yield return new WaitUntil(() => waitFlags == 0b0011);

        waitFlags = 0b0000;
        //Temporarily shows players in immediate cone in front of player
        //UpdatePlayerView(false);
        StartCoroutine(PulseImmediateCone(() =>
        {
            waitFlags |= 0b0001;
        }));
        yield return new WaitUntil(() => waitFlags == 0b0001);

        ExecuteState();
    }

    void PresentMovementOptions()
    {
        Tile curTile = board.GetTileFromID(this.currentTileIdx);
        Game.Instance.tileButtons = board.GetAdjacentTilesFromID(this.currentTileIdx);
        foreach (Tile neighborTile in Game.Instance.tileButtons)
        {
            neighborTile.gameObject.layer = 0;
            neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        Game.Instance.watchButton.GetComponent<Button>().interactable = true;
    }
}
