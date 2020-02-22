using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.UI;

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

public class Tile : MonoBehaviour
{
    public Vector2 center;
    public List<Edge> walls;
    public int id;
    public bool isExit;

    public Tile(int id, Vector2 center, List<Edge> walls)
    {
        this.id = id;
        this.center = center;
        this.walls = walls;
    }

    public void OnMouseDown()
    {
        Game.Instance.currentPlayer.currentTileIdx = id;
        foreach (Tile neighborTile in Game.Instance.tileButtons)
        {
            neighborTile.gameObject.layer = 2;
            if (neighborTile.isExit)
            {
                neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = new Color(255 / 255.0f, 225 / 255.0f, 0 / 255.0f);
            }
            else
            {
                neighborTile.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
        Game.Instance.watchButton.GetComponent<Button>().interactable = false;
        Game.Instance.stayButton.GetComponent<Button>().interactable = false;
        Game.Instance.currentPlayer.ProcessAction(PlayerAction.MoveToTile);
    }

    /*
    public void OnMouseEnter()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
    }
    */
    
    
}



public class Board : MonoBehaviour
{
    int numRows = 5;
    int numCols = 5;
    List<int>[] adjList;
    Tile[] tiles;
    List<GameObject> wallsGOs;
    public Camera cam;
    float tileWidth = 10.0f;
    float tileHeight = 10.0f;
    float wallWidth = 1.0f;
    float wallLength = 10.0f;

    //public Transform tester;

    // Start is called before the first frame update
    void Start()
    {
        cam.transform.position = new Vector3(cam.orthographicSize, cam.orthographicSize, -10);
        tileWidth = 2.0f * cam.orthographicSize / numCols;
        tileHeight = 2.0f * cam.orthographicSize / numRows;
        wallWidth = 0.1f * tileWidth;
        wallLength = tileHeight;

        adjList = new List<int>[numRows * numCols];
        tiles = new Tile[numRows * numCols];
        wallsGOs = new List<GameObject>();
    }

    public void ResetBoard()
    {
        foreach(Tile tile in tiles)
        {
            if (tile)
            {
                Destroy(tile.gameObject);
            }
        }

        foreach (GameObject wallsGO in wallsGOs)
        {
            Destroy(wallsGO);
        }

        adjList = new List<int>[numRows * numCols];
        for (int i = 0; i < adjList.Length; i++)
        {
            adjList[i] = new List<int>();
        }
        tiles = new Tile[numRows * numCols];
        wallsGOs = new List<GameObject>();
        ParseBoardASCIIArt("board1.txt");
        CreateBoard();
    }
    

    // Update is called once per frame
    void Update()
    {
        //Construct player view mesh depending on choice
        
        /*
        Vector2 playerPos2D = new Vector2(tester.transform.position.x, tester.transform.position.y);
        Vector2 leftFunnel = (Quaternion.AngleAxis(45.0f, new Vector3(0, 0, 1)) * tester.transform.up).normalized;
        Vector2 rightFunnel = (Quaternion.AngleAxis(-45.0f, new Vector3(0, 0, 1)) * tester.transform.up).normalized;
        Debug.DrawRay(tester.transform.position, 15.0f * leftFunnel, Color.green);
        Debug.DrawRay(tester.transform.position, 15.0f * rightFunnel, Color.red);
        Debug.DrawRay(tester.transform.position, 15.0f * Vector2.right, Color.yellow);
        ConstructPlayerViewHull(playerPos2D, leftFunnel, rightFunnel);
        */
    }

    void ParseBoardASCIIArt(string fileName)
    {
        var sr = new StreamReader(Application.dataPath + "/" + fileName);
        var fileContents = sr.ReadToEnd();
        sr.Close();

        string[] lines = fileContents.Split("\n"[0]);
        System.Array.Reverse(lines);

        for (int i = 0; i < lines.Length; i+=3)
        {
            int r = i / 3;
            for (int j = 0; j < lines[i].Length - 2; j+=4)
            {
                int c = j / 4;
                Vector2 tileCenter = new Vector2(c * tileWidth + tileWidth / 2, r * tileHeight + tileHeight / 2);
                List<Edge> walls = new List<Edge>();
                if (lines[i][j] == '|' && lines[i + 1][j] == '|' && lines[i + 2][j] == '|')
                {
                    walls.Add(new Edge(new Vector2(c * tileWidth, r * tileHeight), new Vector2(c * tileWidth, (r + 1) * tileHeight)));
                }
                else
                {
                    if (c > 0)
                    {
                        adjList[c + r * numCols].Add(c - 1 + r * numCols);
                        adjList[c - 1 + r * numCols].Add(c + r * numCols);
                    }
                }
                if (lines[i][j + 1] == '_' && lines[i][j + 2] == '_' && lines[i][j + 3] == '_')
                {
                    walls.Add(new Edge(new Vector2(c * tileWidth, r * tileHeight), new Vector2((c + 1) * tileWidth, r * tileHeight)));
                }
                else
                {
                    if (r > 0)
                    {
                        adjList[c + r * numCols].Add(c + (r - 1) * numCols);
                        adjList[c + (r - 1) * numCols].Add(c + r * numCols);
                    }
                }

                if (r == numRows - 1)
                {
                    walls.Add(new Edge(new Vector2(c * tileWidth, (r + 1) * tileHeight), new Vector2((c + 1) * tileWidth, (r + 1) * tileHeight)));
                }

                if (c == numCols - 1)
                {
                    walls.Add(new Edge(new Vector2((c + 1) * tileWidth, r * tileHeight), new Vector2((c + 1) * tileWidth, (r + 1) * tileHeight)));
                }

                GameObject tileGO = new GameObject();
                Tile tile = tileGO.AddComponent<Tile>();
                tile.id = c + r * numCols;
                tile.center = tileCenter;
                tile.walls = walls;
                tile.isExit = false;
                this.tiles[c + r * numCols] = tile;

                if (lines[i + 1][j + 2] == 'K')
                {
                    Knifer knifer = (Knifer)Instantiate(Resources.Load("Players/knifer", typeof(Knifer)));
                    knifer.currentTileIdx = tile.id;
                    knifer.board = this;
                    Game.Instance.knifers.Add(knifer);
                    
                }
                else if (lines[i + 1][j + 2] == 'G')
                {
                    Gunner gunner = (Gunner)Instantiate(Resources.Load("Players/gunner", typeof(Gunner)));
                    gunner.currentTileIdx = tile.id;
                    gunner.board = this;
                    Game.Instance.gunners.Add(gunner);
                }
                else if (lines[i + 1][j + 2] == 'O')
                {
                    tile.isExit = true;
                }

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

    void CreateBoard()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            Tile tile = this.tiles[i];
            //Modify Tile gameObject
            tile.gameObject.transform.parent = transform;
            tile.gameObject.layer = gameObject.layer;
            tile.gameObject.transform.position = tile.center;
            // Set up game object with mesh;
            tile.gameObject.AddComponent<MeshRenderer>();
            if (tile.isExit)
            {
                tile.gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"));
                tile.gameObject.GetComponent<MeshRenderer>().material.color = new Color(255/255.0f, 225/255.0f, 0/255.0f);
            }
            else
            {
                tile.gameObject.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"));
                tile.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            
            MeshFilter tileMeshFilter = tile.gameObject.AddComponent<MeshFilter>();
            tileMeshFilter.mesh = CreateTileMesh();
            tile.gameObject.AddComponent<BoxCollider2D>();
            tile.gameObject.layer = 2;

            

            for (int j = 0; j < tile.walls.Count; j++)
            {
                GameObject wallGO = new GameObject();
                Vector2 wallCenter2D = (tile.walls[j].p2 + tile.walls[j].p1) / 2;
                wallGO.transform.parent = transform;
                wallGO.layer = 3;
                
                wallGO.transform.position = new Vector3(wallCenter2D.x, wallCenter2D.y, -0.1f);
                Vector2 edgeDir = (tile.walls[j].p2 - tile.walls[j].p1).normalized;
                //transform.forward points in -z direction (into screen)
                //transform.up points in +y direction
                wallGO.transform.up = edgeDir;
                // Set up game object with mesh;
                wallGO.AddComponent<MeshRenderer>();
                wallGO.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Color"));
                wallGO.GetComponent<MeshRenderer>().material.color = Color.black;
                MeshFilter wallMeshFilter = wallGO.AddComponent<MeshFilter>();
                wallMeshFilter.mesh = CreateWallMesh(edgeDir);
                wallGO.AddComponent<BoxCollider2D>();
                wallsGOs.Add(wallGO);
            }
            
        }
    }

    public void PanCameraToPlayerTile(int playerTileIdx, System.Action callback)
    {
        Tile tile = this.tiles[playerTileIdx];
        StartCoroutine(PanCameraToPosition(new Vector3(tile.center.x, tile.center.y, cam.transform.position.z), callback));
    }

    IEnumerator PanCameraToPosition(Vector3 targetPos, System.Action callback)
    {
        Vector3 origPos = cam.transform.position;
        float totalT = 1.0f;
        float t = 0.0f;
        while (t < totalT)
        {
            cam.transform.position = Vector3.Lerp(origPos, targetPos, t);
            t += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = targetPos;
        callback();
    }

    public Tile GetTileFromID(int currentTileID)
    {
        return tiles[currentTileID];
    }

    public List<Tile> GetAdjacentTilesFromID(int currentTileID)
    {
        List<int> neighborIDs = adjList[currentTileID];
        List<Tile> neighborTiles = new List<Tile>();
        for(int i = 0; i < neighborIDs.Count; i++)
        {
            neighborTiles.Add(tiles[neighborIDs[i]]);
        }
        return neighborTiles;
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

    public List<Vector2> ConstructPlayerViewHull(Vector2 currentPos, Vector2 leftFunnel, Vector3 rightFunnel)
    {
        List<AngleRange> wallAngleRanges = SortedWallAngleRanges(currentPos, leftFunnel, rightFunnel);
        List<AngleRange> mergedWallAngleRanges = new List<AngleRange>();

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
                if (curAngleRange.t1 <= wallAngleRanges[j].t1 && curAngleRange.t2 > wallAngleRanges[j].t1 && curAngleRange.t2 < wallAngleRanges[j].t2)
                {
                    tempAngleRanges.Add(new AngleRange(wallAngleRanges[j].edge, curAngleRange.t2, wallAngleRanges[j].t2));
                }
                else if (curAngleRange.t1 > wallAngleRanges[j].t1 && curAngleRange.t1 < wallAngleRanges[j].t2 && curAngleRange.t2 >= wallAngleRanges[j].t2)
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


        List<Vector2> hull = new List<Vector2>();
        hull.Add(currentPos);
        mergedWallAngleRanges = mergedWallAngleRanges.OrderBy(angleRange => angleRange.t1).ToList();
        Vector2 last = currentPos;
        for (int i = 0; i < mergedWallAngleRanges.Count; i++)
        {
            AngleRange angleRange = mergedWallAngleRanges[i];
            //Debug.Log(angleRange.t1.ToString() + ", " + angleRange.t2.ToString());
            //Debug.DrawLine(new Vector3(angleRange.edge.p1.x, angleRange.edge.p1.y, -1.0f), new Vector3(angleRange.edge.p2.x, angleRange.edge.p2.y, -1.0f), Color.cyan);

            Vector2 dir1 = (Quaternion.AngleAxis(angleRange.t1, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            Vector2 s1 = angleRange.edge.Intersect(currentPos, dir1);
            hull.Add(s1);
            Debug.DrawLine(new Vector3(last.x, last.y, -0.5f), new Vector3(s1.x, s1.y, -0.5f), Color.magenta);

            Vector2 dir2 = (Quaternion.AngleAxis(angleRange.t2, new Vector3(0, 0, 1)) * rightFunnel).normalized;
            Vector2 s2 = angleRange.edge.Intersect(currentPos, dir2);
            hull.Add(s2);
            Debug.DrawLine(new Vector3(s1.x, s1.y, -0.5f), new Vector3(s2.x, s2.y, -0.5f), Color.magenta);
            last = s2;
        }
        Debug.DrawLine(new Vector3(last.x, last.y, -0.5f), new Vector3(currentPos.x, currentPos.y, -0.5f), Color.magenta);
        return hull;
    }

}

