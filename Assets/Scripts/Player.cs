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
    Dead
}

public enum PlayerAction
{
    Watch,
    MoveToTile,
    Skip
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

    public virtual void ProcessAction(PlayerAction action)
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
        transform.position = startingTile.center;
        transform.up = Vector2.up;
    }

    /*
    public void RotatePlayerToDir(Vector2 dir, System.Action callback)
    {
        StartCoroutine(FaceDir(), callback);
    }

    IEnumerator FaceDir()
    {
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.back);
            transform.rotation = Quaternion.Slerp(origRotation, targetRotation, Time.deltaTime);

            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    */

    
    public IEnumerator MoveToTile(Tile tile)
    {
        Vector2 origPos = transform.position;
        Vector3 origCamPos = Camera.main.transform.position;
        Vector2 targetPos = tile.center;
        Vector3 targetCamPos = new Vector3(tile.center.x, tile.center.y, Camera.main.transform.position.z);
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {

            transform.position = Vector2.Lerp(origPos, targetPos, t);
            Camera.main.transform.position = Vector3.Lerp(origCamPos, targetCamPos, t);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        Camera.main.transform.position = targetCamPos;
        this.ExecuteState();
    }

}
