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

    public override void ExecuteState()
    {
        Game.Instance.NextTurn();
    }

    /*
    public override void ExecuteState()
    {
        switch (curState)
        {
            case PlayerState.Dead:
                //Skip turn
                Game.Instance.NextTurn();
                break;
            case PlayerState.StartingTurn:
                StartCoroutine(StartTurn());
                curState = PlayerState.FirstMove;
                break;
            case PlayerState.FirstMove:
                PresentMovementOptions();
                break;
            case PlayerState.SecondMove:
                PresentMovementOptions();
                break;
            case PlayerState.ThirdMove:
                PresentMovementOptions();
                break;
            case PlayerState.EndingTurn:
                Debug.Log("Ending turn...");
                //StartCoroutine(EndTurn(clickedTile));
                break;
        }
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


    */

    public void Die()
    {
        curState = PlayerState.Dead;
        Debug.Log("THIS KNIFER IS DEAD");
        gameObject.SetActive(false);

    }
}
