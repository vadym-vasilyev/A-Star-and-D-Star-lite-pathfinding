using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMarkerController : MonoBehaviour {

    readonly int markerZCoord = 1;

    [SerializeField] Tilemap tilemap;

    [SerializeField] Tile openListMarker;
    [SerializeField] Tile closeListMarker;
    [SerializeField] Tile currentNodeMarker;
    [SerializeField] Tile currentMoveNodeMarker;
    [SerializeField] Tile pathNodeMarker;
    [SerializeField] Tile movedPathNodeMarker;

    public void ClearAllMarkers() {
        BoundsInt tilemapBounds = tilemap.cellBounds;
        tilemapBounds.zMin = markerZCoord;
        tilemapBounds.zMax = markerZCoord + 1;
        TileBase[] tileArray = Enumerable.Repeat<TileBase>(null, tilemapBounds.size.x * tilemapBounds.size.y * tilemapBounds.size.z).ToArray();
        tilemap.SetTilesBlock(tilemapBounds, tileArray);
    }

    public void RemoveMarker(Vertex node) {
        tilemap.SetTile(new Vector3Int(node.pos.x, node.pos.y, markerZCoord), null);
    }

    public void RemoveNonPathMarker(Vertex node) {
        Vector3Int markerPos = new Vector3Int(node.pos.x, node.pos.y, markerZCoord);
        TileBase markerAtPos = tilemap.GetTile(markerPos);
        if (markerAtPos != movedPathNodeMarker && markerAtPos != currentMoveNodeMarker) {
            tilemap.SetTile(markerPos, null);
        }
    }

    public void PutClosedNodeMarker(Vertex node) {
        PutMarkerIfCurrentNotPathMoved(node.pos, closeListMarker);
    }

    public void PutOpenNodeMarker(Vertex node) {
        PutMarkerIfCurrentNotPathMoved(node.pos, openListMarker);
    }

    public void PutCurrentNodeMarker(Vertex node) {
        PutMarkerIfCurrentNotPathMoved(node.pos, currentNodeMarker);
    }

    public void PutCurrentMoveNode(Vertex node) {
        PutMarkerIfCurrentNotPathMoved(node.pos, currentMoveNodeMarker);
    }

    public void PutPathNodeMarker(Vertex node) {
        PutMarkerIfCurrentNotPathMoved(node.pos, pathNodeMarker);
    }

    public void PutMovedPathNodeMarker(Vertex node) {
        tilemap.SetTile(new Vector3Int(node.pos.x, node.pos.y, markerZCoord), movedPathNodeMarker);
    }

    public void RemoveAllPathMarkers() {
        BoundsInt boundsInt = tilemap.cellBounds;
        for (int xPos = boundsInt.xMin; xPos < boundsInt.size.x + boundsInt.xMin; xPos++) {
            for (int yPos = boundsInt.yMin; yPos < boundsInt.size.y + boundsInt.yMin; yPos++) {
                Vector3Int pos = new Vector3Int(xPos, yPos, markerZCoord);
                Tile tile = tilemap.GetTile(pos) as Tile;
                if (tile == pathNodeMarker) {
                    tilemap.SetTile(pos, null);
                }
            }
        }
    }

    private void PutMarkerIfCurrentNotPathMoved(Vector2Int pos, Tile marker) {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, markerZCoord);
        TileBase markerAtPos = tilemap.GetTile(tilePos);
        if (markerAtPos != movedPathNodeMarker && markerAtPos != currentMoveNodeMarker) {
            tilemap.SetTile(tilePos, marker);
        }
    }
}
