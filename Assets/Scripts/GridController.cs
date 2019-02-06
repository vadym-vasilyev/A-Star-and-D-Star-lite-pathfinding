using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GridController : MonoBehaviour {

    [SerializeField] Tilemap tilemap;

    [SerializeField] Tile blocked;
    [SerializeField] Tile standartCost;
    [SerializeField] Tile doubleCost;
    [SerializeField] Tile tripleCost;

    [SerializeField] Tile startPosTile;
    [SerializeField] Tile goalPosTile;

    [SerializeField] Tile openList;
    [SerializeField] Tile closeList;
    [SerializeField] Tile pathTile;

    [SerializeField] Image currentBrush;

    Vector3Int startPos = new Vector3Int(-13, 4, 0);
    Vector3Int goalPos = new Vector3Int(6, -6, 0);

    Tile brush;

    private TilemapGraf graf = new TilemapGraf();

    public TilemapGraf Graf { get => graf; private set => graf = value; }

    void OnMouseOver() {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 tilemapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int coordinate = tilemap.WorldToCell(tilemapPos);
            coordinate.z = 0;
            Tile tile = tilemap.GetTile(coordinate) as Tile;
            if (tile == startPosTile || tile == goalPosTile) {
                return;
            }
            if (brush == startPosTile) {
                if (startPos != null) {
                    tilemap.SetTile(startPos, standartCost);
                }
                startPos = coordinate;
            } else if (brush == goalPosTile) {
                if (goalPos != null) {
                    tilemap.SetTile(goalPos, standartCost);
                }
                goalPos = coordinate;
            }
            tilemap.SetTile(coordinate, brush);
        }
    }

    void Start() {
        brush = blocked;
        currentBrush.sprite = brush.sprite;
        tilemap.SetTile(startPos, startPosTile);
        tilemap.SetTile(goalPos, goalPosTile);
    }

    public void SetBrushBlocked() {
        brush = blocked;
        currentBrush.sprite = brush.sprite;
    }

    public void SetBrushStandart() {
        brush = standartCost;
        currentBrush.sprite = brush.sprite;
    }

    public void SetBrushDoubleCost() {
        brush = doubleCost;
        currentBrush.sprite = brush.sprite;
    }

    public void SetBrushTripleCost() {
        brush = tripleCost;
        currentBrush.sprite = brush.sprite;
    }

    public void SetBrushGoal() {
        brush = goalPosTile;
        currentBrush.sprite = brush.sprite;
    }

    public void SetBrushStart() {
        brush = startPosTile;
        currentBrush.sprite = brush.sprite;
    }

    public void CreateGraf() {
        BoundsInt boundsInt = tilemap.cellBounds;
        graf.InitNodeMap();
        AddAllVertecies(boundsInt);
        AddAllEdges(boundsInt);
    }

    private void AddAllEdges(BoundsInt boundsInt) {
        for (int xPos = 0; xPos < boundsInt.size.x; xPos++) {
            for (int yPos = 0; yPos < boundsInt.size.y; yPos++) {
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
        for (int xPos = 0; xPos < boundsInt.size.x; xPos++) {
            for (int yPos = 0; yPos < boundsInt.size.y; yPos++) {
                Tile tile = tilemap.GetTile(new Vector3Int(boundsInt.xMin + xPos, boundsInt.yMin + yPos, 0)) as Tile;
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
        return tile == blocked;
    }

    //TODO: Change if to cost dictionary
    private float TileCost(Tile tile) {
        if (tile == standartCost) {
            return 1f;
        } else if (tile == doubleCost) {
            return 2f;
        } else if (tile == tripleCost) {
            return 3f;
        }
        throw new ArgumentException("Unknow tile on tilemap");
    }
}
