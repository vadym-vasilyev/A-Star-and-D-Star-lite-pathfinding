using System.Collections.Generic;
using UnityEngine;

public interface IVertextPositionResolver {

    Vertex GetVertexAtPos(Vector2Int pos);

    List<Vertex> GetVertextPredecessors(Vertex node);

    List<Vertex> GetVertextSuccessors(Vertex node);

    Vector2Int GetPosForVertex(Vertex vertex);

}