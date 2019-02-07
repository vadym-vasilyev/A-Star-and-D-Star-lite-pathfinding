using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeuristicEstimate {
    float Estimate(Vertex from, Vertex goal);
}

public class EuclideanDistanceHeuristic : IHeuristicEstimate {

    TilemapGraf graf;

    public EuclideanDistanceHeuristic(TilemapGraf graf) {
        this.graf = graf;
    }

    public float Estimate(Vertex from, Vertex goal) {
        Vector2Int currentPos = graf.getPositionByVertex(from);
        Vector2Int goalPos = graf.getPositionByVertex(goal);
        return Vector2Int.Distance(currentPos, goalPos);
    }
}

public class ManhattanDistanceHeuristic : IHeuristicEstimate {

    TilemapGraf graf;
    float minimumCost;
    bool tieBreaking;

    public ManhattanDistanceHeuristic(TilemapGraf graf, float minimumCost = 1f, bool tieBreaking = false) {
        this.graf = graf;
        this.minimumCost = minimumCost;
        this.tieBreaking = tieBreaking;
    }

    public float Estimate(Vertex from, Vertex goal) {
        Vector2Int currentPos = graf.getPositionByVertex(from);
        Vector2Int goalPos = graf.getPositionByVertex(goal);
        float estimate = minimumCost * (Mathf.Abs(currentPos.x - goalPos.x) + Mathf.Abs(currentPos.y - goalPos.y));
        if (tieBreaking) {
            estimate *= (1.0f + minimumCost / 1000);
        }
        return estimate;
    }
}
