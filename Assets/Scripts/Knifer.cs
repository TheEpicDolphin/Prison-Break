using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Knifer : Player
{

    // Start is called before the first frame update
    new void Start()
    {
        transform.up = Vector2.up;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ProcessAction(PlayerAction action, Dictionary<string, object> data = null)
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
                    curState = PlayerState.ThirdMove;
                }
                else if (curState == PlayerState.ThirdMove)
                {
                    curState = PlayerState.EndingTurn;
                }

                Tile clickedTile = board.GetTileFromID(currentTileIdx);
                
                foreach (Gunner gunner in Game.Instance.gunners) 
                {
                    if (clickedTile.id == gunner.currentTileIdx) 
                    {
                        gunner.ProcessAction(PlayerAction.Die);
                    }
				}

                if (Game.Instance.CheckIfAllGunnersDead())
                {
                    curState = PlayerState.Inactive;
                }
                StartCoroutine(MoveToTile(clickedTile));
                break;

            case PlayerAction.Die:
                curState = PlayerState.Dead;
                StartCoroutine(Die());
                break;

            case PlayerAction.Skip:
                curState = PlayerState.EndingTurn;
                ExecuteState();
                break;

            case PlayerAction.Rotate:
                Vector2 dir = (Vector2)data["dir"];
                curState = PlayerState.EndingTurn;
                StartCoroutine(FaceDir(dir));
                break;
        }
    }

    public override void ExecuteState()
    {
        switch (curState)
        {
            case PlayerState.Idle:
                ShowKnifers();
                curState = PlayerState.FirstMove;
                StartCoroutine(StartTurn());
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
            case PlayerState.Rotating:
                PresentRotatingOptions();
                break;
            case PlayerState.EndingTurn:
                curState = PlayerState.Idle;
                Game.Instance.NextTurn();
                //StartCoroutine(EndTurn(clickedTile));
                break;
            case PlayerState.Dead:
                Game.Instance.NextTurn();
                break;
            case PlayerState.Inactive:
                //Do nothing
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
        Game.Instance.stayButton.GetComponent<Button>().interactable = true;
    }

    void PresentRotatingOptions()
    {
        Game.Instance.rightButton.GetComponent<Button>().interactable = true;
        Game.Instance.upButton.GetComponent<Button>().interactable = true;
        Game.Instance.leftButton.GetComponent<Button>().interactable = true;
        Game.Instance.downButton.GetComponent<Button>().interactable = true;
    }

    IEnumerator PulseImmediateCone(System.Action callback)
    {
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            t += Time.deltaTime;
            yield return null;
        }
        callback();
    }

    IEnumerator Die()
    {
        Vector3 origScale = transform.localScale;
        Vector3 targetScale = 0.01f * transform.localScale;
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(origScale, targetScale, t/totalT);
            yield return null;
        }
        gameObject.GetComponent<Renderer>().enabled = false;
    }

    
}
