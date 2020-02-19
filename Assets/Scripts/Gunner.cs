﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum GunnerAction { Move, Watch, Stay };
enum FacingDirection { Right, Up, Left, Down };

public class Gunner : Player
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override IEnumerator StartTurn()
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

        StartCoroutine(PresentMovementOptions());

        /*
        int numMoves = 0;
        while(numMoves < 2)
        {
            waitFlags = 0b0000;
            //Provide player with movement options
            GunnerAction gunnerAction;
            PresentMovementOptions(numMoves, currentTileIdx, () =>
            {
                waitFlags |= 0b0001;
            });

            
            yield return new WaitUntil(() => waitFlags == 0b0011);

            
            //Perform action animation
            waitFlags = 0b0000;
            if(gunnerAction == GunnerAction.Move)
            {
                //Play moving animation
                MovePlayerToTile(this.currentTileIdx, () =>
                {
                    waitFlags |= 0b0001;
                });
                yield return new WaitUntil(() => waitFlags == 0b0001);
            }
            else if(gunnerAction == GunnerAction.Watch)
            {

                ShowExtendedCone(() =>
                {
                    waitFlags |= 0b0001;
                });
                yield return new WaitUntil(() => waitFlags == 0b0001);

                List<Knifer> knifersKilled = ScanForKnifers();
                foreach (Knifer knifer in knifersKilled)
                {
                    //Play killing animation for each
                    waitFlags = 0b0000;
                    ShootKnifer(() =>
                    {
                        waitFlags |= 0b0001;
                    });
                    yield return new WaitUntil(() => waitFlags == 0b0001);
                }

                waitFlags = 0b0000;
                HideExtendedCone(() =>
                {
                    waitFlags |= 0b0001;
                });
                yield return new WaitUntil(() => waitFlags == 0b0001);


                if (game.knifersLeft == 0)
                {
                    //Game over. Gunner won!
                    yield break;
                }
                break;
            }
            else if(gunnerAction == GunnerAction.Stay)
            {
                break;
            }
            

            numMoves += 1;
        }
        */
    }


    List<Vector2> GetExtendedCone()
    {
        //Construct player view mesh depending on choice
        Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 leftFunnel = (Quaternion.AngleAxis(45.0f, new Vector3(0, 0, 1)) * transform.up).normalized;
        Vector2 rightFunnel = (Quaternion.AngleAxis(-45.0f, new Vector3(0, 0, 1)) * transform.up).normalized;
        Debug.DrawRay(transform.position, 15.0f * leftFunnel, Color.green);
        Debug.DrawRay(transform.position, 15.0f * rightFunnel, Color.red);
        Debug.DrawRay(transform.position, 15.0f * Vector2.right, Color.yellow);
        return board.ConstructPlayerViewHull(playerPos2D, leftFunnel, rightFunnel);
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

    /*
    IEnumerator ShowExtendedCone(System.Action callback)
    {

    }

    IEnumerator HideExtendedCone(System.Action callback)
    {

    }
    

    List<Knifer> ScanForKnifers()
    {
        return new List<Knifer>();
    }

    void ShootKnifer(System.Action callback)
    {

    }

    */


    public override IEnumerator PresentMovementOptions()
    {

        Tile curTile = board.GetTileFromID(this.currentTileIdx);
        List<Tile> adjacentTiles = board.GetAdjacentTilesFromID(this.currentTileIdx);
        foreach (Tile neighborTile in adjacentTiles)
        {
            neighborTile.gameObject.layer = 0;
            neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        while (true)
        {
            //This creates ray through mouse position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit)
            {
                //Highlight tile
                Button button = hit.collider.gameObject.GetComponent<Button>();

                if (Input.GetMouseButton(0))
                {
                    //Reset tiles
                    foreach (Tile neighborTile in adjacentTiles)
                    {
                        neighborTile.gameObject.layer = 2;
                        neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                    }
                    button.Click(this);
                    yield break;
                }

            }
            yield return null;

        }
    }

    
    public IEnumerator Watch(Tile tile)
    {

        ShowExtendedCone();
        yield return new WaitForSeconds(3);

        /*
        List<Knifer> knifersKilled = ScanForKnifers();
        foreach (Knifer knifer in knifersKilled)
        {
            //Play killing animation for each
            waitFlags = 0b0000;
            ShootKnifer(() =>
            {
                waitFlags |= 0b0001;
            });
            yield return new WaitUntil(() => waitFlags == 0b0001);
        }

        waitFlags = 0b0000;
        */

        HideExtendedCone();

        //StartCoroutine(endTurn);
    }

    void ShowExtendedCone()
    {
        List<Vector2> extendedConeHull = GetExtendedCone();
        foreach(Vector2 pt in extendedConeHull)
        {
            //Draw;
        }

    }

    void HideExtendedCone()
    {

    }

    /*
    public IEnumerator EndTurn()
    {
        //Provide player with rotation options
        waitFlags = 0b0000;
        FacingDirection facingDir = board.PresentRotationOptions(numMoves, () =>
        {
            waitFlags |= 0b0001;
        });
        yield return new WaitUntil(() => waitFlags == 0b0001);

        Vector2 faceDir;
        if (facingDir == FacingDirection.Right)
        {

        }
        else if (facingDir == FacingDirection.Up)
        {

        }
        else if (facingDir == FacingDirection.Left)
        {

        }
        else
        {

        }

        //Play rotating animation
        waitFlags = 0b0000;
        RotatePlayerToDir(faceDir, () =>
        {
            waitFlags |= 0b0001;
        });
        yield return new WaitUntil(() => waitFlags == 0b0001);



        //Temporarily shows players in immediate cone in front of player
        waitFlags = 0b0000;
        UpdatePlayerView(false);
        StartCoroutine(PulseImmediateCone(() =>
        {
            waitFlags |= 0b0001;
        }));
        yield return new WaitUntil(() => waitFlags == 0b0001);

        //Show player end transition
        
        //waitFlags = 0b0000;
        //TransitionHandler.ShowEndTransition(this.playerName, () =>
        //{
        //    waitFlags |= 0b0001;
        //});
        //yield return new WaitUntil(() => waitFlags == 0b0001);
        
    }
    */

}