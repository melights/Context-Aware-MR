/* *****************************************************************************
 * 
 *								EDUCATION RESEARCH GROUP
 *							MORGRIDGE INSTITUTE FOR RESEARCH
 * 			
 * 				
 * Copyright (c) 2012 EDUCATION RESEARCH, MORGRIDGE INSTITUTE FOR RESEARCH
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated  * documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
 * SOFTWARE.
 *  
 * 
 ******************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// APA object dictionary.
/// Attached to a 
/// gameObject, it scans the scene and indexes other gameobjects for use with a collider-free raycasting technique
/// </summary>

public class APAObjectDictionary : MonoBehaviour {
	public APAOctree octree;
	public static APAObjectDictionary singleton;
	private int octreeDepth = 3;
	private int objectsPerChunk = 1000;
	public delegate void CallbackMethod();
	
	void Awake(){
		singleton = this;	
	}
	
	void OnDestroy(){
		Debug.Log("Mem Before Clear: " + System.GC.GetTotalMemory(true) / 1024f / 1024f);
		octree.Clear();
		octree = null;
		Destroy(singleton);
		Debug.Log("Mem After Clear: " + System.GC.GetTotalMemory(true) / 1024f / 1024f);
	}
		
	public void Init (CallbackMethod del, Bounds bounds)
	{		
		octree = new APAOctree(bounds, octreeDepth);	
		StartCoroutine(PopulateOctree (del));		
	}
	

	IEnumerator PopulateOctree (CallbackMethod del)
	{
		GameObject[] gameObjects = GameObject.FindObjectsOfType (typeof(GameObject)) as GameObject[];
		
		GameObject curGO; 
		Triangle[] curTris = new Triangle[] {};
		MeshFilter curMeshFilter = null;
		APAOctree finalNode;
		for (int i = 0; i < gameObjects.Length; i++) {
			curGO = gameObjects [i];
			if (curGO == null || curGO.name.Contains("Combined Mesh") || curGO.name == null || curGO.layer != LayerMask.NameToLayer("Raycast")) continue;
			
			curMeshFilter = curGO.GetComponent<MeshFilter>();
			if (!curMeshFilter) continue;
			curTris = new Triangle[] {};
			curTris = GetTriangles(curGO);
			for (int k = 0; k < curTris.Length; k++){
				finalNode = octree.IndexTriangle(curTris[k]);
				finalNode.AddTriangle(curTris[k]);	
			}
			
			if (i % objectsPerChunk == 1){
				yield return 0;
			}
		}
		
		del();
		Debug.Log("Created Database");
		Debug.Log("Total Indexed Triangles: " + GetTriangleCount(octree));
	
	}
	
	
	int GetTriangleCount(APAOctree o){
		int count = 0;
		count = o.triangles.Count;
		foreach(APAOctree oct in o.m_children){
			count += GetTriangleCount(oct) ;
		}
		return count;
	}

	Triangle[] GetTriangles(GameObject go){
		Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;

		Vector3[] verts = mesh.vertices;
		Vector2[] uvs = mesh.uv;
		List<Triangle> triangleList = new List<Triangle>();

        int subMeshCount = mesh.subMeshCount;

        // every material has a seperate triangle list
        // for each sub mesh
        for (int sm = 0; sm < subMeshCount; sm++)
        {
            int[] vIndex = mesh.GetTriangles(sm);
            int i = 0;
            while (i < vIndex.Length)
            {
                triangleList.Add(
                    new Triangle(
                    verts[vIndex[i + 0]],
                    verts[vIndex[i + 1]],
                    verts[vIndex[i + 2]],
                    uvs[vIndex[i + 0]],
                    uvs[vIndex[i + 1]],
                    uvs[vIndex[i + 2]],
                    go.transform,
                    sm
                    ));
                i += 3;
            }
        }
		return triangleList.ToArray();
	}
		
	void OnDrawGizmos(){
		DrawOctree(octree);
	}
	
	void DrawOctree(APAOctree oct){
		Gizmos.DrawWireCube(oct.bounds.center, oct.bounds.size);
		
//		foreach(APAOctree o in oct.m_children){
//			DrawOctree(o);	
//		}
	}
	
	public static APAOctree GetOctree(){
		return singleton.octree;
	}
}
