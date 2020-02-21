using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Idle,
    FirstMove,
    SecondMove,
    ThirdMove,
    EndingTurn,
    Dead,
    Rotating
}

public enum PlayerAction
{
    Watch,
    MoveToTile,
    Skip,
    Rotate,
    Die
}


public class Player : MonoBehaviour
{
    public string playerName;
    public Board board;
    public int currentTileIdx;
    public int movesLeft;
    public LineRenderer playerView;
    public PlayerState curState;

    // Start is called before the first frame update
    void Start()
    {
        curState = PlayerState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void ProcessAction(PlayerAction action, Dictionary<string, object> data = null)
    {

    }

    public virtual void ExecuteState()
    {

    }

    /*
     *  Setup starting position and rotation for each player 
     * 
     */
    public void Setup()
    {
        Tile startingTile = board.GetTileFromID(currentTileIdx);
        transform.position = new Vector3(startingTile.center.x, startingTile.center.y, transform.position.z);
        transform.up = Vector2.up;
    }

    public IEnumerator FaceDir(Vector2 targetDir)
    {
        Quaternion origRotation = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(transform.forward, new Vector3(targetDir.x, targetDir.y, 0.0f));

        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            transform.rotation = Quaternion.Slerp(origRotation, targetRot, t / totalT);

            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;

        this.ExecuteState();
    }
    
    public IEnumerator MoveToTile(Tile tile)
    {
        Vector3 origPos = transform.position;
        Vector3 origCamPos = Camera.main.transform.position;
        Vector3 targetPos = new Vector3(tile.center.x, tile.center.y, transform.position.z);
        Vector3 targetCamPos = new Vector3(tile.center.x, tile.center.y, Camera.main.transform.position.z);
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {

            transform.position = Vector3.Lerp(origPos, targetPos, t);
            Camera.main.transform.position = Vector3.Lerp(origCamPos, targetCamPos, t / totalT);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        Camera.main.transform.position = targetCamPos;
        this.ExecuteState();
    }

    public void HideKnifers()
    {
        foreach (Knifer knifer in Game.Instance.knifers)
        {
            knifer.GetComponent<Renderer>().enabled = false;
        }
    }

    public void ShowKnifers()
    {
        foreach (Knifer knifer in Game.Instance.knifers)
        {
            if (knifer.curState != PlayerState.Dead)
            {
                knifer.GetComponent<Renderer>().enabled = true;
            }

        }
    }

}
