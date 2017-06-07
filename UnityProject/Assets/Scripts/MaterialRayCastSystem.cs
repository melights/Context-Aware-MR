using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialRayCastSystem : MonoBehaviour {

    // UI Elements to Update

    [SerializeField]
    private Text m_uiMaterialTextOutput;

    [SerializeField]
    private Image m_uiMaterialColourOutput;

    [SerializeField]
    private DataManager m_dataManager;

    [SerializeField]
    private Transform m_debugHitPoint;

    [SerializeField]
    private Camera m_renderCam;

    Vector3 lastHitPos = Vector3.zero;

    // Use this for initialization
    void Start () {


    }
	
	// Update is called once per frame
	void Update () {
 

        if (Input.GetMouseButtonDown(0))
        {
            Ray r = m_renderCam.ScreenPointToRay(Input.mousePosition);

            APARaycastHit hit;
            if (APARaycast.Raycast(r, out hit))
            {
                lastHitPos = hit.point;
            }
            else
            {
                Debug.Log("Miss! " + APARaycast.intersectionErrorType);
            }
        }

       // Debug.DrawRay(r.origin, r.direction, Color.green, 100);

        m_debugHitPoint.position = lastHitPos;

    }

}
