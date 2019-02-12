using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;
using System;

public class DStarLiteAlgorithm {
    readonly GridController gridController;
    IPriorityQueue<PathRecord, Key> openList = new SimplePriorityQueue<PathRecord, Key>();
    Dictionary<Vertex, PathRecord> pathRecords = new Dictionary<Vertex, PathRecord>();
    IHeuristicEstimate heuristicEstimate;

    int maxSteps = 1000;

    PathRecord startNode = null;
    PathRecord goalNode = null;

    float timeDelay;
    float k = 0;

    public DStarLiteAlgorithm(GridController gridController, IHeuristicEstimate heuristicEstimate, float timeDelay = 1.0f) {
        this.gridController = gridController;
        this.heuristicEstimate = heuristicEstimate;
        startNode = new PathRecord(gridController.GetStartNodeVertex());
        startNode.costSoFar = startNode.rightHandSide = float.PositiveInfinity;
        pathRecords.Add(startNode.node, startNode);

        goalNode = new PathRecord(gridController.GetGoalNodeVertex());
        pathRecords.Add(goalNode.node, goalNode);
        goalNode.key = CalculateKey(goalNode);
        openList.Enqueue(goalNode, goalNode.key);
        this.timeDelay = timeDelay;
    }

    private Key CalculateKey(PathRecord pathRecord) {
        float minPath = Mathf.Min(pathRecord.costSoFar, pathRecord.rightHandSide);
        float heuristic = heuristicEstimate.Estimate(gridController.GetPosForVertex(pathRecord.node), gridController.GetPosForVertex(startNode.node));
        return new Key(minPath + heuristic + k, minPath);
    }

    private List<PathRecord> GetPredecessors(PathRecord pathRecord) {
        List<Vertex> predecessorNodes = gridController.GetVertextPredecessors(pathRecord.node);
        return predecessorNodes.Select(n => GetOrCreateIfAbsent(n)).ToList();
    }

    private List<PathRecord> GetSuccecessors(PathRecord pathRecord) {
        List<Vertex> succecessorsNodes = gridController.GetVertextSuccessors(pathRecord.node);
        return succecessorsNodes.Select(n => GetOrCreateIfAbsent(n)).ToList();
    }

    private PathRecord GetOrCreateIfAbsent(Vertex node) {
        PathRecord predecessor;
        pathRecords.TryGetValue(node, out predecessor);
        if (predecessor == null) {
            predecessor = new PathRecord(node);
            predecessor.costSoFar = float.PositiveInfinity;
            predecessor.rightHandSide = float.PositiveInfinity;
            pathRecords.Add(node, predecessor);
        }
        return predecessor;
    }

    private void UpdateVertex(PathRecord pathRecord) {
        if (openList.Contains(pathRecord)) {
            if (Mathf.Abs(pathRecord.costSoFar - pathRecord.rightHandSide) > Mathf.Epsilon) {
                pathRecord.key = CalculateKey(pathRecord);
                openList.UpdatePriority(pathRecord, pathRecord.key);
            } else {
                openList.Remove(pathRecord);
            }
        } else if (Mathf.Abs(pathRecord.costSoFar - pathRecord.rightHandSide) > Mathf.Epsilon) {
            pathRecord.key = CalculateKey(pathRecord);
            openList.Enqueue(pathRecord, pathRecord.key);
        }
    }

    public IEnumerator ComputeShortestPath() {

        PathRecord currentNode;

        while (openList.First.key < CalculateKey(startNode) || startNode.rightHandSide > startNode.costSoFar) {
            if (maxSteps-- == 0) {
                Debug.Log("can't find path");
                break;
            }
            currentNode = openList.Dequeue();

            #region Not a part of algorithm, visualization purposes only
            gridController.PutOpenNodeMarker(currentNode.node);
            yield return new WaitForSeconds(timeDelay);
            #endregion

            Key keyOld = currentNode.key;
            Key keyNew = CalculateKey(currentNode);
            if (keyOld < keyNew) {
                currentNode.key = keyNew;
                openList.Enqueue(currentNode, currentNode.key);
            } else if (currentNode.costSoFar > currentNode.rightHandSide) {
                ProcessOverconsistentNode(currentNode);
            } else {
                ProcessUnderconsistentNode(currentNode);
            }
        }
    }

    private void ProcessUnderconsistentNode(PathRecord currentNode) {
        float costSoFarCurrent = currentNode.costSoFar;
        currentNode.costSoFar = float.PositiveInfinity;
        pathRecords[currentNode.node] = currentNode;
        List<PathRecord> predecessorPathRecords = GetPredecessors(currentNode);
        predecessorPathRecords.Add(currentNode);
        predecessorPathRecords.ForEach(p => {
            if (p != goalNode && p.rightHandSide == costSoFarCurrent + p.node.edges.First(e => e.to == currentNode.node).cost) {
                List<PathRecord> succecessors = GetSuccecessors(p);
                p.rightHandSide = succecessors.Min(s => s.costSoFar + p.node.edges.First(e => e.to == currentNode.node).cost);
                pathRecords[p.node] = p;
            }
            UpdateVertex(p);
        });
    }

    private void ProcessOverconsistentNode(PathRecord currentNode) {
        currentNode.costSoFar = currentNode.rightHandSide;
        pathRecords[currentNode.node] = currentNode;
        List<PathRecord> predecessorPathRecords = GetPredecessors(currentNode);
        predecessorPathRecords.ForEach(p => {
            if (p != goalNode) {
                p.rightHandSide = Mathf.Min(p.rightHandSide, currentNode.costSoFar + p.node.edges.First(e => e.to == currentNode.node).cost);
                pathRecords[p.node] = p;
            }
            UpdateVertex(p);
        });
    }

    public class PathRecord {
        public Vertex node;
        public PathConnection connection;
        public float costSoFar;
        public float rightHandSide;
        public Key key;

        public PathRecord(Vertex node) {
            this.node = node;
        }

        public override bool Equals(object obj) {
            var record = obj as PathRecord;
            return record != null && EqualityComparer<Vertex>.Default.Equals(node, record.node);
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

    public class Key : IComparable<Key> {
        float k1;
        float k2;

        public Key(float k1, float k2) {
            this.k1 = k1;
            this.k2 = k2;
        }

        public int CompareTo(Key other) {
            if (this < other) {
                return -1;
            }
            return 1;
        }

        public override string ToString() {
            return "[" + k1 + ":" + k2 + "]";
        }

        public static bool operator <(Key a, Key b) {
            if (a.k1 < b.k1) {
                return true;
            } else if (Mathf.Abs(a.k1 - b.k1) < Mathf.Epsilon && a.k2 < b.k2) {
                return true;
            } else {
                return false;
            }
        }

        public static bool operator >(Key a, Key b) {
            return b < a;
        }
    }
}
