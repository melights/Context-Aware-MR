using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [SerializeField]
    private MaterialRayCastSystem m_materialRayCastSystem;

    [SerializeField]
    private DataManager m_dataManager;

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

    [SerializeField]
    private GameObject m_particleParent;

    private Vector3 lastHitPos = Vector3.zero;

    private WeaponStruct[] m_weaponDataCopy;

    // Use this for initialization
    void Start () {
        m_weaponDataCopy = m_dataManager.GetWeaponDataArray();
    }
	
	// Update is called once per frame
	void Update () {

        // Fire Ray
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
                    // We know the material...so...
                    finalMaterialName = mat.m_name;
                    finalMaterialColour = mat.m_colour;

                    // Grab Weapon Struct
                    int weaponIndex = 0;
                    var weaponStruct = m_weaponDataCopy[weaponIndex];



                    // Spawn Particle Effects
                    // uses material index to go into array
                    var newParticle = Instantiate(weaponStruct.m_prefabHitParticles[mat.m_index]) as GameObject;
                    newParticle.transform.SetParent(m_particleParent.transform, false);

                    // todo: orientate to normal
                    var wdsNrm = hit.transform.TransformVector(hit.normal);
                    newParticle.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(wdsNrm, Vector3.up));

                    // todo: Play Sounds



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
