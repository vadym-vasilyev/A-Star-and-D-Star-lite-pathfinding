using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm {

    readonly GridController gridController;
    float sleepTime;

    IHeuristicEstimate heuristic;

    OpenList openList = new OpenList();
    Dictionary<Vertex, PathRecord> closeList = new Dictionary<Vertex, PathRecord>();

    public AStarAlgorithm(GridController gridController, IHeuristicEstimate heuristic, float sleepTime = 0.1f) {
        this.gridController = gridController;
        this.sleepTime = sleepTime;
        this.heuristic = heuristic;
    }

    public System.Collections.IEnumerator FindPath(System.Action<List<Edge>> callback = null) {
        Vertex startNode = gridController.GetStartNodeVertex();
        Vertex goalNode = gridController.GetGoalNodeVertex();
        openList.Add(startNode, new PathRecord(startNode));

        #region Not a part of algorithm, visualization purposes only
        gridController.PutOpenNodeMarker(startNode);
        #endregion

        PathRecord current = null;

        while (openList.Count > 0) {
            current = openList.PopMinValue();
            #region Not a part of algorithm, visualization purposes only
            gridController.PutCurrentNodeMarker(current.node);
            #endregion

            if (current.node == goalNode) {
                //WOW! we found goal!
                break;
            }
            yield return ProcessAllEdgesFromCurrent(current, goalNode);
            gridController.RemoveMarker(current.node);

            closeList.Add(current.node, current);
            openList.Remove(current.node);
            gridController.PutClosedNodeMarker(current.node);
            yield return new WaitForSecondsRealtime(sleepTime);
        }
        List<Edge> path = new List<Edge>();

        if (current.node == goalNode) {
            while (current.node != startNode) {
                #region Not a part of algorithm, visualization purposes only
                if (goalNode != current.node) {
                    gridController.PutPathNodeMarker(current.node);
                    yield return new WaitForSecondsRealtime(sleepTime);
                }
                #endregion
                path.Add(current.connection.edge);
                current = current.connection.fromRecord;
            }
            path.Reverse();
        }

        if (callback != null) {
            callback.Invoke(path);
        }
    }

    public System.Collections.IEnumerator ProcessAllEdgesFromCurrent(PathRecord current, Vertex goalNode) {
        foreach (Edge edge in current.node.edges) {

            PathRecord nextNode;
            float nextNodeCostSoFar = current.costSoFar + edge.cost;
            float nextNodeHeuristic;

            if (closeList.ContainsKey(edge.to)) {
                nextNode = closeList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                nextNodeHeuristic = nextNode.heuristicValue;
                closeList.Remove(nextNode.node);
                #region Not a part of algorithm, visualization purposes only
                gridController.RemoveMarker(nextNode.node);
                #endregion
            } else if (openList.ContainsKey(edge.to)) {
                nextNode = openList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                nextNodeHeuristic = nextNode.heuristicValue;
            } else {
                nextNode = new PathRecord(edge.to);
                nextNodeHeuristic = heuristic.Estimate(gridController.GetPosForVertex(nextNode.node));
            }

            nextNode.costSoFar = nextNodeCostSoFar;
            nextNode.heuristicValue = nextNodeHeuristic;
            nextNode.connection = new PathConnection(edge, current);
            if (!openList.ContainsKey(nextNode.node)) {
                openList.Add(nextNode.node, nextNode);
                #region Not a part of algorithm, visualization purposes only
                gridController.PutOpenNodeMarker(nextNode.node);
                #endregion
            }
            yield return new WaitForSecondsRealtime(sleepTime);
        }
    }
}

public class PathRecord {
    public Vertex node;
    public PathConnection connection;
    public float costSoFar;
    public float heuristicValue;

    public PathRecord(Vertex node) {
        this.node = node;
    }

    public float GetCostEstimation() {
        return costSoFar + heuristicValue;
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

class OpenList {
    class PathRecordComparer : IComparer<PathRecord> {
        public int Compare(PathRecord x, PathRecord y) {

            if (x.node.Equals(y.node)) {
                return 0;
            } 

            int result = Comparer<float>.Default.Compare(x.GetCostEstimation(), y.GetCostEstimation());
            if (result == 0) {
                result = Comparer<float>.Default.Compare(x.heuristicValue, y.heuristicValue);
            }
            if (result == 0) {
                result = -1;
            }
            return result;
        }
    }

    IDictionary<Vertex, PathRecord> pathRecords = new Dictionary<Vertex, PathRecord>();
    SortedSet<PathRecord> sortedValues = new SortedSet<PathRecord>(new PathRecordComparer());

    public int Count { get => pathRecords.Count; }

    public void Add(Vertex key, PathRecord value) {
        pathRecords.Add(key, value);
        sortedValues.Add(value);
    }

    public void Remove(Vertex key) {
        PathRecord value = pathRecords[key];
        pathRecords.Remove(key);
        sortedValues.Remove(value);
    }

    public PathRecord PopMinValue() {
        return sortedValues.Min;
    }

    public bool ContainsKey(Vertex key) {
        return pathRecords.ContainsKey(key);
    }

    public PathRecord this[Vertex index] {
        get { return pathRecords[index]; }
        set { pathRecords[index] = value; }
    }
}
