using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
                StartCoroutine(MoveToTile(clickedTile));
                
                break;
            case PlayerAction.Watch: // Player clicked 
                curState = PlayerState.EndingTurn;
                StartCoroutine(Watch());
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
    
    IEnumerator Watch()
    {

        ShowExtendedCone();
        yield return new WaitForSeconds(3);
        /*
        foreach(Player player in Game.Instance.players)
        {
            //This creates ray through mouse position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(transform.position,);
            if (hit)
            {
                if (hit.collider.gameObject.GetComponent<Knifer>())
                {
                    Knifer knifer = hit.collider.gameObject.GetComponent<Knifer>();
                    knifer.Die();
                }
            }
        }
        */
        HideExtendedCone();

        this.ExecuteState();
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

    void ShowExtendedCone()
    {
        List<Vector2> extendedConeHull = GetExtendedCone();
        extendedConeHull.Add(extendedConeHull[0]);
        Vector3[] vertices = new Vector3[extendedConeHull.Count];
        for(int i = 0; i < extendedConeHull.Count; i++)
        {
            Vector2 vert2d = extendedConeHull[i];
            vertices[i] = new Vector3(vert2d.x, vert2d.y, -0.2f);
        }

        GameObject playerViewGO = new GameObject();
        this.playerView = playerViewGO.AddComponent<LineRenderer>();
        this.playerView.material = new Material(Shader.Find("Sprites/Default"));
        this.playerView.widthMultiplier = 0.1f;
        this.playerView.positionCount = extendedConeHull.Count;
        this.playerView.SetPositions(vertices);

    }

    void HideExtendedCone()
    {
        playerView.gameObject.SetActive(false);
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

    /*
    public override IEnumerator PresentMovementOptions()
    {

        Tile curTile = board.GetTileFromID(this.currentTileIdx);
        Game.Instance.tileButtons = board.GetAdjacentTilesFromID(this.currentTileIdx);
        foreach (Tile neighborTile in Game.Instance.tileButtons)
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
    */

}
