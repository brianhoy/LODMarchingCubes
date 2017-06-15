﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCCreator : MonoBehaviour {
	public GameObject MeshPrefab;

	private List<GameObject> Meshes;
	private List<ExtractionResult> results;

	// Use this for initialization
	void Start () {
		Meshes = new List<GameObject>();
		results = new List<ExtractionResult>();

		int lods = 2;
		int startingSize = 8;
		int runningOffset = 0;
	

		for(int i = 0; i < lods; i++) {
			ExtractionInput input = new ExtractionInput();
			input.Isovalue = 0;
			int res = 8; //(int)((float)startingSize / Mathf.Pow(2f, i));
			input.Resolution = new Util.Vector3i(res, res, res);
			print("resolution" + input.Resolution.z);
			int size = (int)Mathf.Pow(2, i);
			input.LODSides = 10;
			input.Size = new Vector3(size, size, size);
			runningOffset += size * 4;
			Vector3 off = new Vector3(runningOffset, 0, 0);
			input.Sample = (float x, float y, float z) => UtilFuncs.Sample(x + off.x, y + off.y, z + off.z);;

			results.Add(SurfaceExtractor.ExtractSurface(input));
			results[results.Count - 1].Offset = off;
		} // 1 2	4 8		16 32

		foreach(ExtractionResult r in results) {
			CreateMesh(r);
		}
	}
	
	void CreateMesh(ExtractionResult r) {
		GameObject isosurfaceMesh = Instantiate(MeshPrefab, r.Offset, Quaternion.identity);
		Meshes.Add(isosurfaceMesh);

		Material mat = isosurfaceMesh.GetComponent<Renderer>().materials[0];
		MeshFilter mf = isosurfaceMesh.GetComponent<MeshFilter>();
		MeshCollider mc = isosurfaceMesh.GetComponent<MeshCollider>();

		mf.mesh.vertices = r.Mesh.vertices;
		mf.mesh.triangles = r.Mesh.triangles;
		mc.sharedMesh = mf.mesh;
		//if(m.normals != null) mf.mesh.normals = m.normals;
		mf.mesh.RecalculateNormals();
		mf.mesh.RecalculateBounds();
	}

	// Update is called once per frame
	void OnDrawGizmos() {
		DrawGridCells();
	}

	void DrawGridCells() {
		foreach(ExtractionResult r in results) {
			Gizmos.color = Color.gray;
			foreach(Util.GridCell c in r.Cells) {
				DrawGridCell(c, r.Offset);
			}
			Gizmos.color = Color.yellow;
			foreach(Util.GridCell c in r.DebugTransitionCells1S) {
				DrawGridCell(c, r.Offset);
			}
			Gizmos.color = Color.red;
			foreach(Util.GridCell c in r.DebugTransitionCells2S) {
				DrawGridCell(c, r.Offset);
			}
			Gizmos.color = Color.magenta;
			foreach(Util.GridCell c in r.DebugTransitionCells3S) {
				DrawGridCell(c, r.Offset);
			}
		}
	}

	void DrawGridCell(Util.GridCell c, Vector3 offset) {
		for(int i = 0; i < c.points.Length; i++) {
			Gizmos.DrawCube(c.points[i].position + offset, 0.1f * Vector3.one);
		}
		for(int i = 0; i < 12; i++) {
			Gizmos.DrawLine(c.points[edges[i,0]].position + offset, c.points[edges[i,1]].position + offset);
		}
	}

	// [edgeNum] = [corner1, corner2]
	public static readonly int[,] edges = {
		{4, 7}, {0, 3}, {5, 6}, {1, 2}, {4, 5}, {0, 1}, {7, 6}, {3, 2}, {0, 4}, {5, 1}, {7, 3}, {2, 6}
	};
}
/*
Vertex and Edge Index Map
		
        7-------6------6
       /.             /|
      10.           11 |
     /  0           /  2
    /   .          /   |     ^ Y
   3-------7------2    |     |
   |    4 . . 4 . |. . 5     --> X
   |   .          |   /		 \/ -Z
   1  8           3  9
   | .            | /
   |.             |/
   0-------5------1
*/