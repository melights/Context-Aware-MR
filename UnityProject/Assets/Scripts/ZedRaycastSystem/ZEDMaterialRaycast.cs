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


                // If changed process of getting this position, then remember to udpate for loop at bottom
                Vector3 cVsPos = new Vector3(pixelData.r, pixelData.g, -pixelData.b);

                // get back into object space
                Matrix4x4 inverseView = m_zedCamera.worldToCameraMatrix.inverse;

                Vector3 cWsPos = inverseView * new Vector4(cVsPos.x, cVsPos.y, cVsPos.z, 1.0f);

                // Now we need to find the normal....
                {
                    // https://stackoverflow.com/questions/37627254/how-to-reconstruct-normal-from-depth-without-artifacts-on-edge
                    //     [ ]
                    // [ ] [x] [ ]
                    //     [ ]

                    // 4 samples around edge
                    // find the closest sample top and bottom, left and right relative to center sample
                    // then normalise cross product on it? (x dir, y dir)

                    Vector2[] offsets = new Vector2[4]
                    {
                        new Vector2( 0, 1),
                        new Vector2( 0,-1),
                        new Vector2( 1, 0),
                        new Vector2(-1, 0)
                    };

                    Vector3[] wsPositions = new Vector3[4];
                    float[] distToCenter = new float[4];

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 uvOffset = pixelUv + offsets[i];
                        Color sample = zedReadable.GetPixel((int)uvOffset.x, (int)uvOffset.y);
                        Vector4 vsPos = new Vector4(sample.r, sample.g, -sample.b, 1.0f);

                        // Calculate WS POS
                        Vector3 sampleWsPos = inverseView * vsPos;

                        distToCenter[i] = Vector3.Distance(cWsPos, sampleWsPos);

                        wsPositions[i] = sampleWsPos;
                    }

                    int verticalSampleIndex = 0;
                    int horizontalSampleIndex = 2;

                    // Find closest vertical sample
                    if (distToCenter[0] > distToCenter[1])
                    {
                        verticalSampleIndex = 1;
                    }

                    // Find closest horizontal sample
                    if (distToCenter[2] > distToCenter[3])
                    {
                        horizontalSampleIndex = 3;
                    }

                    Vector3 normalGen = Vector3.Cross(wsPositions[verticalSampleIndex] - cWsPos, wsPositions[horizontalSampleIndex] - cWsPos);
                    normalGen.Normalize();

                    // Update Hit Point Sphere
                    m_hitPointSphere.position = cWsPos;
                }



                Debug.Log(pixelUv);
                Debug.Log(pixelData);
            }

            RenderTexture.active = currentRT;
        }
    }
}
