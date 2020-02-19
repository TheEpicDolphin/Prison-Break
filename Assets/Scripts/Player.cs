using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Player : MonoBehaviour
{
    public string playerName;
    public Board board;
    public Game game;
    public Camera cam;
    public int currentTileIdx;
    int numMoves;
    public LineRenderer playerView;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual IEnumerator StartTurn()
    {
        yield return null;
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
        Vector3 origCamPos = cam.transform.position;
        Vector2 targetPos = tile.center;
        Vector3 targetCamPos = new Vector3(tile.center.x, tile.center.y, cam.transform.position.z);
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {

            transform.position = Vector2.Lerp(origPos, targetPos, t);
            cam.transform.position = Vector3.Lerp(origCamPos, targetCamPos, t);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        cam.transform.position = targetCamPos;

        PresentMovementOptions();
    }

    public virtual void PresentMovementOptions()
    {

    }

    /*
    public virtual IEnumerator PresentMovementOptions()
    {
        
        yield return null;
    }
    */
}
