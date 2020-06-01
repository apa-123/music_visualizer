using UnityEngine;
using System.Collections.Generic;
 
using csDelaunay;
 
// This class was written by the library creator, we modified it to fit our desired results 
public class VoronoiDiagram : MonoBehaviour {
 
    // The number of polygons/sites we want
    public int polygonNumber = 200;
 
    // This is where we will store the resulting data
    public Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    Voronoi voronoi;
    int count;
    
    void Start() {
        // Scale the quad 
        Vector3 temp = transform.localScale;
        temp.x = 25.0f;
        temp.y = 25.0f;
        transform.localScale = temp;

        // Modify the position of the model
        Vector3 pos = transform.localPosition;
        pos.z = 3;
        transform.localPosition = pos;

        // Create your sites (lets call that the center of your polygons)
        List<Vector2f> points = CreateRandomPoint();
       
        // Create the bounds of the voronoi diagram
        // Use Rectf instead of Rect; it's a struct just like Rect and does pretty much the same,
        // but like that it allows you to run the delaunay library outside of unity (which mean also in another tread)
        Rectf bounds = new Rectf(0,0,512,512);
       
        // There is a two ways you can create the voronoi diagram: with or without the lloyd relaxation
        // Here I used it with 2 iterations of the lloyd relaxation
        voronoi = new Voronoi(points,bounds);
 
        // Now retreive the edges from it, and the new sites position if you used lloyd relaxtion
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
 
        DisplayVoronoiDiagram();
    }
   
   // Created to generate a Voronoi animation with Lloyd relaxation applied 
    void Update() {
        voronoi.LloydRelaxation(count);
        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
 
        DisplayVoronoiDiagram();

        count++;
        count = count%10;

        if (count == 1) {
            List<Vector2f> points = CreateRandomPoint();
            Rectf bounds = new Rectf(0,0,512,512);
            voronoi = new Voronoi(points,bounds);
        }
    }

    private List<Vector2f> CreateRandomPoint() {
        // Use Vector2f, instead of Vector2
        // Vector2f is pretty much the same than Vector2, but like you could run Voronoi in another thread
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++) {
            points.Add(new Vector2f(Random.Range(0,512), Random.Range(0,512)));
        }
 
        return points;
    }
 
    // Here is a very simple way to display the result using a simple bresenham line algorithm
    // Just attach this script to a quad
    private void DisplayVoronoiDiagram() {
        Texture2D tx = new Texture2D(512,512);
        foreach (KeyValuePair<Vector2f,Site> kv in sites) {
            tx.SetPixel((int)kv.Key.x, (int)kv.Key.y, Color.black);
        }
        foreach (Edge edge in edges) {
            // if the edge doesn't have clippedEnds, if was not within the bounds, dont draw it
            if (edge.ClippedEnds == null) continue;
            Color random = new Color(
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f), 
                Random.Range(0f, 1f)
            );
            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, random);
        }
        tx.Apply();
 
        this.GetComponent<Renderer>().material.mainTexture = tx;
    }
 
    // Bresenham line algorithm
    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0) {
        int x0 = (int)p0.x;
        int y0 = (int)p0.y;
        int x1 = (int)p1.x;
        int y1 = (int)p1.y;
       
        int dx = Mathf.Abs(x1-x0);
        int dy = Mathf.Abs(y1-y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx-dy;
       
        while (true) {
            tx.SetPixel(x0+offset,y0+offset,c);
           
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2*err;
            if (e2 > -dy) {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx) {
                err += dx;
                y0 += sy;
            }
        }
    }
}      