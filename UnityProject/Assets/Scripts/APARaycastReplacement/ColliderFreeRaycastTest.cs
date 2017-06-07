using UnityEngine;
using System.Collections;

public class ColliderFreeRaycastTest : MonoBehaviour {
	public GameObject highPolySphere;
	public GameObject highPolySphereWithCollider;
	public int testObjectCount = 1000;
	public Bounds bounds;
	
	public bool useStandardColliders;
	System.Diagnostics.Stopwatch sw;
	
	// Use this for initialization
	void Start () {
		sw = new System.Diagnostics.Stopwatch();
		sw.Start();
		GameObject obj;
		if (useStandardColliders){
			for (int i = 0; i < testObjectCount; i++){
				obj = Instantiate(highPolySphereWithCollider, RandomV3(), RandomQuat()) as GameObject;
				obj.transform.parent = transform;
			}
			Callback();
		}else{
			for (int i = 0; i < testObjectCount; i++){
				obj = Instantiate(highPolySphere, RandomV3(), RandomQuat()) as GameObject;
				obj.transform.parent = transform;
			}
			APAObjectDictionary.singleton.Init(Callback, bounds);
		}
		
	}
	
	public void Callback(){
		sw.Stop();
		Debug.Log("Completed Dictionary Build and Object Generation in " + sw.ElapsedMilliseconds / 1000f + " seconds");	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0)){
			if (useStandardColliders){
				RaycastHit hit;
				sw.Reset();
				sw.Start();
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
					hit.transform.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
				}
				sw.Stop();
				Debug.Log("Standard Collider search completed in " + sw.ElapsedMilliseconds + " ms");
			}else{
				APARaycastHit hit;
				if (APARaycast.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
					hit.transform.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 1);
					lastHitPos = hit.point;
				}else{
					Debug.LogWarning("Miss! " + APARaycast.intersectionErrorType);
					Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
					Debug.DrawRay(r.origin, r.direction, Color.green, 100);
				}
			}
		}
	}
	Vector3 lastHitPos = Vector3.zero;
	void OnDrawGizmos(){
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(lastHitPos, .1f);
	}
	
	Vector3 RandomV3(){
		return new Vector3(
			Random.Range(-bounds.extents.x + bounds.center.x + highPolySphere.GetComponent<Renderer>().bounds.extents.x, bounds.extents.x + bounds.center.x - highPolySphere.GetComponent<Renderer>().bounds.extents.x), 
			Random.Range(-bounds.extents.y + bounds.center.y + highPolySphere.GetComponent<Renderer>().bounds.extents.y, bounds.extents.y + bounds.center.y - highPolySphere.GetComponent<Renderer>().bounds.extents.y), 
			Random.Range(-bounds.extents.z + bounds.center.z + highPolySphere.GetComponent<Renderer>().bounds.extents.z, bounds.extents.z + bounds.center.z - highPolySphere.GetComponent<Renderer>().bounds.extents.z)
			);	
	}
	
	Quaternion RandomQuat(){
		return Quaternion.Euler(RandomV3()* 10);	
	}
}
