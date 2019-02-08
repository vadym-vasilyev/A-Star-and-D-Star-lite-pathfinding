using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GridController : MonoBehaviour {

    [SerializeField] Tilemap tilemap;

    [SerializeField] Tile blockedTile;
    [SerializeField] Tile standartCostTile;
    [SerializeField] Tile doubleCostTile;
    [SerializeField] Tile tripleCostTile;

    [SerializeField] Tile startPosTile;
    [SerializeField] Tile goalPosTile;

    [SerializeField] Tile openListMarker;
    [SerializeField] Tile closeListMarker;
    [SerializeField] Tile currentNodeMarker;
    [SerializeField] Tile pathNodeMarker;

    Vector3Int startPos;
    Vector3Int goalPos;

    //TODO: get default positions from GUI
    readonly Vector3Int defaultStartPosition = new Vector3Int(-13, 4, 0);
    readonly Vector3Int defaultGoalPosition = new Vector3Int(-6, -4, 0);

    Dictionary<BrushType, Tile> brushes;
    Tile curentBrush;

    TilemapGraf graf = new TilemapGraf();

    bool inPattFindMode = false;

    const int markerZCoord = 1;

    public TilemapGraf Graf { get => graf; }

    public Tile CurrentBrush { get => curentBrush; }
    public Vector3Int StartPos { get => startPos; set => startPos = value; }
    public Vector3Int GoalPos { get => goalPos; set => goalPos = value; }

    void Awake() {
        SetDefaultStartAndGoalPositions();
        brushes = new Dictionary<BrushType, Tile>();
        brushes.Add(BrushType.standart, standartCostTile);
        brushes.Add(BrushType.doubleCost, doubleCostTile);
        brushes.Add(BrushType.tripleCost, tripleCostTile);
        brushes.Add(BrushType.blocked, blockedTile);
        brushes.Add(BrushType.start, startPosTile);
        brushes.Add(BrushType.goal, goalPosTile);
        SetBrush(BrushType.blocked);
    }

    void OnMouseOver() {
        if (Input.GetMouseButton(0)) {
            Vector3 tilemapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int coordinate = tilemap.WorldToCell(tilemapPos);
            coordinate.z = 0;
            if (inPattFindMode) {
                Debug.Log(coordinate);
                return;
            }
            Tile tile = tilemap.GetTile(coordinate) as Tile;
            if (tile == startPosTile || tile == goalPosTile) {
                return;
            }
            if (curentBrush == startPosTile) {
                if (startPos != null) {
                    tilemap.SetTile(startPos, standartCostTile);
                }
                startPos = coordinate;
            } else if (curentBrush == goalPosTile) {
                if (goalPos != null) {
                    tilemap.SetTile(goalPos, standartCostTile);
                }
                goalPos = coordinate;
            }
            tilemap.SetTile(coordinate, curentBrush);
        }
    }

    public void ClearAllMarkers() {
        inPattFindMode = false;
        BoundsInt tilemapBounds = tilemap.cellBounds;
        tilemapBounds.zMin = markerZCoord;
        tilemapBounds.zMax = markerZCoord + 1;
        TileBase[] tileArray = Enumerable.Repeat<TileBase>(null, tilemapBounds.size.x * tilemapBounds.size.y * tilemapBounds.size.z).ToArray();
        tilemap.SetTilesBlock(tilemapBounds, tileArray);
    }

    public void ResetAll() {
        ClearAllMarkers();
        BoundsInt tilemapBounds = tilemap.cellBounds;
        tilemapBounds.zMin = 0;
        tilemapBounds.zMax = 1;
        TileBase[] tileArray = Enumerable.Repeat<TileBase>(standartCostTile, tilemapBounds.size.x * tilemapBounds.size.y * tilemapBounds.size.z).ToArray();
        tilemap.SetTilesBlock(tilemapBounds, tileArray);
        SetDefaultStartAndGoalPositions();
    }

    public void CreateGraf() {
        inPattFindMode = true;
        BoundsInt boundsInt = tilemap.cellBounds;
        graf.InitNodeMap();
        AddAllVertecies(boundsInt);
        AddAllEdges(boundsInt);
    }

    public void SetBrush(BrushType brushType) {
        curentBrush = brushes[brushType];
    }

    public void PutClosedNodeMarker(Vertex node) {
        Vector2Int pos = GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), closeListMarker);
    }

    public void PutOpenNodeMarker(Vertex node) {
        Vector2Int pos = GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), openListMarker);
    }

    public void PutCurrentNodeMarker(Vertex node) {
        Vector2Int pos = GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), currentNodeMarker);
    }

    public void PutPathNodeMarker(Vertex node) {
        Vector2Int pos = GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), pathNodeMarker);
    }

    public void RemoveMarker(Vertex node) {
        Vector2Int pos = GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), null);
    }

    public Vertex GetVertexAtPos(Vector2Int pos) {
        return graf.GetVertexAtPos(pos);
    }

    public Vector2Int GetPosForVertex(Vertex vertex) {
        return graf.getPositionByVertex(vertex);
    }

    public Vertex GetStartNodeVertex() {
        return GetVertexAtPos(new Vector2Int(startPos.x, startPos.y));
    }

    public Vertex GetGoalNodeVertex() {
        return GetVertexAtPos(new Vector2Int(goalPos.x, goalPos.y));
    }

    private void AddAllEdges(BoundsInt boundsInt) {

        for (int xPos = boundsInt.xMin; xPos < boundsInt.size.x + boundsInt.xMin; xPos++) {
            for (int yPos = boundsInt.yMin; yPos < boundsInt.size.y + boundsInt.yMin; yPos++) {
                Vertex vertexFrom = graf.GetVertexAtPos(new Vector2Int(xPos, yPos));
                if (vertexFrom == null || vertexFrom.blocked) {
                    continue;
                }
                AddEdge(vertexFrom, graf.GetVertexAtPos(new Vector2Int(xPos - 1, yPos)));
                AddEdge(vertexFrom, graf.GetVertexAtPos(new Vector2Int(xPos + 1, yPos)));
                AddEdge(vertexFrom, graf.GetVertexAtPos(new Vector2Int(xPos, yPos - 1)));
                AddEdge(vertexFrom, graf.GetVertexAtPos(new Vector2Int(xPos, yPos + 1)));
            }
        }
    }

    private void AddEdge(Vertex vertexFrom, Vertex vertexTo) {
        if (vertexTo == null || vertexTo.blocked) {
            return;
        }
        float cost = (vertexTo.cost + vertexFrom.cost) / 2;
        graf.AddEdge(vertexFrom, vertexTo, cost);
    }

    private void AddAllVertecies(BoundsInt boundsInt) {
        for (int xPos = boundsInt.xMin; xPos < boundsInt.size.x + boundsInt.xMin; xPos++) {
            for (int yPos = boundsInt.yMin; yPos < boundsInt.size.y + boundsInt.yMin; yPos++) {
                Tile tile = tilemap.GetTile(new Vector3Int(xPos, yPos, 0)) as Tile;
                if (!tile) {
                    continue;
                }
                CreateAndAddVertexForTile(xPos, yPos, tile);
            }
        }
    }

    private void CreateAndAddVertexForTile(int xPos, int yPos, Tile tile) {
        bool blocked = IsTileBlocked(tile);
        float cost = 0f;
        if (!blocked) {
            cost = TileCost(tile);
        }
        Vertex vertex = new Vertex(cost, blocked, new List<Edge>());
        graf.AddNode(new Vector2Int(xPos, yPos), vertex);
    }

    private bool IsTileBlocked(Tile tile) {
        return tile == blockedTile;
    }

    //TODO: Change if to cost dictionary
    private float TileCost(Tile tile) {
        if (tile == standartCostTile || tile == startPosTile || tile == goalPosTile) {
            return 1f;
        } else if (tile == doubleCostTile) {
            return 2f;
        } else if (tile == tripleCostTile) {
            return 3f;
        }
        throw new ArgumentException("Unknow tile on tilemap");
    }

    private void SetDefaultStartAndGoalPositions() {
        startPos = defaultStartPosition;
        goalPos = defaultGoalPosition;
        tilemap.SetTile(startPos, startPosTile);
        tilemap.SetTile(goalPos, goalPosTile);
    }
}

public enum BrushType {
    standart,
    doubleCost,
    tripleCost,
    blocked,
    start,
    goal
}
