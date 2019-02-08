using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeuristicType {
    Manhattan = 0,
    Euclidean = 1,
    None = 2
}

public enum TieBreakerType {
    ScaleHUpwards = 0,
    Directed = 1,
    None = 2
}

public interface IHeuristicEstimate {
    float Estimate(Vector2Int currentPos);
}

public interface IHeuristicTieBarker {
    void ApllyTieBraker(Vector2Int currentPos, ref float estimate, float heuristicCoeff);
}

public abstract class TieBrakeHeueristic : IHeuristicEstimate {
    protected IHeuristicTieBarker tieBraker;
    protected float heuristicCoeff;

    protected TieBrakeHeueristic(IHeuristicTieBarker tieBraker, float heuristicCoeff) {
        this.tieBraker = tieBraker;
        this.heuristicCoeff = heuristicCoeff;
    }

    public float Estimate(Vector2Int currentPos) {
        float heuristicValue = EstimateRaw(currentPos);
        tieBraker.ApllyTieBraker(currentPos, ref heuristicValue, heuristicCoeff);
        return heuristicValue;
    }

    public abstract float EstimateRaw(Vector2Int currentPos);
}


public static class HeuristicFactory {
    public static IHeuristicEstimate СreateHeuristic(HeuristicType heuristicType, TieBreakerType tieBreaker, Vector2Int startPos, Vector2Int goalPos, float heuristicCoeff) {
        switch (heuristicType) {
            case HeuristicType.Euclidean:
                return new EuclideanDistanceHeuristic(СreateTieBraker(tieBreaker, startPos, goalPos), goalPos, heuristicCoeff);
            case HeuristicType.Manhattan:
                return new ManhattanDistanceHeuristic(СreateTieBraker(tieBreaker, startPos, goalPos), goalPos, heuristicCoeff);
            default:
                return new NoneHeuristic();
        }
    }

    private static IHeuristicTieBarker СreateTieBraker(TieBreakerType tieBreaker, Vector2Int startPos, Vector2Int goalPos) {
        switch (tieBreaker) {
            case TieBreakerType.ScaleHUpwards:
                return new ScaleHUpwardTieBrak();
            case TieBreakerType.Directed:
                return new DirectionalTieBraker(startPos, goalPos);
            default:
                return new NoneTieBrak();
        }
    }
}


public class EuclideanDistanceHeuristic : TieBrakeHeueristic {

    Vector2Int goalPos;

    public EuclideanDistanceHeuristic(IHeuristicTieBarker tieBarker, Vector2Int goalPos, float heuristicCoeff) : base(tieBarker, heuristicCoeff) {
        this.goalPos = goalPos;
    }

    public override float EstimateRaw(Vector2Int currentPos) {
        return Vector2Int.Distance(currentPos, goalPos) * heuristicCoeff;
    }
}

public class ManhattanDistanceHeuristic : TieBrakeHeueristic {

    Vector2Int goalPos;


    public ManhattanDistanceHeuristic(IHeuristicTieBarker tieBarker, Vector2Int goalPos, float heuristicCoeff) : base(tieBarker, heuristicCoeff) {
        this.goalPos = goalPos;
    }

    public override float EstimateRaw(Vector2Int currentPos) {
        float estimate = heuristicCoeff * (Mathf.Abs(currentPos.x - goalPos.x) + Mathf.Abs(currentPos.y - goalPos.y));
        return estimate;
    }
}

public class NoneHeuristic : IHeuristicEstimate {
    public float Estimate(Vector2Int currentPos) {
        return 0f;
    }
}

public class ScaleHUpwardTieBrak : IHeuristicTieBarker {
    public void ApllyTieBraker(Vector2Int currentPos, ref float estimate, float heuristicCoeff) {
        estimate *= (1.0f + heuristicCoeff / 1000);
    }
}

public class DirectionalTieBraker : IHeuristicTieBarker {
    Vector2Int startPos;
    Vector2Int goalPos;

    public DirectionalTieBraker(Vector2Int startPos, Vector2Int goalPos) {
        this.startPos = startPos;
        this.goalPos = goalPos;
    }

    public void ApllyTieBraker(Vector2Int currentPos, ref float estimate, float heuristicCoeff) {
        float dx1 = currentPos.x - goalPos.x;
        float dy1 = currentPos.y - goalPos.y;
        float dx2 = startPos.x - goalPos.x;
        float dy2 = startPos.y - goalPos.y;
        float cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
        estimate += cross * 0.001f;
    }
}

public class NoneTieBrak : IHeuristicTieBarker {
    public void ApllyTieBraker(Vector2Int currentPos, ref float estimate, float heuristicCoeff) {
    }
}
