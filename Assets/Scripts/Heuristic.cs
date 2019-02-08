using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeuristicEstimate {
    float Estimate(Vector2Int currentPos);
}

public static class HeuristicFactory {
    public static IHeuristicEstimate GetHeuristic(HeuristicType heuristicType, Vector2Int goalPos, float heuristicCoeff) {
        switch (heuristicType) {
            case HeuristicType.Euclidean:
                Debug.Log("Euclidian");
                return new EuclideanDistanceHeuristic(goalPos, heuristicCoeff);
            case HeuristicType.Manhattan:
                Debug.Log("Manhattan");
                return new ManhattanDistanceHeuristic(goalPos, heuristicCoeff);
            default:
                Debug.Log("None");
                return new NoneHeuristic();
        }
    }
}

public enum HeuristicType {
    Euclidean = 0,
    Manhattan = 1,
    None = 3

}

public class EuclideanDistanceHeuristic : IHeuristicEstimate {

    Vector2Int goalPos;
    float heuristicCoeff;

    public EuclideanDistanceHeuristic(Vector2Int goalPos, float heuristicCoeff) {
        this.goalPos = goalPos;
        this.heuristicCoeff = heuristicCoeff;
    }

    public float Estimate(Vector2Int currentPos) {
        return Vector2Int.Distance(currentPos, goalPos) * heuristicCoeff;
    }
}

public class ManhattanDistanceHeuristic : IHeuristicEstimate {

    Vector2Int goalPos;
    float heuristicCoeff;


    public ManhattanDistanceHeuristic(Vector2Int goalPos, float heuristicCoeff) {
        this.goalPos = goalPos;
        this.heuristicCoeff = heuristicCoeff;
    }

    public float Estimate(Vector2Int currentPos) {
        float estimate = heuristicCoeff * (Mathf.Abs(currentPos.x - goalPos.x) + Mathf.Abs(currentPos.y - goalPos.y));
        return estimate;
    }
}

public class NoneHeuristic : IHeuristicEstimate {
    public float Estimate(Vector2Int currentPos) {
        return 0f;
    }
}
