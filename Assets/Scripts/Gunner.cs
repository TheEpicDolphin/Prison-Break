using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gunner : Player
{

    // Start is called before the first frame update
    new void Start()
    {
        gameObject.layer = 2;
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
                PresentMovementOptions();
                break;
            case PlayerState.SecondMove:
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
        yield return new WaitForSeconds(0.5f);
        float radius = 2.0f;
        ShowImmediateViewCone(radius);

        yield return new WaitForSeconds(0.5f);
        List<Knifer> nearbyKnifers = new List<Knifer>();
        //Reveal nearby knifers.
        foreach (Knifer knifer in Game.Instance.knifers)
        {
            if((knifer.transform.position - transform.position).magnitude < radius)
            {
                knifer.gameObject.GetComponent<Renderer>().enabled = true;
                nearbyKnifers.Add(knifer);
            }
        }

        yield return new WaitForSeconds(2.0f);

        //Hide knifers
        foreach(Knifer knifer in nearbyKnifers)
        {
            knifer.gameObject.GetComponent<Renderer>().enabled = false;
        }

        yield return new WaitForSeconds(0.5f);
        HideViewCone();
        yield return new WaitForSeconds(0.5f);

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

        ShowExtendedViewCone();
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
            if (theta1 >= 0.0f && theta2 >= 0.0f)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir);
                Debug.DrawRay(origin, dir, Color.red, 2.0f);
                if (hit)
                {

                    //Dont show knifer if it is already dead
                    if (knifer.curState != PlayerState.Dead && hit.collider.gameObject.GetInstanceID() == knifer.gameObject.GetInstanceID())
                    {
                        knifer.GetComponent<Renderer>().enabled = true;
                        knifer.ProcessAction(PlayerAction.Die);
                    }
                }
            }
            
        }

        yield return new WaitForSeconds(2);

        HideViewCone();
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

    void ShowExtendedViewCone()
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
        this.playerView.material = new Material(Shader.Find("Unlit/Color"));
        this.playerView.material.color = Color.yellow;
        this.playerView.widthMultiplier = 0.05f;
        this.playerView.positionCount = extendedConeHull.Count;
        this.playerView.SetPositions(vertices);

    }

    void ShowImmediateViewCone(float radius)
    {
        List<Vector3> immediateConeHull = new List<Vector3>();
        float theta = 0.0f;

        while (theta < 360.0f)
        {
            Vector3 v = transform.position + radius * (Quaternion.AngleAxis(theta, new Vector3(0, 0, 1)) * transform.up).normalized;
            immediateConeHull.Add(v);
            theta += 15.0f;
        }
        immediateConeHull.Add(immediateConeHull[0]);

        GameObject playerViewGO = new GameObject();
        this.playerView = playerViewGO.AddComponent<LineRenderer>();
        this.playerView.material = new Material(Shader.Find("Sprites/Default"));
        this.playerView.material.color = Color.cyan;
        this.playerView.widthMultiplier = 0.05f;
        this.playerView.positionCount = immediateConeHull.Count;
        this.playerView.SetPositions(immediateConeHull.ToArray());

    }

    void HideViewCone()
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
