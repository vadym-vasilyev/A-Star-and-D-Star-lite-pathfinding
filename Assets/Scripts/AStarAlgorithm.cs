using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System;
using System.Linq;

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
        gridController.CreateGraf();

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
            float endNodeHeuristic;

            if (closeList.ContainsKey(edge.to)) {
                Debug.Log("Contains close list " + gridController.GetPosForVertex(edge.to));
                nextNode = closeList[edge.to];
                if (nextNode.costSoFar <= nextNodeCostSoFar) {
                    continue;
                }
                endNodeHeuristic = nextNode.etimatedCost - nextNode.costSoFar;
                closeList.Remove(nextNode.node);
                #region Not a part of algorithm, visualization purposes only
                gridController.RemoveMarker(nextNode.node);
                #endregion
            } else if (openList.ContainsKey(edge.to)) {
                Debug.Log("Contains open list " + gridController.GetPosForVertex(edge.to));
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
    public float etimatedCost;

    public PathRecord(Vertex node) {
        this.node = node;
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


class OpenList {
    IDictionary<Vertex, PathRecord> forward = new Dictionary<Vertex, PathRecord>();
    SimplePriorityQueue<PathRecord> priorityQueue = new SimplePriorityQueue<PathRecord>();

    public int Count { get => forward.Count; }

    public void Add(Vertex key, PathRecord value) {
        forward.Add(key, value);
        priorityQueue.Enqueue(value, value.etimatedCost);
    }

    public void Remove(Vertex key) {
        PathRecord Value = forward[key];
        forward.Remove(key);
        priorityQueue.Remove(Value);
    }

    public PathRecord PopMinValue() {
        PathRecord value = priorityQueue.Dequeue();
        forward.Remove(value.node);
        return value;
    }

    public bool ContainsKey(Vertex key) {
        return forward.ContainsKey(key);
    }

    public PathRecord this[Vertex index] {
        get { return forward[index]; }
        set { forward[index] = value; }
    }
}
