using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RayTriCollider : MonoBehaviour {


    System.Diagnostics.Stopwatch sw;


    // Use this for initialization
    void Start () {

        sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        var bc = GetComponent<BoxCollider>();

        if (bc == null)
        {
            Debug.LogError("No Box Collider Detected!");
        }


        APAObjectDictionary.singleton.Init(Callback, bc.bounds);

        bc.enabled = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}


    public void Callback()
    {
        sw.Stop();
        Debug.Log("Completed Dictionary Build and Object Generation in " + sw.ElapsedMilliseconds / 1000f + " seconds");
    }
}
