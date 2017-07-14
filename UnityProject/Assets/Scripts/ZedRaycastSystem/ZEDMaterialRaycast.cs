using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo: force 16:9 resolution

public class ZEDMaterialRaycast : MonoBehaviour {

    [SerializeField]
    private TextureOverlay m_textureOverlay;

    [SerializeField]
    private Camera m_zedCamera;

    [SerializeField]
    private Transform m_hitPointSphere;

    bool m_raycastTroggerd = false;
    Vector2 m_mosuePos;

	// Use this for initialization
	void Start () {
		
	}

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_raycastTroggerd = true;
            m_mosuePos = Input.mousePosition;
        }
    }

    // Update is called once per frame
    private void OnPostRender()
    {
        if (m_raycastTroggerd)
        {
            m_raycastTroggerd = false;
            var zedTexture = m_textureOverlay.depthXYZZed;
            RenderTextureFormat zedFormat = RenderTextureFormat.ARGBFloat;

            // Store current RT
            RenderTexture currentRT = RenderTexture.active;

            // Create new render target to copy
            {
                RenderTexture tempRT = new RenderTexture(zedTexture.width, zedTexture.height, 0, zedFormat);

                RenderTexture.active = tempRT;

                Graphics.Blit(zedTexture, tempRT);

                Texture2D zedReadable = new Texture2D(zedTexture.width, zedTexture.height, zedTexture.format, false);
                zedReadable.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
                zedReadable.Apply();

                Vector2 pixelUv;

                pixelUv.x = m_mosuePos.x / Screen.width;
                pixelUv.y = m_mosuePos.y / Screen.height;

                //var pixelData = zedReadable.GetPixelBilinear(pixelUv.x, pixelUv.y);


                pixelUv.y = 1.0f - pixelUv.y;

                pixelUv.x *= zedReadable.width;
                pixelUv.y *= zedReadable.height;

                // this only works for starting camera position and rotation
                var pixelData = zedReadable.GetPixel((int)pixelUv.x, (int)pixelUv.y);

                // Need to guard against uninit areas of pixels
                // This can happen if camera hasn't been able to do depth properly in area

                if(
                   float.IsNaN(pixelData.r) || 
                   float.IsInfinity(pixelData.r) ||
                   float.IsNaN(pixelData.g) ||
                   float.IsInfinity(pixelData.g) ||
                   float.IsNaN(pixelData.b) ||
                   float.IsInfinity(pixelData.b)
                   )
                    {
                    return;
                }
       

                Vector3 viewspacePos = new Vector3(pixelData.r, pixelData.g, -pixelData.b);

                // get back into object space
                Matrix4x4 inverseView = m_zedCamera.worldToCameraMatrix.inverse;

                Vector3 worldPos = inverseView * new Vector4(viewspacePos.x, viewspacePos.y, viewspacePos.z, 1.0f);

               // Vector3 worldPos = m_zedCamera.transform.TransformPoint(objectPos);

                m_hitPointSphere.position = worldPos;

                Debug.Log(pixelUv);
                Debug.Log(pixelData);
            }

            RenderTexture.active = currentRT;
        }
    }
}
