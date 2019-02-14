using System.Collections.Generic;
using UnityEngine;

public interface IVertexOperations {

    Vertex GetVertexAtPos(Vector2Int pos);

    List<Vertex> GetVertextPredecessors(Vertex node);

    List<Vertex> GetVertextSuccessors(Vertex node);

}