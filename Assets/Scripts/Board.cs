using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


/* 
public struct Edge
{
    public Vector2 p1;
    public Vector2 p2;
}

public class Tile
{
    public Vector3 center;
    public List<Edge> walls;
    public Tile(Vector3 center, List<Vector2Int> walls)
    {
        this.center = center;
        this.walls = new List<Edge>();
        foreach (Vector2Int wall in walls)
        {
            this.walls.Add(new Edge(,));
        }
        
    }

    
}

public class Board : MonoBehaviour
{
    List<int>[] adjList = new List<int>[25];
    Tile[] tiles = new Tile[25];
    float tileWidth = 1.0f;
    float tileHeight = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        DrawBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DrawBoard()
    {

    }

    List<Edge> WallPointsSortedByAngle(Vector2 currentPos, Vector2 leftFunnel, Vector2 rightFunnel)
    {
        List<Edge> walls = new List<Edge>();
        foreach(Tile tile in this.tiles)
        {
            for(int i = 0; i < tile.walls.Count; i++)
            {
                Edge unorientedEdge = tile.walls[i];
                Vector2 v1 = unorientedEdge.p1 - currentPos;
                Vector2 v2 = unorientedEdge.p2 - currentPos;

                Edge orientedEdge;
                //Orient edges so that they go counterclockwise around player
                if (Vector2.SignedAngle(v1, rightFunnel) > Vector2.SignedAngle(v2, rightFunnel))
                {
                    orientedEdge = new Edge(v2, v1);
                }
                else
                {
                    orientedEdge = new Edge(v1, v2);
                }

                if(Vector2.SignedAngle(orientedEdge.v2, rightFunnel))
                {
                    walls.Add(orientedEdge);
                }
                
            } 
        }

        //Use binary search tree instead
        return walls.OrderBy(e => Vector2.SignedAngle(e.p2 - currentPos, rightFunnel)).ThenBy(e => (e.p2 - currentPos).magnitude).ToList();
    }

    List<Vector2> ConstructPlayerView(Vector2 currentPos, Vector2 leftFunnel, Vector3 rightFunnel)
    {
        List<Edge> walls = WallPointsSortedByAngle(currentPos, leftFunnel, rightFunnel);
        List<Vector2> hull = new List<Vector2>();
        hull.Add(currentPos);

        for(int i = 0; i < walls.Count; i++)
        {
            if (Vector2.SignedAngle(walls[i].p1 - currentPos, rightFunnel) >= 0.0f)
            {
                if((walls[i].p1 - hull[hull.Count - 1]).magnitude > 1e-5f)
                {
                    hull.Add(walls[i].p1);
                }
                hull.Add(walls[i].p2);

                //Find wall start point that is just less than or equal to walls[i].p2.


                rightFunnel = walls[i].p2 - currentPos;
            }
        }
        return hull;
    }



}
*/