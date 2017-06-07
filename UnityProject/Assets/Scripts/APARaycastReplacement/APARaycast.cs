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

public class APARaycastHit{
	
	public float distance;
	public Transform transform;
	public Vector2 barycentricCoordinate;
	public Vector2 textureCoord;
	public Vector3 point;
    public Triangle triangle;
	
	public APARaycastHit(){
		this.distance = 0f;
		this.transform = null;
		this.textureCoord = Vector2.zero;
		this.barycentricCoordinate = Vector2.zero;
		this.point = Vector3.zero;
        // hack
        //this.triangle = new Triangle(Vector3.zero, Vector3.zero, Vector3.zero, Vector2.zero, Vector2.zero, Vector2.zero, transform);
	}
	
	public APARaycastHit(Transform transform, float distance, Vector2 barycentricCoordinate){
		this.distance = distance;
		this.transform = transform;
		this.barycentricCoordinate = barycentricCoordinate;
		this.textureCoord = Vector2.zero;
		this.point = Vector3.zero;
	}
}

public class APARaycast : MonoBehaviour {
	
	static Vector3 edge1 = new Vector3();
	static Vector3 edge2 = new Vector3();
	static Vector3 tVec = new Vector3();
	static Vector3 pVec = new Vector3();
	static Vector3 qVec = new Vector3();
	
	static float det = 0;
	static float invDet = 0;
	static float u = 0;
	static float v = 0;
	
	static float epsilon = 0.0000001f;
	
	static System.Diagnostics.Stopwatch stopWatch;
	
	public static string intersectionErrorType = "";

	public static bool Raycast (Ray ray, out APARaycastHit hit)
	{	
		hit = new APARaycastHit();
		List<APARaycastHit> hits = new List<APARaycastHit>();
	
		hits = INTERNAL_RaycastAll(ray);
		
		hits = SortResults(hits);
		if (hits.Count > 0){
			hit = hits[0];
			return true;
		}
		return false;
	}
	
	public static APARaycastHit[] RaycastAll(Ray ray)
	{
		return INTERNAL_RaycastAll(ray).ToArray();
	}
	
	public static APARaycastHit[] RaycastAll(Ray ray, float dist, LayerMask mask){
		List<APARaycastHit> hits = INTERNAL_RaycastAll(ray);
		for (int i = 0; i < hits.Count; i++){
			if (hits[i].distance > dist) hits.RemoveAt(i);
			if ((1 << hits[i].transform.gameObject.layer & mask.value) != 1 << hits[i].transform.gameObject.layer){
				hits.RemoveAt(i);
			}
		}
		return hits.ToArray();
	}
	
	static List<APARaycastHit> INTERNAL_RaycastAll(Ray ray)
	{
		
		stopWatch = new System.Diagnostics.Stopwatch();
		stopWatch.Start();
		List<APARaycastHit> hits = new List<APARaycastHit>();
		APAOctree octree = APAObjectDictionary.GetOctree();
		
		if (octree.bounds.IntersectRay(ray)){
			hits = RecurseOctreeBounds(octree, ray);	
		}
		
		hits = SortResults(hits);
		stopWatch.Stop();
		Debug.Log("Search Time: " + stopWatch.ElapsedMilliseconds + " ms");
		return hits;
	}
	
	static bool INTERNAL_Raycast (Ray ray, out APARaycastHit hit)
	{	
		hit = new APARaycastHit();
		List<APARaycastHit> hits = new List<APARaycastHit>();
	
		APAOctree octree = APAObjectDictionary.GetOctree();
		
		if (octree.bounds.IntersectRay(ray)){
			hits = RecurseOctreeBounds(octree, ray);	
		}
		
		hits = SortResults(hits);
		if (hits.Count > 0){
			hit = hits[0];	
		}
		return hits.Count > 0;
	}

    static List<APARaycastHit> RecurseOctreeBounds(APAOctree octree, Ray ray)
    {
        List<APARaycastHit> hits = new List<APARaycastHit>();
        float dist = 0f;
        Vector2 baryCoord = new Vector2();
        if (octree.bounds.IntersectRay(ray))
        {
            for (int i = 0; i < octree.triangles.Count; i++)
            {
                if (TestIntersection(octree.triangles[i], ray, out dist, out baryCoord))
                {
                    hits.Add(BuildRaycastHit(octree.triangles[i], dist, baryCoord));
                }
            }
        }
        for (int i = 0; i < octree.m_children.Count; i++)
        {
            hits.AddRange(RecurseOctreeBounds(octree.m_children[i], ray));
        }
        return hits;
    }

    static APARaycastHit BuildRaycastHit(Triangle hitTriangle, float distance, Vector2 barycentricCoordinate){

		APARaycastHit returnedHit = new APARaycastHit(hitTriangle.trans, distance, barycentricCoordinate);
		returnedHit.textureCoord = hitTriangle.uv_pt0 + ((hitTriangle.uv_pt1 - hitTriangle.uv_pt0) * barycentricCoordinate.x) + ((hitTriangle.uv_pt2 - hitTriangle.uv_pt0) * barycentricCoordinate.y);
		returnedHit.point = hitTriangle.pt0 + ((hitTriangle.pt1 - hitTriangle.pt0) * barycentricCoordinate.x) + ((hitTriangle.pt2 - hitTriangle.pt0) * barycentricCoordinate.y);
        returnedHit.triangle = hitTriangle;
		return returnedHit;
		
	}
	
	/// <summary>
	/// Tests the intersection.
	/// Implementation of the Moller/Trumbore intersection algorithm 
	/// </summary>
	/// <returns>
	/// Bool if the ray does intersect
	/// out dist - the distance along the ray at the intersection point
	/// out hitPoint - 
	/// </returns>
	/// <param name='triangle'>
	/// If set to <c>true</c> triangle.
	/// </param>
	/// <param name='ray'>
	/// If set to <c>true</c> ray.
	/// </param>
	/// <param name='dist'>
	/// If set to <c>true</c> dist.
	/// </param>
	/// <param name='baryCoord'>
	/// If set to <c>true</c> barycentric coordinate of the intersection point.
	/// </param>
	/// http://www.cs.virginia.edu/~gfx/Courses/2003/ImageSynthesis/papers/Acceleration/Fast%20MinimumStorage%20RayTriangle%20Intersection.pdf
	static bool TestIntersection(Triangle triangle, Ray ray, out float dist, out Vector2 baryCoord){
		baryCoord = Vector2.zero;
		dist = Mathf.Infinity;
		edge1 = triangle.pt1 - triangle.pt0;
		edge2 = triangle.pt2 - triangle.pt0;
		
		pVec = Vector3.Cross (ray.direction, edge2);
		det = Vector3.Dot ( edge1, pVec);
		if (det < epsilon) {
			intersectionErrorType = "Failed Epsilon";
			return false;	
		}
		tVec = ray.origin - triangle.pt0;
		u = Vector3.Dot (tVec, pVec);
		if (u < 0 || u > det) {
			intersectionErrorType = "Failed Dot1";
			return false;	
		}
		qVec = Vector3.Cross (tVec, edge1);
		v = Vector3.Dot (ray.direction, qVec);
		if (v < 0 || u + v > det) {
			intersectionErrorType = "Failed Dot2";
			return false;	
		}
		dist = Vector3.Dot(edge2, qVec);
		invDet = 1 / det;
		dist *= invDet;
		baryCoord.x = u * invDet;
		baryCoord.y = v * invDet;
		return true;
	}
	
	static List<APARaycastHit> SortResults(List<APARaycastHit> input){
		
		APARaycastHit a = new APARaycastHit();
		APARaycastHit b = new APARaycastHit();
		bool swapped = true;
		while (swapped){
			swapped = false;
			for(int i = 1; i < input.Count; i++){
				if (input[i-1].distance > input[i].distance){
					a = input[i-1];
					b = input[i];
					input[i-1] = b;
					input[i] = a;
					swapped = true;
				}
			}
		}
		
		return input;
	}
	

}
