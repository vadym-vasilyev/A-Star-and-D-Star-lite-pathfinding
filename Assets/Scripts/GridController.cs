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
    Dictionary<BrushType, Tile> brushes;

    //TODO: get default positions from GUI
    readonly Vector3Int defaultStartPosition = new Vector3Int(-13, 4, 0);
    readonly Vector3Int defaultGoalPosition = new Vector3Int(-6, -4, 0);

    public Tile CurrentBrush { get; private set; }
    public Vector3Int StartPos { get; set; }
    public Vector3Int GoalPos { get; set; }
    public TilemapGraf Graf { get; set; } = new TilemapGraf();
    public PathGridState PathGridState { get; set; } = PathGridState.Edit;

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
            if (PathGridState == PathGridState.PathFind) {
                Debug.Log(coordinate);
                return;
            }
            Tile tile = tilemap.GetTile(coordinate) as Tile;
            if (tile == startPosTile || tile == goalPosTile) {
                return;
            }
            if (CurrentBrush == startPosTile) {
                if (StartPos != null) {
                    tilemap.SetTile(StartPos, standartCostTile);
                }
                StartPos = coordinate;
            } else if (CurrentBrush == goalPosTile) {
                if (GoalPos != null) {
                    tilemap.SetTile(GoalPos, standartCostTile);
                }
                GoalPos = coordinate;
            }
            tilemap.SetTile(coordinate, CurrentBrush);
            if (PathGridState == PathGridState.MoveByPath) {
                UpdateVertex(new Vector2Int(coordinate.x, coordinate.y), CurrentBrush);
            }
        }
    }

    public Vertex GetStartVertex() {
        return Graf.GetVertexAtPos(new Vector2Int(StartPos.x, StartPos.y));
    }

    public Vertex GetGoalVertex() {
        return Graf.GetVertexAtPos(new Vector2Int(GoalPos.x, GoalPos.y));
    }

    public void ClearAllNodes() {
        BoundsInt tilemapBounds = tilemap.cellBounds;
        tilemapBounds.zMin = 0;
        tilemapBounds.zMax = 1;
        TileBase[] tileArray = Enumerable.Repeat<TileBase>(standartCostTile, tilemapBounds.size.x * tilemapBounds.size.y * tilemapBounds.size.z).ToArray();
        tilemap.SetTilesBlock(tilemapBounds, tileArray);
        SetDefaultStartAndGoalPositions();
    }

    public void InitGraf() {
        BoundsInt boundsInt = tilemap.cellBounds;
        Graf.InitNodeMap();
        AddAllVertecies(boundsInt);
        AddAllEdges(boundsInt);
    }

    public void SetBrush(BrushType brushType) {
        CurrentBrush = brushes[brushType];
    }

    public List<Vertex> UpdateVertex(Vector2Int pos, Tile tile) {
        Vertex vertexNew = CreateVertex(tile);
        Vertex vertexOld = Graf.GetVertexAtPos(pos);

        if (vertexNew == vertexOld) {
            return new List<Vertex>();
        }

        Graf.AddNode(pos, vertexNew);

        List<Vertex> predecessors = Graf.GetVertextPredecessors(vertexOld);
        predecessors.ForEach(p => {
            p.edges.RemoveAll(e => e.to == vertexOld);
            if (!vertexNew.blocked && !p.blocked) {
                AddEdge(p, vertexNew);
                AddEdge(vertexNew, p);
            }
        });

        List<Vertex> changedVertecies = new List<Vertex>(predecessors);
        changedVertecies.Add(vertexNew);
        return changedVertecies;
    }

    private void AddAllEdges(BoundsInt boundsInt) {

        for (int xPos = boundsInt.xMin; xPos < boundsInt.size.x + boundsInt.xMin; xPos++) {
            for (int yPos = boundsInt.yMin; yPos < boundsInt.size.y + boundsInt.yMin; yPos++) {
                Vertex vertexFrom = Graf.GetVertexAtPos(new Vector2Int(xPos, yPos));
                if (vertexFrom == null || vertexFrom.blocked) {
                    continue;
                }
                AddEdge(vertexFrom, Graf.GetVertexAtPos(new Vector2Int(xPos - 1, yPos)));
                AddEdge(vertexFrom, Graf.GetVertexAtPos(new Vector2Int(xPos + 1, yPos)));
                AddEdge(vertexFrom, Graf.GetVertexAtPos(new Vector2Int(xPos, yPos - 1)));
                AddEdge(vertexFrom, Graf.GetVertexAtPos(new Vector2Int(xPos, yPos + 1)));
            }
        }
    }

    private void AddEdge(Vertex vertexFrom, Vertex vertexTo) {
        if (vertexTo == null || vertexTo.blocked) {
            return;
        }
        float cost = (vertexTo.cost + vertexFrom.cost) / 2;
        Graf.AddEdge(vertexFrom, vertexTo, cost);
    }

    private void AddAllVertecies(BoundsInt boundsInt) {
        for (int xPos = boundsInt.xMin; xPos < boundsInt.size.x + boundsInt.xMin; xPos++) {
            for (int yPos = boundsInt.yMin; yPos < boundsInt.size.y + boundsInt.yMin; yPos++) {
                Tile tile = tilemap.GetTile(new Vector3Int(xPos, yPos, 0)) as Tile;
                if (!tile) {
                    continue;
                }
                CreateAndAddVertexForTile(new Vector2Int(xPos, yPos), tile);
            }
        }
    }

    private void CreateAndAddVertexForTile(Vector2Int pos, Tile tile) {
        Vertex vertex = CreateVertex(tile);
        Graf.AddNode(pos, vertex);
    }

    private Vertex CreateVertex(Tile tile) {
        bool blocked = IsTileBlocked(tile);
        float cost = 0f;
        if (!blocked) {
            cost = TileCost(tile);
        }
        return new Vertex(cost, blocked, new List<Edge>());
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
        StartPos = defaultStartPosition;
        GoalPos = defaultGoalPosition;
        tilemap.SetTile(StartPos, startPosTile);
        tilemap.SetTile(GoalPos, goalPosTile);
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

public enum PathGridState {
    Edit,
    PathFind,
    MoveByPath
}
