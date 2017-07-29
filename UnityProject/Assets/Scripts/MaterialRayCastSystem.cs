using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialRayCastSystem : MonoBehaviour {

    [SerializeField]
    private DataManager m_dataManager;

    static private MaterialStruct[] m_materialDataCopy;

    // Use this for initialization
    void Start () {
        m_materialDataCopy = m_dataManager.GetMaterialDataArray();
    }

    static public MaterialStruct FindMaterialStructFromColour(Color col)
    {
        string hexId = HexUtility.colorToHex(col);

        Debug.Log("Find " + hexId);

        for (int i = 0; i < m_materialDataCopy.Length; i++)
        {
            var hex = m_materialDataCopy[i].m_hexCol;

            if (hex == hexId)
            {
                return m_materialDataCopy[i];
            }
        }
        return null;
    }

    static public bool RayVsSceneMaterial(Ray r, out APARaycastHit hit, out MaterialStruct material)
    {
        hit = null;
        material = null;

        if (APARaycast.Raycast(r, out hit))
        {
            Debug.Log("Hit " + hit.transform.gameObject.name);

            // Find The Material
            var go = hit.transform.gameObject;
            var goMat = go.GetComponent<Renderer>().material;
            var matMainTex = goMat.mainTexture as Texture2D;
            Vector2 pixelUv = hit.textureCoord;
            pixelUv.x *= matMainTex.width;
            pixelUv.y *= matMainTex.height;

            var pixelColour = matMainTex.GetPixel((int)pixelUv.x, (int)pixelUv.y);
            material = FindMaterialStructFromColour(pixelColour);

            return true;
        }
        else
        {
            // Debug.Log("Miss! " + APARaycast.intersectionErrorType);

            return false;
        }
    }
}
