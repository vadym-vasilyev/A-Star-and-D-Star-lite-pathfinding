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
    float Estimate(Vector2Int currentPos, Vector2Int toPos);
}

public interface IHeuristicTieBarker {
    void ApllyTieBraker(Vector2Int currentPos, Vector2Int toPos, ref float estimate, float heuristicCoeff);
}

public abstract class TieBrakeHeueristic : IHeuristicEstimate {
    protected IHeuristicTieBarker tieBraker;
    protected float heuristicCoeff;

    protected TieBrakeHeueristic(IHeuristicTieBarker tieBraker, float heuristicCoeff) {
        this.tieBraker = tieBraker;
        this.heuristicCoeff = heuristicCoeff;
    }

    public float Estimate(Vector2Int currentPos, Vector2Int toPos) {
        float heuristicValue = EstimateRaw(currentPos, toPos);
        tieBraker.ApllyTieBraker(currentPos, toPos, ref heuristicValue, heuristicCoeff);
        return heuristicValue;
    }

    public abstract float EstimateRaw(Vector2Int currentPos, Vector2Int toPos);
}

//TODO: change to Builder pattern
public static class HeuristicFactory {
    public static IHeuristicEstimate СreateHeuristic(HeuristicType heuristicType, TieBreakerType tieBreaker, Vector2Int startPos, float heuristicCoeff) {
        switch (heuristicType) {
            case HeuristicType.Euclidean:
                return new EuclideanDistanceHeuristic(СreateTieBraker(tieBreaker, startPos), heuristicCoeff);
            case HeuristicType.Manhattan:
                return new ManhattanDistanceHeuristic(СreateTieBraker(tieBreaker, startPos), heuristicCoeff);
            default:
                return new NoneHeuristic();
        }
    }

    private static IHeuristicTieBarker СreateTieBraker(TieBreakerType tieBreaker, Vector2Int startPos) {
        switch (tieBreaker) {
            case TieBreakerType.ScaleHUpwards:
                return new ScaleHUpwardTieBrak();
            case TieBreakerType.Directed:
                return new DirectionalTieBraker(startPos);
            default:
                return new NoneTieBrak();
        }
    }
}


public class EuclideanDistanceHeuristic : TieBrakeHeueristic {


    public EuclideanDistanceHeuristic(IHeuristicTieBarker tieBarker, float heuristicCoeff) : base(tieBarker, heuristicCoeff) {
    }

    public override float EstimateRaw(Vector2Int currentPos, Vector2Int toPos) {
        return Vector2Int.Distance(currentPos, toPos) * heuristicCoeff;
    }
}

public class ManhattanDistanceHeuristic : TieBrakeHeueristic {

    public ManhattanDistanceHeuristic(IHeuristicTieBarker tieBarker, float heuristicCoeff) : base(tieBarker, heuristicCoeff) {
    }

    public override float EstimateRaw(Vector2Int currentPos, Vector2Int toPos) {
        float estimate = heuristicCoeff * (Mathf.Abs(currentPos.x - toPos.x) + Mathf.Abs(currentPos.y - toPos.y));
        return estimate;
    }
}

public class NoneHeuristic : IHeuristicEstimate {
    public float Estimate(Vector2Int currentPos, Vector2Int toPos) {
        return 0f;
    }
}

public class ScaleHUpwardTieBrak : IHeuristicTieBarker {
    public void ApllyTieBraker(Vector2Int currentPos, Vector2Int toPos, ref float estimate, float heuristicCoeff) {
        estimate *= (1.0f + heuristicCoeff / 1000);
    }
}

public class DirectionalTieBraker : IHeuristicTieBarker {
    Vector2Int fromPos;

    public DirectionalTieBraker(Vector2Int fromPos) {
        this.fromPos = fromPos;
    }

    public void ApllyTieBraker(Vector2Int currentPos, Vector2Int toPos, ref float estimate, float heuristicCoeff) {
        float dx1 = currentPos.x - toPos.x;
        float dy1 = currentPos.y - toPos.y;
        float dx2 = fromPos.x - toPos.x;
        float dy2 = fromPos.y - toPos.y;
        float cross = Mathf.Abs(dx1 * dy2 - dx2 * dy1);
        estimate += cross * 0.001f;
    }
}

public class NoneTieBrak : IHeuristicTieBarker {
    public void ApllyTieBraker(Vector2Int currentPos, Vector2Int toPos, ref float estimate, float heuristicCoeff) {
    }
}
