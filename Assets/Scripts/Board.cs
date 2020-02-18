using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;


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
    public Vector2 center;
    public List<Edge> walls;
    public int id;
    public Tile(int id, Vector2 center, List<Edge> walls)
    {
        this.id = id;
        this.center = center;
        this.walls = walls;
    }
}

public class Board : MonoBehaviour
{
    int numRows = 5;
    int numCols = 5;
    List<int>[] adjList;
    Tile[] tiles;
    GameObject[] tileGOs;
    List<GameObject> wallsGOs;
    float tileWidth = 1.0f;
    float tileHeight = 1.0f;
    float wallWidth = 0.1f;
    float wallLength = 1.0f;

    public GameObject player;
    int curPlayerTileID;
    GameObject playerView;

    bool wait;
    // Start is called before the first frame update
    void Start()
    {
        playerView = new GameObject();
        playerView.transform.parent = player.transform;
        playerView.transform.localPosition = Vector2.zero;
        playerView.transform.localRotation = Quaternion.identity;
        playerView.AddComponent<MeshRenderer>();
        playerView.GetComponent<MeshRenderer>().material.color = Color.green;
        this.playerView.AddComponent<MeshFilter>();

        adjList = new List<int>[numRows * numCols];
        for(int i = 0; i < adjList.Length; i++)
        {
            adjList[i] = new List<int>();
        }
        tiles = new Tile[numRows * numCols];
        tileGOs = new GameObject[numRows * numCols];
        wallsGOs = new List<GameObject>();

        ParseBoardASCIIArt("board_ascii.txt");
        CreateBoard();

        player.transform.position = tiles[0].center;
        curPlayerTileID = 0;
        wait = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerPos2D = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 leftFunnel = (Quaternion.AngleAxis(45.0f, new Vector3(0, 0, 1)) * player.transform.up).normalized;
        Vector2 rightFunnel = (Quaternion.AngleAxis(-45.0f, new Vector3(0, 0, 1)) * player.transform.up).normalized;
        Debug.DrawRay(player.transform.position, 15.0f * leftFunnel, Color.green);
        Debug.DrawRay(player.transform.position, 15.0f * rightFunnel, Color.red);
        Debug.DrawRay(player.transform.position, 15.0f * Vector2.right, Color.yellow);
        ConstructPlayerView(playerPos2D, leftFunnel, rightFunnel);


        List<int> neighbors = GetAdjacentTileIDs(curPlayerTileID);
        if (Input.GetKey(KeyCode.D) && this.wait == false)
        {
            this.wait = true;
            MovePlayerToTile(neighbors[0], () => 
            {
                curPlayerTileID = neighbors[0];
                this.wait = false;
            });
        }
        /*
        else if (Input.GetKey(KeyCode.W))
        {

        }
        else if (Input.GetKey(KeyCode.A))
        {

        }
        else if(Input.GetKey(KeyCode.S))
        {

        }
        */

    }


    void ParseBoardASCIIArt(string fileName)
    {
        var sr = new StreamReader(Application.dataPath + "/" + fileName);
        var fileContents = sr.ReadToEnd();
        sr.Close();

        string[] lines = fileContents.Split("\n"[0]);
        for (int i = 0; i < lines.Length; i++)
        {
            string row = lines[numRows - 1 - i];
            for(int c = 0; c < row.Length - 2; c+=2)
            {
                int j = c / 2;
                Vector2 tileCenter = new Vector2(j * tileWidth + tileWidth/2, i * tileHeight + tileHeight / 2);
                List<Edge> walls = new List<Edge>();
                if(row[c] == '|')
                {
                    walls.Add(new Edge(new Vector2(j * tileWidth, i * tileHeight), new Vector2(j * tileWidth, (i + 1) * tileHeight)));
                }
                else
                {
                    if(j > 0)
                    {
                        adjList[j + i * numCols].Add(j - 1 + i * numCols);
                        adjList[j - 1 + i * numCols].Add(j + i * numCols);
                    }
                }
                if (row[c + 1] == '_')
                {
                    walls.Add(new Edge(new Vector2(j * tileWidth, i * tileHeight), new Vector2((j + 1) * tileWidth, i * tileHeight)));
                }
                else
                {
                    if (i < numRows - 1)
                    {
                        adjList[j + i % numCols].Add(j + (i + 1) % numCols);
                        adjList[j + (i + 1) % numCols].Add(j + i % numCols);
                    }
                }

                if(i == numRows - 1)
                {
                    walls.Add(new Edge(new Vector2(j * tileWidth, (i + 1) * tileHeight), new Vector2((j + 1) * tileWidth, (i + 1) * tileHeight)));
                }

                if (j == numCols - 1)
                {
                    walls.Add(new Edge(new Vector2((j + 1) * tileWidth, i * tileHeight), new Vector2((j + 1) * tileWidth, (i + 1) * tileHeight)));
                }
                this.tiles[j + i * numCols] = new Tile(j + i * numCols, tileCenter, walls);
            }
        }

    }

    private Mesh CreateTileMesh()
    {
        Vector2[] vertices2D = new Vector2[] {
            new Vector2(-tileWidth/2, -tileHeight/2),
            new Vector2(tileWidth/2, -tileHeight/2),
            new Vector2(tileWidth/2, tileHeight/2),
            new Vector2(-tileWidth/2, tileHeight/2)
        };

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh CreateWallMesh(Vector3 dir)
    {
        Vector2[] vertices2D = new Vector2[] {
            new Vector2(-wallWidth/2, -wallLength/2),
            new Vector2(wallWidth/2, -wallLength/2),
            new Vector2(wallWidth/2, wallLength/2),
            new Vector2(-wallWidth/2, wallLength/2)
        };

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh CreateViewMesh(Vector2[] vertices2D)
    {
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void CreateBoard()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            Tile tile = this.tiles[i];
            //Create gameobjects here
            tileGOs[i] = new GameObject();
            tileGOs[i].transform.position = tile.center;
            // Set up game object with mesh;
            tileGOs[i].AddComponent<MeshRenderer>();
            tileGOs[i].GetComponent<MeshRenderer>().material.color = Color.white;
            MeshFilter tileMeshFilter = tileGOs[i].AddComponent<MeshFilter>();
            tileMeshFilter.mesh = CreateTileMesh();

            for(int j = 0; j < tile.walls.Count; j++)
            {
                GameObject wallGO = new GameObject();
                Vector2 wallCenter2D = (tile.walls[j].p2 + tile.walls[j].p1) / 2;
                wallGO.transform.position = new Vector3(wallCenter2D.x, wallCenter2D.y, -0.5f);
                Vector2 edgeDir = (tile.walls[j].p2 - tile.walls[j].p1).normalized;
                //transform.forward points in -z direction (into screen)
                //transform.up points in +y direction
                wallGO.transform.up = edgeDir;
                // Set up game object with mesh;
                wallGO.AddComponent<MeshRenderer>();
                wallGO.GetComponent<MeshRenderer>().material.color = Color.black;
                MeshFilter wallMeshFilter = wallGO.AddComponent<MeshFilter>();
                wallMeshFilter.mesh = CreateWallMesh(edgeDir);
                wallsGOs.Add(wallGO);
            }
            
        }
    }

    void MovePlayerToTile(int targetTileID, System.Action callback)
    {
        StartCoroutine(MoveToTile(player.transform, this.tiles[targetTileID].center, callback));
    }

    IEnumerator MoveToTile(Transform playerTrans, Vector2 targetPos, System.Action callback)
    {
        Vector2 origPos = playerTrans.position;
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            
            playerTrans.position = Vector2.Lerp(origPos, targetPos, t);
            t += Time.deltaTime;
            yield return null;
        }
        playerTrans.position = targetPos;
        callback();
    }

    /*
    void RotatePlayerToDir(Vector2 dir)
    {
        StartCoroutine(FaceDir());
    }

    IEnumerator FaceDir()
    {
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.back);
            player.transform.rotation = Quaternion.Slerp(origRotation, targetRotation, Time.deltaTime);

            t += Time.deltaTime;
            yield return null;
        }
        player.transform.rotation = targetRotation;
    }
    */
    

    List<int> GetAdjacentTileIDs(int currentTileID)
    {
        List<int> neighborIDs = adjList[currentTileID];
        return neighborIDs;
    }

    /*
     * Only works of angle between leftFunnel and rightFunnel < 180.0f
     * 
     */
    List<AngleRange> SortedWallAngleRanges(Vector2 currentPos, Vector2 leftFunnel, Vector2 rightFunnel)
    {
        List<AngleRange> walls = new List<AngleRange>();
        foreach (Tile tile in this.tiles)
        {
            for (int i = 0; i < tile.walls.Count; i++)
            {
                Edge unorientedEdge = tile.walls[i];
                Vector2 v1 = unorientedEdge.p1 - currentPos;
                Vector2 v2 = unorientedEdge.p2 - currentPos;
                float theta1 = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(rightFunnel.x, rightFunnel.y, 0), Vector3.back);
                float theta2 = Vector3.SignedAngle(new Vector3(v2.x, v2.y, 0), new Vector3(rightFunnel.x, rightFunnel.y, 0), Vector3.back);
                float totalTheta = Vector3.SignedAngle(new Vector3(leftFunnel.x, leftFunnel.y, 0), new Vector3(rightFunnel.x, rightFunnel.y, 0), Vector3.back);
                //Debug.Log("----------------");
                //Debug.Log(theta1);
                //Debug.Log(theta2);
                //Debug.Log(totalTheta);
                Edge orientedEdge;
                //Orient edges so that they go counterclockwise around player
                if (theta1 > theta2)
                {
                    orientedEdge = new Edge(unorientedEdge.p2, unorientedEdge.p1);
                }
                else
                {
                    orientedEdge = new Edge(unorientedEdge.p1, unorientedEdge.p2);
                }

                v1 = orientedEdge.p1 - currentPos;
                v2 = orientedEdge.p2 - currentPos;
                theta1 = Vector3.SignedAngle(new Vector3(v1.x, v1.y, 0), new Vector3(rightFunnel.x, rightFunnel.y, 0), Vector3.back);
                theta2 = Vector3.SignedAngle(new Vector3(v2.x, v2.y, 0), new Vector3(rightFunnel.x, rightFunnel.y, 0), Vector3.back);
                if (theta1 < totalTheta && theta2 > 0.0f && (theta2 - theta1) < 180.0f)
                {
                    walls.Add(new AngleRange(orientedEdge, Mathf.Max(theta1, 0.0f), Mathf.Min(theta2, totalTheta)));
                }

            }
        }

        //Sort by start angle for each angle range
        return walls.OrderBy(angleRange => ((angleRange.edge.p1 + angleRange.edge.p2)/2 - currentPos).magnitude).ToList();
    }

    void ConstructPlayerView(Vector2 currentPos, Vector2 leftFunnel, Vector3 rightFunnel)
    {
        List<AngleRange> wallAngleRanges = SortedWallAngleRanges(currentPos, leftFunnel, rightFunnel);
        List<AngleRange> mergedWallAngleRanges = new List<AngleRange>();
        Debug.Log("------------------");
        Debug.Log(wallAngleRanges.Count);

        /*
        for (int i = 0; i < wallAngleRanges.Count; i++)
        {
            AngleRange angleRange = wallAngleRanges[i];
            Debug.DrawLine(new Vector3(angleRange.edge.p1.x, angleRange.edge.p1.y, -1.0f), new Vector3(angleRange.edge.p2.x, angleRange.edge.p2.y, -1.0f), Color.cyan);
        }
        */

        while (wallAngleRanges.Count > 0)
        {
            List<AngleRange> tempAngleRanges = new List<AngleRange>();
            AngleRange curAngleRange = wallAngleRanges[0];

            for (int j = 1; j < wallAngleRanges.Count; j++)
            {
                if (curAngleRange.t1 < wallAngleRanges[j].t1 && curAngleRange.t2 > wallAngleRanges[j].t1 && curAngleRange.t2 < wallAngleRanges[j].t2)
                {
                    tempAngleRanges.Add(new AngleRange(wallAngleRanges[j].edge, curAngleRange.t2, wallAngleRanges[j].t2));
                }
                else if (curAngleRange.t1 > wallAngleRanges[j].t1 && curAngleRange.t1 < wallAngleRanges[j].t2 && curAngleRange.t2 > wallAngleRanges[j].t2)
                {
                    tempAngleRanges.Add(new AngleRange(wallAngleRanges[j].edge, wallAngleRanges[j].t1, curAngleRange.t1));
                }
                else if (curAngleRange.t2 >= wallAngleRanges[j].t2 && curAngleRange.t1 <= wallAngleRanges[j].t1)
                {
                    //Effectively ignore this angle range
                    continue;
                }
                else
                {
                    tempAngleRanges.Add(wallAngleRanges[j]);
                }
            }
            wallAngleRanges = tempAngleRanges;
            mergedWallAngleRanges.Add(curAngleRange);
        }

        mergedWallAngleRanges = mergedWallAngleRanges.OrderBy(angleRange => angleRange.t1).ToList();
        Vector2 last = currentPos;
        for (int i = 0; i < mergedWallAngleRanges.Count; i++)
        {
            AngleRange angleRange = mergedWallAngleRanges[i];
            Debug.Log(angleRange.t1.ToString() + ", " + angleRange.t2.ToString());
            //Debug.DrawLine(new Vector3(angleRange.edge.p1.x, angleRange.edge.p1.y, -1.0f), new Vector3(angleRange.edge.p2.x, angleRange.edge.p2.y, -1.0f), Color.cyan);

            Vector2 dir1 = (Quaternion.AngleAxis(angleRange.t1, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            Vector2 s1 = angleRange.edge.Intersect(currentPos, dir1);
            Debug.DrawLine(new Vector3(last.x, last.y, -0.5f), new Vector3(s1.x, s1.y, -0.5f), Color.magenta);

            Vector2 dir2 = (Quaternion.AngleAxis(angleRange.t2, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            Vector2 s2 = angleRange.edge.Intersect(currentPos, dir2);
            Debug.DrawLine(new Vector3(s1.x, s1.y, -0.5f), new Vector3(s2.x, s2.y, -0.5f), Color.magenta);
            last = s2;
        }
        Debug.DrawLine(new Vector3(last.x, last.y, -0.5f), new Vector3(currentPos.x, currentPos.y, -0.5f), Color.magenta);


        /*
        List<Vector2> hull = new List<Vector2>();
        hull.Add(Vector2.zero);

        for (int i = 0; i < mergedWallAngleRanges.Count; i++)
        {
            AngleRange angleRange = mergedWallAngleRanges[i];
            Vector2 dir1 = (Quaternion.AngleAxis(angleRange.t1, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            //hull.Add(angleRange.edge.Intersect(currentPos, dir1) - currentPos);
            hull.Add(playerView.transform.InverseTransformPoint(angleRange.edge.Intersect(currentPos, dir1)));

            Vector2 dir2 = (Quaternion.AngleAxis(angleRange.t2, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            //hull.Add(angleRange.edge.Intersect(currentPos, dir2) - currentPos);
            hull.Add(playerView.transform.InverseTransformPoint(angleRange.edge.Intersect(currentPos, dir2)));
        }

        //Set view mesh
        playerView.GetComponent<MeshFilter>().mesh = CreateViewMesh(hull.ToArray());
        */

    }


}
*/