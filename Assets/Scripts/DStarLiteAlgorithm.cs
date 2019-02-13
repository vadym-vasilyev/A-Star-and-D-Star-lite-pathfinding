using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using System.Linq;
using System;

public class DStarLiteAlgorithm {
    GridMarkerController gridMarkerController;
    IVertextPositionResolver positionResolver;

    IPriorityQueue<PathRecord, Key> openList = new SimplePriorityQueue<PathRecord, Key>();
    Dictionary<Vertex, PathRecord> pathRecords = new Dictionary<Vertex, PathRecord>();
    IHeuristicEstimate heuristicEstimate;

    int maxSteps = 1000;

    PathRecord startNode = null;
    PathRecord goalNode = null;

    float timeDelay;
    float k = 0;
    bool pathFound;


    public DStarLiteAlgorithm(GridMarkerController gridMarkerController, IVertextPositionResolver positionResolver, IHeuristicEstimate heuristicEstimate, float timeDelay = 1.0f) {
        this.gridMarkerController = gridMarkerController;
        this.heuristicEstimate = heuristicEstimate;
        this.positionResolver = positionResolver;
        this.timeDelay = timeDelay;
    }

    private Key CalculateKey(PathRecord pathRecord) {
        float minPath = Mathf.Min(pathRecord.costSoFar, pathRecord.rightHandSide);
        float heuristic = heuristicEstimate.Estimate(positionResolver.GetPosForVertex(pathRecord.node), positionResolver.GetPosForVertex(startNode.node));
        return new Key(minPath + heuristic + k, minPath);
    }

    private List<PathRecord> GetPredecessors(PathRecord pathRecord) {
        List<Vertex> predecessorNodes = positionResolver.GetVertextPredecessors(pathRecord.node);
        return predecessorNodes.Select(n => GetOrCreateIfAbsent(n)).ToList();
    }

    private List<PathRecord> GetSuccecessors(PathRecord pathRecord) {
        List<Vertex> succecessorsNodes = positionResolver.GetVertextSuccessors(pathRecord.node);
        return succecessorsNodes.Select(n => GetOrCreateIfAbsent(n)).ToList();
    }

    private PathRecord GetOrCreateIfAbsent(Vertex node) {
        PathRecord predecessor;
        if (!pathRecords.TryGetValue(node, out predecessor)) {
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

    private void ProcessUnderconsistentNode(PathRecord currentNode) {
        float costSoFarCurrent = currentNode.costSoFar;
        currentNode.costSoFar = float.PositiveInfinity;
        List<PathRecord> predecessorPathRecords = GetPredecessors(currentNode);
        predecessorPathRecords.Add(currentNode);
        predecessorPathRecords.ForEach(p => {
            if (p != goalNode && p.rightHandSide == costSoFarCurrent + GetCost(p, currentNode)) {
                List<PathRecord> succecessors = GetSuccecessors(p);
                p.rightHandSide = succecessors.Min(s => s.costSoFar + GetCost(p, s));
            }
            UpdateVertex(p);
        });
    }

    private void ProcessOverconsistentNode(PathRecord currentNode) {
        currentNode.costSoFar = currentNode.rightHandSide;
        List<PathRecord> predecessorPathRecords = GetPredecessors(currentNode);
        predecessorPathRecords.ForEach(p => {
            if (p != goalNode) {
                p.rightHandSide = Mathf.Min(p.rightHandSide, currentNode.costSoFar + GetCost(p, currentNode));
            }
            UpdateVertex(p);
        });
    }

    private float GetCost(PathRecord from, PathRecord to) {
        return GetCost(from.node, to.node);
    }

    private float GetCost(Vertex from, Vertex to) {
        return from.edges.First(e => e.to == to).cost;
    }

    public void UpdatePath(Vertex vertextOld, Vertex updatedVertex) {
        PathRecord changedPathRecord = null;
        if (pathRecords.TryGetValue(vertextOld, out changedPathRecord)) {
            if (changedPathRecord != goalNode) {
                changedPathRecord.node = updatedVertex;
                List<PathRecord> successors = GetSuccecessors(changedPathRecord);
                changedPathRecord.rightHandSide = successors.Min(s => s.costSoFar + GetCost(changedPathRecord, s));
            }
            UpdateVertex(changedPathRecord);
        };

    }

    public IEnumerator ComputeShortestPath(System.Action<List<Edge>> pathFind = null) {

        PathRecord currentNode;
        pathFound = false;
        while (openList.First.key < CalculateKey(startNode) || startNode.rightHandSide > startNode.costSoFar) {
            if (maxSteps-- == 0) {
                Debug.Log("can't find path");
                yield break;
            }
            currentNode = openList.Dequeue();

            #region Not a part of algorithm, visualization purposes only
            gridMarkerController.PutCurrentNodeMarker(currentNode.node);
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
        pathFound = true;
    }

    public IEnumerator MoveByPath(Vertex starVertex, Vertex goalVertex) {

        startNode = new PathRecord(starVertex);
        startNode.costSoFar = startNode.rightHandSide = float.PositiveInfinity;
        pathRecords.Add(startNode.node, startNode);

        goalNode = new PathRecord(goalVertex);
        pathRecords.Add(goalNode.node, goalNode);
        goalNode.key = CalculateKey(goalNode);
        openList.Enqueue(goalNode, goalNode.key);

        yield return ComputeShortestPath();

        if (pathFound) {

            PathRecord currentNode = startNode;
            PathRecord nextNode = currentNode;

            #region Not a part of algorithm, visualization purposes only
            gridMarkerController.PutPathNodeMarker(currentNode.node);
            yield return new WaitForSeconds(timeDelay);
            #endregion

            while (nextNode != goalNode && !pathFound) {
                List<PathRecord> succecessors = GetSuccecessors(nextNode);
                nextNode = succecessors.Aggregate((s1, s2) => s1.costSoFar + GetCost(nextNode, s1) < s2.costSoFar + GetCost(nextNode, s2) ? s1 : s2);
                #region Not a part of algorithm, visualization purposes only
                gridMarkerController.PutPathNodeMarker(nextNode.node);
                yield return new WaitForSeconds(timeDelay);
                #endregion

                if (float.IsPositiveInfinity(nextNode.costSoFar) || Mathf.Abs(nextNode.rightHandSide - (GetCost(currentNode, nextNode) + nextNode.costSoFar)) > Mathf.Epsilon) {
                    k += heuristicEstimate.Estimate(positionResolver.GetPosForVertex(currentNode.node), positionResolver.GetPosForVertex(startNode.node));
                    startNode = currentNode;
                    yield return ComputeShortestPath();
                } else {
                    currentNode = nextNode;
                }
            }
        }
    }
    class PathRecord {
        public Vertex node;
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

    class Key : IComparable<Key> {
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
