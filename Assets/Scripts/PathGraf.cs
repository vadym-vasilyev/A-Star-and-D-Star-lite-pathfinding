using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilemapGraf {
    Dictionary<Vector2Int, Vertex> grafData = new Dictionary<Vector2Int, Vertex>();

    public void InitNodeMap() {
        grafData.Clear();
    }

    public void AddNode(Vector2Int pos, Vertex vertex) {
        grafData[pos] = vertex;
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

    public Vector2Int GetPositionByVertex(Vertex vertex) {
        var pos = grafData.FirstOrDefault(x => x.Value == vertex).Key;
        return pos;
    }

    public List<Vertex> GetVertextPredecessors(Vertex vertex) {
        return grafData.Values.Where(v => (v.edges.Exists(e => e.to == vertex))).ToList();
    }

    public List<Vertex> GetVertextSuccessors(Vertex vertex) {
        return vertex.edges.Select(e => e.to).ToList();
    }
}

public class Vertex {

    public float cost;
    public bool blocked;
    public List<Edge> edges;

    public Vertex(float cost, bool blocked, List<Edge> edges) {
        this.cost = cost;
        this.blocked = blocked;
        this.edges = edges;
    }

    public override bool Equals(object obj) {
        var vertex = obj as Vertex;
        return vertex != null &&
               cost == vertex.cost &&
               blocked == vertex.blocked &&
               EqualityComparer<List<Edge>>.Default.Equals(edges, vertex.edges);
    }

    public override int GetHashCode() {
        var hashCode = -1898663744;
        hashCode = hashCode * -1521134295 + cost.GetHashCode();
        hashCode = hashCode * -1521134295 + blocked.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Edge>>.Default.GetHashCode(edges);
        return hashCode;
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