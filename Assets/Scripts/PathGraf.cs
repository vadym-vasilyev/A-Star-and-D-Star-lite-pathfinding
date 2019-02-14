using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapGraf : IVertexOperations {
    Dictionary<Vector2Int, Vertex> grafData = new Dictionary<Vector2Int, Vertex>();

    public void InitNodeMap() {
        grafData.Clear();
    }

    public void AddNode(Vertex vertex) {
        grafData[vertex.pos] = vertex;
    }

    public void AddEdge(Vertex nodeFrom, Vertex nodeTo, float cost) {
        Edge edge = new Edge(cost, nodeFrom, nodeTo);
        nodeFrom.edges.Add(edge);
    }

    public Vertex GetVertexAtPos(Vector2Int pos) {
        Vertex vertex;
        grafData.TryGetValue(pos, out vertex);
        return vertex;
    }

    public List<Vertex> GetVertextPredecessors(Vertex vertex) {
        return grafData.Values.Where(v => (v.edges.Exists(e => e.to == vertex))).ToList();
    }

    public List<Vertex> GetVertextSuccessors(Vertex vertex) {
        return vertex.edges.Select(e => e.to).ToList();
    }
}

public class Vertex {
    readonly public Vector2Int pos;
    public float cost;
    public bool blocked;
    public List<Edge> edges;

    public Vertex(Vector2Int pos) {
        this.pos = pos;
    }

    public Vertex(Vector2Int pos, float cost, bool blocked, List<Edge> edges) : this(pos) {
        this.cost = cost;
        this.blocked = blocked;
        this.edges = edges;
    }

    public override bool Equals(object obj) {
        var vertex = obj as Vertex;
        return vertex != null &&
               pos.Equals(vertex.pos);
    }

    public override int GetHashCode() {
        return 991532785 + EqualityComparer<Vector2Int>.Default.GetHashCode(pos);
    }

    public override string ToString() {
        return "Pos: " + pos;
    }
}

public class Edge {
    public float cost;
    public Vertex from;
    public Vertex to;

    public Edge(float cost, Vertex from, Vertex to) {
        this.cost = cost;
        this.from = from;
        this.to = to;
    }

    public override bool Equals(object obj) {
        var edge = obj as Edge;
        return edge != null &&
               cost == edge.cost &&
               EqualityComparer<Vertex>.Default.Equals(from, edge.from) &&
               EqualityComparer<Vertex>.Default.Equals(to, edge.to);
    }

    public override int GetHashCode() {
        var hashCode = 1170585241;
        hashCode = hashCode * -1521134295 + cost.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(from);
        hashCode = hashCode * -1521134295 + EqualityComparer<Vertex>.Default.GetHashCode(to);
        return hashCode;
    }
}