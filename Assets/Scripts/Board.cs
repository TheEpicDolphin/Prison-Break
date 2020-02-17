using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Edge
{
    public Vector2 p1;
    public Vector2 p2;

    public Edge(Vector2 p1, Vector2 p2)
    {
        this.p1 = p1;
        this.p2 = p2;
    }


    /*
     *  Intersects edge with line
     * 
     */
    public Vector2 Intersect(Vector2 origin, Vector2 dir)
    {
        Vector2 n1 = Vector2.Perpendicular(p2 - p1);
        Vector2 n2 = Vector2.Perpendicular(dir);
        float D = n2.x * n1.y - n2.y * n1.x;
        float Dx = Vector2.Dot(n2, origin) * n1.y - n2.y * Vector2.Dot(n1, this.p1);
        float Dy = n2.x * Vector2.Dot(n1, this.p1) - Vector2.Dot(n2, origin) * n1.x;

        Vector2 intersection = new Vector2(Dx / D, Dy / D);
        return intersection;
    }
}

public class AngleRange
{
    public Edge edge;
    public float t1;
    public float t2;

    public AngleRange(Edge edge, float t1, float t2)
    {
        this.edge = edge;
        this.t1 = t1;
        this.t2 = t2;
    }
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

    /*
     * Only works of angle between leftFunnel and rightFunnel < 180.0f
     * 
     */
    List<AngleRange> SortedWallAngleRanges(Vector2 currentPos, Vector2 leftFunnel, Vector2 rightFunnel)
    {
        List<AngleRange> walls = new List<AngleRange>();
        foreach(Tile tile in this.tiles)
        {
            for(int i = 0; i < tile.walls.Count; i++)
            {
                Edge unorientedEdge = tile.walls[i];
                Vector2 v1 = unorientedEdge.p1 - currentPos;
                Vector2 v2 = unorientedEdge.p2 - currentPos;
                float theta1 = Vector2.SignedAngle(v1, rightFunnel);
                float theta2 = Vector2.SignedAngle(v2, rightFunnel);
                float totalTheta = Vector2.SignedAngle(leftFunnel, rightFunnel);

                Edge orientedEdge;
                //Orient edges so that they go counterclockwise around player
                if (theta1 > theta2)
                {
                    orientedEdge = new Edge(v2, v1);
                }
                else
                {
                    orientedEdge = new Edge(v1, v2);
                }

                if(theta1 < totalTheta && theta2 > 0.0f)
                {
                    walls.Add(new AngleRange(orientedEdge, Mathf.Max(theta1, 0.0f), Mathf.Min(theta2, totalTheta)));
                }
                
            } 
        }

        //Sort by start angle for each angle range
        return walls.OrderBy(angleRange => angleRange.t1).ToList();
    }

    List<Vector2> ConstructPlayerView(Vector2 currentPos, Vector2 leftFunnel, Vector3 rightFunnel)
    {
        

        List<AngleRange> wallAngleRanges = SortedWallAngleRanges(currentPos, leftFunnel, rightFunnel);
        List<AngleRange> mergedWallAngleRanges = new List<AngleRange>();
        

        for(int i = 0; i < wallAngleRanges.Count; i++)
        {
            List<AngleRange> tempAngleRanges = new List<AngleRange>();
            AngleRange curAngleRange = wallAngleRanges[i];
            AngleRange mergedAngleRange = curAngleRange;
            for(int j = i + 1; j < wallAngleRanges.Count; j++)
            {
                if((curAngleRange.edge.p1 - currentPos).magnitude > (wallAngleRanges[j].edge.p1 - currentPos).magnitude)
                {
                    mergedAngleRange = new AngleRange(curAngleRange.edge, curAngleRange.t1, wallAngleRanges[j].t1);
                    tempAngleRanges.Add(wallAngleRanges[j]);
                }
                else if (curAngleRange.t2 < wallAngleRanges[j].t1)
                {
                    tempAngleRanges.Add(wallAngleRanges[j]);
                }
                else if (curAngleRange.t2 > wallAngleRanges[j].t1 && curAngleRange.t2 < wallAngleRanges[j].t2)
                {
                    tempAngleRanges.Add(new AngleRange(wallAngleRanges[j].edge, curAngleRange.t2, wallAngleRanges[j].t2));
                }
                else if(curAngleRange.t2 > wallAngleRanges[j].t2)
                {
                    //Effectively ignore this angle range
                    continue;
                }
            }
            wallAngleRanges = tempAngleRanges;
            mergedWallAngleRanges.Add(mergedAngleRange);
        }




        List<Vector2> hull = new List<Vector2>();
        hull.Add(currentPos);

        for(int i = 0; i < mergedWallAngleRanges.Count; i++)
        {
            AngleRange angleRange = mergedWallAngleRanges[i];
            Vector2 dir1 = (Quaternion.AngleAxis(angleRange.t1, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            hull.Add(angleRange.edge.Intersect(currentPos, dir1));

            Vector2 dir2 = (Quaternion.AngleAxis(angleRange.t2, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            hull.Add(angleRange.edge.Intersect(currentPos, dir2));
        }
        return hull;
    }



}
