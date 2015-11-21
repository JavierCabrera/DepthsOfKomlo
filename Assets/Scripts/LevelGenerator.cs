using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
	public Material groundMaterial;
	public int mapDimensionX;
	public int mapDimensionZ;
	public int numberOfRooms;
	private bool[,] map;
	private int numQuads;
	private int advancedRows;
	private int avgXSizePerRoom;
	private Mesh mesh;
	private Vector3[] vertices;

	// Use this for initialization
	void Awake ()
	{
		InitializeMap ();
	}

	private void InitializeMap ()
	{
		// Start zone initialization
		CreateMetaMap ();
		CreateMapMesh ();
	}

	private void CreateMetaMap ()
	{
		map = new bool[mapDimensionX, mapDimensionZ];
		
		map [0, 0] = true;
		map [0, 1] = true;
		map [1, 0] = true;
		map [1, 1] = true;
		numQuads = 4;
		
		advancedRows = 2;
		avgXSizePerRoom = (int)(mapDimensionX - 2) / numberOfRooms;
		
		for (int i = 0; i < numberOfRooms; i++) {
			AddRoom ();
		}

	}

	private void AddRoom ()
	{
		int roomWidth = avgXSizePerRoom + Random.Range (-1, 1);
		if (roomWidth + advancedRows > mapDimensionX) {
			roomWidth = mapDimensionX - advancedRows;
		}

		int zSize = Random.Range (2, Mathf.Min(mapDimensionZ, roomWidth * 4));

		// Get first and last spot on the room filled
		int first = -1,
		last = -1;
		for (int i = 0; i < mapDimensionZ; i++) {
			if (map [advancedRows - 1, i] && first == -1) {
				first = i;
			}
			if (!map [advancedRows - 1, i] && first != -1) {
				last = i;
				break;
			}
		}

		// Compute the placement for the boundaries
		// Compute the range for the first corner
		// We need to assure an overlap between contiguous rooms of at least 2 tiles
		// to be able to move around
		int fCornerMin, fCornerMax, firstCornerIndex;
		fCornerMin = (zSize > first + 2) ? 0 : first + 2 - zSize;
		fCornerMax = (last + zSize - 2 > mapDimensionZ) ? mapDimensionZ - zSize : last - 2;

		firstCornerIndex = Random.Range (fCornerMin, fCornerMax);
		int lastCorner = firstCornerIndex + zSize;
		int bottomCorner = advancedRows + roomWidth;

		for (int i = firstCornerIndex; i < lastCorner; i++) {
			for (int j = advancedRows; j < bottomCorner; j++) {
				map [j, i] = true;
			}
		}

		advancedRows += roomWidth;
		numQuads += lastCorner * bottomCorner;
	}

	private void CreateMapMesh ()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter> ();
		if (meshFilter == null) {
			meshFilter = gameObject.AddComponent<MeshFilter> ();
		}
		MeshRenderer renderer = GetComponent<MeshRenderer> ();
		if (renderer == null) {
			renderer = gameObject.AddComponent<MeshRenderer> ();
		}
		MeshCollider collider = GetComponent<MeshCollider> (); 
		if (collider == null) {
			collider = gameObject.AddComponent<MeshCollider> ();
		}

		renderer.material = groundMaterial;
		meshFilter.mesh = collider.sharedMesh = mesh = new Mesh ();
		mesh.name = "Level";

		vertices = new Vector3[(mapDimensionX + 1) * (mapDimensionZ + 1)];
		Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4 (1f, 0f, 0f, -1f);
		for (int i = 0, z = 0; z <= mapDimensionZ; z++) {
			for (int x = 0; x <= mapDimensionX; x++, i++) {
				vertices [i] = new Vector3 (x, 0, z);
				uv [i] = new Vector2 (x, z);
				tangents [i] = tangent;
			}
		}
		
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.tangents = tangents;

		// Generate triangle set
		int [] triangles = new int[numQuads * 6];
		for (int ti = 0, vi = 0, z = 0; z < mapDimensionZ; vi++, z++) {
			for (int x = 0; x < mapDimensionX; x++, vi++) {
				if (map [x, z]) {
					triangles [ti] = vi;
					triangles [ti + 3] = triangles [ti + 2] = vi + 1;
					triangles [ti + 4] = triangles [ti + 1] = vi + mapDimensionX + 1;
					triangles [ti + 5] = vi + mapDimensionX + 2;
					ti += 6;
				}
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();
	}
}
