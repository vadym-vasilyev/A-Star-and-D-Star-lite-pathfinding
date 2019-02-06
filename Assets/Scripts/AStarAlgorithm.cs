using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarAlgorithm {

    private GridController gridController;

    private TilemapGraf graf;

    IHeuristicEstimate heuristic;

    DictionaryWithSortedValues<Vertex, PathRecord> openList = new DictionaryWithSortedValues<Vertex, PathRecord>();
    Dictionary<Vertex, PathRecord> closeList = new Dictionary<Vertex, PathRecord>();

    public List<Edge> FindPath(Vertex startNode, Vertex goalNode) {

        openList.Add(startNode, new PathRecord(startNode));

        PathRecord current = null;

        while (openList.Count > 0) {
            current = openList.GetmMinValue();
            if (current.node == goalNode) {
                //WOW! we found goal!
                break;
            }
            ProcessAllEdgesFromCurrent(current, goalNode);
            openList.Remove(current.node);
            closeList.Add(current.node, current);
        }

        if (current.node == goalNode) {
            List<Edge> path = new List<Edge>();
            while (current.node != startNode) {
                path.Add(current.connection.edge);
                current = current.connection.fromRecord;
            }
            path.Reverse();
            return path;
        }
        return null;
    }

    private void ProcessAllEdgesFromCurrent(PathRecord current, Vertex goalNode) {
        foreach (Edge edge in current.node.edges) {

            PathRecord nextNode;
            float nextNodeCostSoFar = current.costSoFar + edge.cost;
            float endNodeHeuristic;

            if (closeList.ContainsKey(edge.to)) {
                nextNode = closeList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                endNodeHeuristic = nextNode.etimatedCost - nextNode.costSoFar;
                closeList.Remove(nextNode.node);
            } else if (openList.ContainsKey(edge.to)) {
                nextNode = openList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                endNodeHeuristic = nextNode.etimatedCost - nextNode.costSoFar;
            } else {
                nextNode = new PathRecord(edge.to);
                endNodeHeuristic = heuristic.Estimate(nextNode.node, goalNode);
            }

            nextNode.costSoFar = nextNodeCostSoFar;
            nextNode.etimatedCost = nextNode.costSoFar + endNodeHeuristic;
            nextNode.connection = new PathConnection(edge, current);

            if (!openList.ContainsKey(nextNode.node)) {
                openList.Add(nextNode.node, nextNode);
            }
        }
    }
}

public class PathRecord : Comparer<PathRecord> {
    public Vertex node;
    public PathConnection connection;
    public float costSoFar;
    public float etimatedCost;

    public PathRecord(Vertex node) {
        this.node = node;
    }

    public override int Compare(PathRecord x, PathRecord y) {
        return Comparer<float>.Default.Compare(x.etimatedCost, y.etimatedCost);
    }

    public override bool Equals(object obj) {
        var record = obj as PathRecord;
        return record != null &&
               EqualityComparer<Vertex>.Default.Equals(node, record.node);
    }

    public override int GetHashCode() {
        return -231681771 + EqualityComparer<Vertex>.Default.GetHashCode(node);
    }
}

public class PathConnection {
    public Edge edge;
    public PathRecord fromRecord;

    public PathConnection(Edge edge, PathRecord fromRecord) {
        this.edge = edge;
        this.fromRecord = fromRecord;
    }
}


class DictionaryWithSortedValues<T1, T2> {
    private IDictionary<T1, T2> forward = new Dictionary<T1, T2>();
    private SortedSet<T2> sortedValues = new SortedSet<T2>();

    public int Count { get => forward.Count; }

    public void Add(T1 key, T2 value) {
        forward.Add(key, value);
        sortedValues.Add(value);
    }

    public void Remove(T1 key) {
        T2 Value = forward[key];
        forward.Remove(key);
        sortedValues.Remove(Value);
    }

    public T2 GetmMinValue() {
        return sortedValues.Min;
    }

    public bool ContainsKey(T1 key) {
        return forward.ContainsKey(key);
    }

    public T2 this[T1 index] {
        get { return forward[index]; }
        set { forward[index] = value; }
    }
}

interface IHeuristicEstimate {
    float Estimate(Vertex from, Vertex goal);
}

class EuclideanDistanceHeuristic : IHeuristicEstimate {

    TilemapGraf graf;

    public float Estimate(Vertex from, Vertex goal) {
        Vector2Int currentPos = graf.getPositionByVertex(from);
        Vector2Int goalPos = graf.getPositionByVertex(goal);
        Vector2Int distance = (goalPos - currentPos);
        return Mathf.Sqrt(distance.x * distance.x + distance.y * distance.y);
    }
}