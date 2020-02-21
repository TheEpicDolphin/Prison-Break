using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gunner : Player
{

    // Start is called before the first frame update
    void Start()
    {
        gameObject.layer = 2;
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
                    curState = PlayerState.Rotating;
                }

                Tile clickedTile = board.GetTileFromID(currentTileIdx);

                foreach (Knifer knifer in Game.Instance.knifers)
                {
                    if (clickedTile.id == knifer.currentTileIdx)
                    {
                        ProcessAction(PlayerAction.Die);
                    }
                }
                Game.Instance.CheckIfAllGunnersDead();
                StartCoroutine(MoveToTile(clickedTile));
                
                break;
            case PlayerAction.Watch: // Player clicked 
                curState = PlayerState.Rotating;
                StartCoroutine(Watch());
                break;
            case PlayerAction.Skip:
                curState = PlayerState.Rotating;
                ExecuteState();
                break;
            case PlayerAction.Rotate:
                Vector2 dir = (Vector2) data["dir"];
                curState = PlayerState.EndingTurn;
                StartCoroutine(FaceDir(dir));
                break;
            case PlayerAction.Die:
                curState = PlayerState.Dead;
                StartCoroutine(Die());
                break;
        }
    }

    public override void ExecuteState()
    {
        switch (curState)
        {
            case PlayerState.Idle:
                curState = PlayerState.FirstMove;
                HideKnifers();
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
            case PlayerState.Dead:
                Game.Instance.NextTurn();
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
        Game.Instance.stayButton.GetComponent<Button>().interactable = true;
    }

    void PresentRotatingOptions()
    {
        Game.Instance.rightButton.GetComponent<Button>().interactable = true;
        Game.Instance.upButton.GetComponent<Button>().interactable = true;
        Game.Instance.leftButton.GetComponent<Button>().interactable = true;
        Game.Instance.downButton.GetComponent<Button>().interactable = true;
    }

    IEnumerator Watch()
    {

        ShowExtendedCone();
        yield return new WaitForSeconds(2);

        //Reveal knifers. These knifers will die
        foreach(Knifer knifer in Game.Instance.knifers)
        {
            Vector2 origin = new Vector2(transform.position.x, transform.position.y);
            Vector2 dir = new Vector2(knifer.transform.position.x, knifer.transform.position.y) - origin;
            Vector3 leftFunnel = (Quaternion.AngleAxis(45.0f, new Vector3(0, 0, 1)) * transform.up).normalized;
            Vector3 rightFunnel = (Quaternion.AngleAxis(-45.0f, new Vector3(0, 0, 1)) * transform.up).normalized;

            float theta1 = Vector3.SignedAngle(new Vector3(dir.x, dir.y, 0), rightFunnel, Vector3.back);
            float theta2 = Vector3.SignedAngle(leftFunnel, new Vector3(dir.x, dir.y, 0), Vector3.back);
            Debug.Log(theta1.ToString() + ", " + theta2.ToString());
            if (theta1 >= 0.0f && theta2 >= 0.0f)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir);
                Debug.DrawRay(origin, dir, Color.red, 2.0f);
                if (hit)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    Debug.Log(knifer.gameObject.name);

                    //Dont show knifer if it is already dead
                    if (knifer.curState != PlayerState.Dead && hit.collider.gameObject.GetInstanceID() == knifer.gameObject.GetInstanceID())
                    {
                        knifer.GetComponent<Renderer>().enabled = true;
                        knifer.ProcessAction(PlayerAction.Die);
                    }
                }
            }
            
        }

        yield return new WaitForSeconds(3);

        HideExtendedCone();
        Game.Instance.CheckIfAllKnifersDead();

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
        this.playerView.material.color = Color.yellow;
        this.playerView.widthMultiplier = 0.05f;
        this.playerView.positionCount = extendedConeHull.Count;
        this.playerView.SetPositions(vertices);

    }

    void HideExtendedCone()
    {
        playerView.gameObject.SetActive(false);
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
            transform.localScale = Vector3.Lerp(origScale, targetScale, t / totalT);
            yield return null;
        }
        gameObject.GetComponent<Renderer>().enabled = false;
    }
}
