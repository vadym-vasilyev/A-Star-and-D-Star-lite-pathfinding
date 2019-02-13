using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMarkerController : MonoBehaviour {

    readonly int markerZCoord = 1;

    [SerializeField] Tilemap tilemap;

    [SerializeField] Tile openListMarker;
    [SerializeField] Tile closeListMarker;
    [SerializeField] Tile currentNodeMarker;
    [SerializeField] Tile pathNodeMarker;

    public IVertextPositionResolver VertexPositionResolver { get; set; }

    void Start() {
        VertexPositionResolver = GetComponent<GridController>().Graf;
    }

    public void ClearAllMarkers() {
        BoundsInt tilemapBounds = tilemap.cellBounds;
        tilemapBounds.zMin = markerZCoord;
        tilemapBounds.zMax = markerZCoord + 1;
        TileBase[] tileArray = Enumerable.Repeat<TileBase>(null, tilemapBounds.size.x * tilemapBounds.size.y * tilemapBounds.size.z).ToArray();
        tilemap.SetTilesBlock(tilemapBounds, tileArray);
    }

    public void RemoveMarker(Vertex node) {
        Vector2Int pos = VertexPositionResolver.GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), null);
    }

    public void PutClosedNodeMarker(Vertex node) {
        Vector2Int pos = VertexPositionResolver.GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), closeListMarker);
    }

    public void PutOpenNodeMarker(Vertex node) {
        Vector2Int pos = VertexPositionResolver.GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), openListMarker);
    }

    public void PutCurrentNodeMarker(Vertex node) {
        Vector2Int pos = VertexPositionResolver.GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), currentNodeMarker);
    }

    public void PutPathNodeMarker(Vertex node) {
        Vector2Int pos = VertexPositionResolver.GetPosForVertex(node);
        tilemap.SetTile(new Vector3Int(pos.x, pos.y, markerZCoord), pathNodeMarker);
    }
}
