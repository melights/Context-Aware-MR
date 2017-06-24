using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [SerializeField]
    private MaterialRayCastSystem m_materialRayCastSystem;

    [SerializeField]
    private Color m_errorCol;

    [SerializeField]
    private Camera m_renderCam;

    [SerializeField]
    private Text m_uiMaterialTextOutput;

    [SerializeField]
    private Image m_uiMaterialColourOutput;

    [SerializeField]
    private Transform m_debugHitPoint;

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
            MaterialStruct mat;

            if (m_materialRayCastSystem.RayVsSceneMaterial(r, out hit, out mat))
            {
                Color finalMaterialColour;
                string finalMaterialName = "";
                lastHitPos = hit.point;

                if (mat != null)
                {
                    finalMaterialName = mat.m_name;
                    finalMaterialColour = mat.m_colour;
                }
                else
                {
                    Debug.LogError("Couldn't Find material!");
                    finalMaterialName = "Error";
                    finalMaterialColour = m_errorCol;
                }

                m_uiMaterialColourOutput.color = finalMaterialColour;
                m_uiMaterialTextOutput.text = finalMaterialName;
            }
            else
            {
                m_uiMaterialColourOutput.color = m_errorCol;
                m_uiMaterialTextOutput.text = "No Data / Ray Missed ";
            }
        }

        // Debug.DrawRay(r.origin, r.direction, Color.green, 100);

        m_debugHitPoint.position = lastHitPos;
    }
}
