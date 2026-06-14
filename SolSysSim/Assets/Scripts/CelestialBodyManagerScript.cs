using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CelestialBodyManagerScript : MonoBehaviour
{
    public int testIterations = 1;
    private CelestialBody test;

    void Start()
    {
        test = new CelestialBody(1, gameObject);
        test.generate(testIterations);
    }
    
    void FixedUpdate()
    {
        test.generate(testIterations);
    }
}

public class CelestialBody
{
    private GameObject spawner;

    private double shortSideIco;
    private double longSideIco;
    
    public CelestialBody(float scale, GameObject spawner)
    {
        this.shortSideIco = scale;
        this.longSideIco = 1.618033988749 * scale; //golden ratio between sides

        this.spawner = spawner;
    }

    public void generate(int iterations)
    {
        float a = (float) (shortSideIco / 2);
        float b = (float) (longSideIco / 2);

        Vector3[] icosahedronVertices = 
        {
            //green
            new Vector3(-a, 0, -b),
            new Vector3(a, 0, -b),
            new Vector3(-a, 0, b),
            new Vector3(a, 0, b),

            //blue
            new Vector3(0, b, -a), 
            new Vector3(0, b, a), 
            new Vector3(0, -b, -a),
            new Vector3(0, -b, a),

            //red
            new Vector3(-b, -a, 0), 
            new Vector3(-b, a, 0), 
            new Vector3(b, a, 0),
            new Vector3(b, -a, 0)
        };

        int[] icosahedronTriangles =
        {
            0, 4, 1,
            1, 4, 10,
            10, 4, 5,
            5, 4, 9,
            9, 4, 0, 
            11, 1, 10, 
            6, 1, 11,
            6, 0, 1,
            6, 8, 0, 
            7, 8, 6,
            7, 6, 11,
            11, 10, 3,
            3, 10, 5,
            3, 5, 2,
            2, 5, 9,
            9, 8, 2,
            7, 2, 8, 
            3, 2, 7,
            11, 3, 7,
            8, 9, 0
        };

        List<Vector3> icosphereVertices = new List<Vector3>();        
        List<int> icosphereTriangles = new List<int>();

        for(int i = 0; i < icosahedronTriangles.Length; i++)
        {
            icosphereVertices.Add(icosahedronVertices[icosahedronTriangles[i]]);
        }

        for(int i = 0; i < iterations; i++)
        {
            List<Vector3> newVertices = new List<Vector3>();
            
            for(int j = 0; j < icosphereVertices.Count; j += 3)
            {
                Vector3 v0 = icosphereVertices[j];
                Vector3 v1 = icosphereVertices[j+1];
                Vector3 v2 = icosphereVertices[j+2];
                Vector3 v3 = Vector3.Slerp(v0, v1, 0.5f);
                Vector3 v4 = Vector3.Slerp(v1, v2, 0.5f);
                Vector3 v5 = Vector3.Slerp(v2, v0, 0.5f);

                // 4 triangles
                newVertices.Add(v0);
                newVertices.Add(v3);
                newVertices.Add(v5);
                
                newVertices.Add(v3);
                newVertices.Add(v1);
                newVertices.Add(v4);
                
                newVertices.Add(v5);
                newVertices.Add(v4);
                newVertices.Add(v2);
                
                newVertices.Add(v5);
                newVertices.Add(v3);
                newVertices.Add(v4);
            }
            
            icosphereVertices = newVertices;
        }

        for(int i = 0; i < icosphereVertices.Count; i++)
        {
            icosphereTriangles.Add(i);
        }

        Mesh mesh = new Mesh
        {
            vertices = icosphereVertices.ToArray(),
            triangles = icosphereTriangles.ToArray()
        };

        Debug.Log(icosphereVertices.Count);
        Debug.Log(icosphereTriangles.Count);

        spawner.GetComponent<MeshFilter>().mesh = mesh;
    } 

    public static float perlin(float x, float y)
    {
        // For a given point, calculate the dot products from the 4 surrounding points between their distance and random vectors

        //topleft: x0, y1
        //topright: x1, y1
        //bottomleft: x0, y0
        //bottomright: x1, y0
        float x0 =(int)x;
        float x1 = x0 + 1;
        float y0 =(int)y;
        float y1 = y0 + 1;

        float topleft = dotProductDistanceRandom(x, y, x0, y1); 
        float topright = dotProductDistanceRandom(x, y, x1, y1);
        float bottomleft = dotProductDistanceRandom(x, y, x0, y0);
        float bottomright = dotProductDistanceRandom(x, y, x1, y0);

        // Then linear interpolate (w/ smoothstep) the top and bottom 2
        //try replacing with own lerp + smoothstep if doesn't work, this implementation is for clarity 
        //(bc i am a dummy who will forget how all this works) 
        //formula: (a1 - a0) * (3.0 - w * 2.0) * w * w + a0; 
        float top = Mathf.Lerp(topleft, topright, Mathf.SmoothStep(0, 1, x - x0));
        float bottom = Mathf.Lerp(bottomleft, bottomright, Mathf.SmoothStep(0, 1, x - x0));

        // Then linear interpolate (w/ smoothstep) the 2 resulting numbers to get the value 
        return Mathf.Lerp(bottom, top, Mathf.SmoothStep(0, 1, y - y0));                           
    }

    public static float perlin(float x, float y, float z)
    {
        float xy = perlin(x, y);
        float yz = perlin(y, z);
        float xz = perlin(x, z);
        float yx = perlin(y, x);
        float zy = perlin(z, y);
        float zx = perlin(z, x);

        return (xy + yz + xz + yx + zy + zx) % 6;
    }

    private static float dotProductDistanceRandom(float x, float y, float cx, float cy)
    {
        //here goes the line for the random vectors (buncha random bs -- might not work)
        double angle = (cx * 49632 + cy * 325176 + 12345) % (2 * Math.PI);
        double rx = Math.Cos(angle);
        double ry = Math.Sin(angle);

        //and here goes the distance vectors
        float dx = x - cx;
        float dy = y - cy;

        // Dot product formula (algebraic) : a.x * b.x + a.y * b.y for 2d
        return dx * (float)rx + dy * (float)ry;
    }
}