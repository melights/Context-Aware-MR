using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalRandomness : MonoBehaviour {

    float m_defaultScale = 0.1f;

	// Use this for initialization
	void Start () {

        float randomScaleMod = Random.Range(0.5f, 1.0f);
        transform.localScale = new Vector3(m_defaultScale, m_defaultScale, m_defaultScale) * randomScaleMod;

        float randomRotationZ = Random.Range(0.0f, 360.0f);
        transform.localEulerAngles = new Vector3(0.0f, 0.0f, randomRotationZ);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
