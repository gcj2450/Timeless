﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LibNoise.Unity;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;

// Generate a sphere out of a cube
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetGenerator : MonoBehaviour {

    public bool hideTrees = false;

    public Planet planet;
    public GameObject waterRef;
    public GameObject[] treeRefs;
    public UnityEngine.Gradient coloring;

	private Vector3[] vertices;
	private Vector3[] normals;
    private int[] trianglesX;
    private int[] trianglesY;
    private int[] trianglesZ;
    private Color[] colors;

    private List<Vector3> spawnPoints = new List<Vector3>();

    void Awake(){
        planet.perlin = new Perlin();
    }
    void Start(){
		Generate();

        if ( spawnPoints.Count > 0 && GameObject.FindWithTag("Player") != null ){
            GameObject.FindWithTag("Player").transform.position = spawnPoints[Random.Range(0,spawnPoints.Count)];
        }
    }

    // Generate the mesh
	private void Generate () {
        CreateVertices(planet.gridSize);
        CreateTriangles(planet.gridSize);

        Mesh m = new Mesh();
        m.name = "planet";

        m.vertices = vertices;
        m.subMeshCount = 3;
        m.SetTriangles(trianglesZ, 0);
        m.SetTriangles(trianglesX, 1);
        m.SetTriangles(trianglesY, 2);

        m.colors = colors;
        m.normals = normals;

        m.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = m;
        GetComponent<MeshCollider>().sharedMesh = m;

        CreateGravity();
        CreateWater();
        CreateSpawnPoints();
        CreateTrees();

        Debug.Log("World generated.");
	}

    // Create trees
    private void CreateTrees(){
        if ( treeRefs.Length < 1 || hideTrees ) return;

        int treeSpawnCount = Mathf.RoundToInt(planet.radius*planet.height*planet.gridSize*planet.treeDensity);

        Transform treesParent = new GameObject("Trees").transform;
        treesParent.transform.SetParent(transform);
        treesParent.gameObject.isStatic = true;

        for (int i = 0; i < treeSpawnCount; i++){
            Vector3 randomPoint = Random.onUnitSphere*planet.radius*planet.height*1.1f;
            Vector3 downVector = planet.origin - randomPoint;

            RaycastHit hit;
            if ( Physics.Raycast(randomPoint, downVector, out hit, planet.radius, 1 << gameObject.layer) ){
                if ( !CloseToSpawnPoint(hit.point) ){
                    Instantiate(treeRefs[Random.Range(0,treeRefs.Length)], hit.point, Quaternion.LookRotation(hit.normal), treesParent);
                }
            }
        }

        Debug.Log("Trees generated.");
    }
    // Create all spawnPoints on planet
    private void CreateSpawnPoints(){
        int spawnPointCount = Random.Range(10,20);
        for (int i = 0; i < spawnPointCount; i++){
            Vector3 randomPoint = Random.onUnitSphere*planet.radius*planet.height*1.1f;
            Vector3 downVector = planet.origin - randomPoint;

            RaycastHit hit;
            Debug.DrawLine(randomPoint, planet.origin, Color.yellow, 1f);
            if ( Physics.Raycast(randomPoint, downVector, out hit, planet.radius, 1 << LayerMask.NameToLayer("Environment")) ){
                 Vector3 pos = hit.point + -downVector.normalized*planet.height;
                 spawnPoints.Add(pos);
            }
        }
        Debug.Log("Created " + spawnPoints.Count + " spawn points.");
    }
    // Create water
    private void CreateWater(){
        GameObject waterObj = Instantiate(waterRef, transform.position, Quaternion.identity, transform);

        float scale = planet.height * planet.radius * 2;
        waterObj.transform.localScale = new Vector3(scale, scale, scale);
    }
    // Apply gravity pull
    private void CreateGravity(){
        GameObject o = new GameObject("Gravity");
        transform.SetParent(o.transform);
        
        SphereCollider col = o.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = planet.gridSize*planet.height*2;

        GravityPull gravityPull = o.AddComponent<GravityPull>();
        gravityPull.gravity = planet.gravity;
    }
    // Create the vertices of the sphere
	private void CreateVertices (int gridSize) {
		int cornerVertices = 8;
		int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
		int faceVertices = (
			(gridSize - 1) * (gridSize - 1) +
			(gridSize - 1) * (gridSize - 1) +
			(gridSize - 1) * (gridSize - 1)) * 2;
		vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
		normals = new Vector3[vertices.Length];
        colors = new Color[vertices.Length];

		int v = 0;
		for (int y = 0; y <= gridSize; y++) {
            for (int x = 0; x <= gridSize; x++) {
				SetVertex(v++, x, y, 0);
			}
			for (int z = 1; z <= gridSize; z++) {
				SetVertex(v++, gridSize, y, z);
			}
			for (int x = gridSize - 1; x >= 0; x--) {
				SetVertex(v++, x, y, gridSize);
			}
			for (int z = gridSize - 1; z > 0; z--) {
				SetVertex(v++, 0, y, z);
			}
		}
		for (int z = 1; z < gridSize; z++) {
			for (int x = 1; x < gridSize; x++) {
				SetVertex(v++, x, gridSize, z);
			}
		}
		for (int z = 1; z < gridSize; z++) {
			for (int x = 1; x < gridSize; x++) {
				SetVertex(v++, x, 0, z);
			}
		}
	}
    // Create the vertex based on vertices count and a given vertex position from a cube
	private void SetVertex (int i, int x, int y, int z) {
		Vector3 v = new Vector3(x, y, z) * 2f / planet.gridSize - Vector3.one;
		float x2 = v.x * v.x;
		float y2 = v.y * v.y;
		float z2 = v.z * v.z;
		Vector3 s;
		s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
		s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
		s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
		normals[i] = s;

        float noise = (float)planet.perlin.GetValue(planet.origin.x+x/planet.radius, planet.origin.y+y/planet.radius, planet.origin.z+z/planet.radius);
		vertices[i] = normals[i] * planet.radius * (planet.height + noise);
        colors[i] = coloring.Evaluate(noise);
	}
    // Create the triangles connecting each vertex to form the mesh
	private void CreateTriangles (int gridSize) {
		trianglesZ = new int[(gridSize * gridSize) * 12];
		trianglesX = new int[(gridSize * gridSize) * 12];
		trianglesY = new int[(gridSize * gridSize) * 12];
		int ring = (gridSize + gridSize) * 2;
		int tZ = 0, tX = 0, tY = 0, v = 0;

		for (int y = 0; y < gridSize; y++, v++) {
			for (int q = 0; q < gridSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize - 1; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = CreateTopFace(trianglesY, tY, ring);
		tY = CreateBottomFace(trianglesY, tY, ring);
	}
    // Create the top face of the spherified cube
	private int CreateTopFace (int[] triangles, int t, int ring) {
		int v = ring * planet.gridSize;
		for (int x = 0; x < planet.gridSize - 1; x++, v++) {
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (planet.gridSize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < planet.gridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + planet.gridSize - 1);
			for (int x = 1; x < planet.gridSize - 1; x++, vMid++) {
				t = SetQuad(triangles, t, vMid, vMid + 1, vMid + planet.gridSize - 1, vMid + planet.gridSize);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + planet.gridSize - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < planet.gridSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}
    // Create the bottom face of the spherified cube
	private int CreateBottomFace (int[] triangles, int t, int ring) {
		int v = 1;
		int vMid = vertices.Length - (planet.gridSize - 1) * (planet.gridSize - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < planet.gridSize - 1; x++, v++, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= planet.gridSize - 2;
		int vMax = v + 2;

		for (int z = 1; z < planet.gridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid + planet.gridSize - 1, vMin + 1, vMid);
			for (int x = 1; x < planet.gridSize - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid + planet.gridSize - 1, vMid + planet.gridSize, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + planet.gridSize - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < planet.gridSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

    // Create the face using the triangles and four vertices
	private static int
	SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}
    // Check if point is within the radius of a spawn point
    private bool CloseToSpawnPoint(Vector3 point){
        foreach (Vector3 spawnPoint in spawnPoints){
            if ( Vector3.Distance(spawnPoint, point) < 20f)
            {
                return true;
            }
        }

        return false;
    }
}