using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithm {

    GridMarkerController gridMarkerController;
    IHeuristicEstimate heuristic;
    readonly float sleepTime;

    OpenList openList = new OpenList();
    Dictionary<Vertex, PathRecord> closeList = new Dictionary<Vertex, PathRecord>();

    public AStarAlgorithm(GridMarkerController gridMarkerController, IHeuristicEstimate heuristic, float sleepTime = 0.1f) {
        this.gridMarkerController = gridMarkerController;
        this.sleepTime = sleepTime;
        this.heuristic = heuristic;
    }

    public System.Collections.IEnumerator FindPath(Vertex startVertex, Vertex goalVertex, System.Action<List<Edge>> callback = null) {

        openList.Add(startVertex, new PathRecord(startVertex));
        PathRecord current = null;
        #region Not a part of algorithm, visualization purposes only
        gridMarkerController.PutOpenNodeMarker(startVertex);
        #endregion

        while (openList.Count > 0) {
            current = openList.PopMinValue();
            #region Not a part of algorithm, visualization purposes only
            gridMarkerController.PutCurrentNodeMarker(current.node);
            #endregion

            if (current.node == goalVertex) {
                //WOW! we found goal!
                break;
            }
            yield return ProcessAllEdgesFromCurrent(current, goalVertex);
            closeList.Add(current.node, current);
            #region Not a part of algorithm, visualization purposes only
            gridMarkerController.RemoveMarker(current.node);
            gridMarkerController.PutClosedNodeMarker(current.node);
            #endregion
            yield return new WaitForSecondsRealtime(sleepTime);
        }
        List<Edge> path = new List<Edge>();
        if (current.node == goalVertex) {
            while (current.node != startVertex) {
                #region Not a part of algorithm, visualization purposes only
                if (goalVertex != current.node) {
                    gridMarkerController.PutPathNodeMarker(current.node);
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

    private System.Collections.IEnumerator ProcessAllEdgesFromCurrent(PathRecord current, Vertex goalVertex) {
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
                gridMarkerController.RemoveMarker(nextNode.node);
                #endregion
            } else if (openList.ContainsKey(edge.to)) {
                nextNode = openList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                nextNodeHeuristic = nextNode.heuristicValue;
            } else {
                nextNode = new PathRecord(edge.to);
                nextNodeHeuristic = heuristic.Estimate(nextNode.node.pos, goalVertex.pos);
            }

            nextNode.costSoFar = nextNodeCostSoFar;
            nextNode.heuristicValue = nextNodeHeuristic;
            nextNode.connection = new PathConnection(edge, current);
            if (!openList.ContainsKey(nextNode.node)) {
                openList.Add(nextNode.node, nextNode);
                #region Not a part of algorithm, visualization purposes only
                gridMarkerController.PutOpenNodeMarker(nextNode.node);
                #endregion
            }
            yield return new WaitForSecondsRealtime(sleepTime);
        }
    }

    class PathRecord {
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

    class PathConnection {
        public Edge edge;
        public PathRecord fromRecord;

        public PathConnection(Edge edge, PathRecord fromRecord) {
            this.edge = edge;
            this.fromRecord = fromRecord;
        }
    }

    class OpenList {

        IDictionary<Vertex, PathRecord> pathRecords = new Dictionary<Vertex, PathRecord>();
        SimplePriorityQueue<PathRecord> sortedValues = new SimplePriorityQueue<PathRecord>();

        public int Count { get => pathRecords.Count; }

        public void Add(Vertex key, PathRecord value) {
            pathRecords.Add(key, value);
            sortedValues.Enqueue(value, value.GetCostEstimation());
        }

        public void Remove(Vertex key) {
            PathRecord value = pathRecords[key];
            pathRecords.Remove(key);
            sortedValues.Remove(value);
        }

        public PathRecord PopMinValue() {
            PathRecord value = sortedValues.Dequeue();
            pathRecords.Remove(value.node);
            return value;
        }

        public bool ContainsKey(Vertex key) {
            return pathRecords.ContainsKey(key);
        }

        public PathRecord this[Vertex index] {
            get { return pathRecords[index]; }
            set { pathRecords[index] = value; }
        }
    }
}
