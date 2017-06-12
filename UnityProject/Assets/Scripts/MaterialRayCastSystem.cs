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

    [SerializeField]
    private Color m_errorCol;

    [SerializeField]
    private GameObject m_materialMeshRenderer;

    Vector3 lastHitPos = Vector3.zero;

    private MaterialStruct[] m_materialDataCopy;

    // Use this for initialization
    void Start () {
        m_materialDataCopy = m_dataManager.GetMaterialDataArray();
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

                Debug.Log("Hit " + hit.transform.gameObject.name);

                var go = hit.transform.gameObject;
                var goMat = go.GetComponent<Renderer>().material;
                var matMainTex = goMat.mainTexture as Texture2D;
                Vector2 pixelUv = hit.textureCoord;
                pixelUv.x *= matMainTex.width;
                pixelUv.y *= matMainTex.height;

                var hitCol = matMainTex.GetPixel((int)pixelUv.x, (int)pixelUv.y);
                
                // find material struct
                // todo: some sort of margin of error?
                var mat = FindMaterialStructFromColour(hitCol, ref m_materialDataCopy);

                string outputString = "";

                if (mat != null)
                {
                    outputString = mat.m_name;
                }
                else
                {
                    Debug.LogError("Couldn't Find material!");
                    outputString = "Error";
                    hitCol = m_errorCol;
                }

                m_uiMaterialColourOutput.color = hitCol;
                m_uiMaterialTextOutput.text = outputString;

            }
            else
            {
               // Debug.Log("Miss! " + APARaycast.intersectionErrorType);

                m_uiMaterialColourOutput.color = m_errorCol;
                m_uiMaterialTextOutput.text = "No Data / Ray Missed : " + APARaycast.intersectionErrorType;
            }
        }

       // Debug.DrawRay(r.origin, r.direction, Color.green, 100);

        m_debugHitPoint.position = lastHitPos;

    }

    private MaterialStruct FindMaterialStructFromColour(Color col, ref MaterialStruct[] ms)
    {
        string hexId = HexUtility.colorToHex(col);

        Debug.Log("Find " + hexId);

        for (int i = 0; i < ms.Length; i++)
        {
            var hex = ms[i].m_hexCol;

            if (hex == hexId)
            {
                return ms[i];
            }
        }
        return null;
    }

    public void ToggleMeshRenderer()
    {
        m_materialMeshRenderer.SetActive(!m_materialMeshRenderer.activeSelf);
    }

}
