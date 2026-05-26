using System;
using System.Collections;
using System.Collections.Generic;
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
}