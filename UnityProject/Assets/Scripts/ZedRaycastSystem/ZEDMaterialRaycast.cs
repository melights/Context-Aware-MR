using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ZEDMaterialRaycast : MonoBehaviour {

    private ExecutePythonFile m_linkedPythonFile;

    [SerializeField]
    private GameController m_gameController;

    [SerializeField]
    private TextureOverlay m_textureOverlay;

    [SerializeField]
    private Camera m_zedCamera;

    [SerializeField]
    private Transform m_hitPointSphere;

    private Material m_exportFlipMaterial;

    private bool m_rayCastTriggered = false;
    private Vector2 m_mousePositionWhenTriggered;
    private RenderTexture m_zedRT;
    private RenderTextureFormat m_zedFormat = RenderTextureFormat.ARGBFloat;

    private int m_lastWidth;
    private int m_lastHeight;

    private float m_mlPercentage = 0.2f;

    private const string m_pythonExePath = @"C:\Python27\python.exe";
    private const string m_pythonFilePath = @"C:\Users\SOPOT\Desktop\BUWork\WenWork\UnityProject\ml\infer.py";
    private const string m_pythonWorkingDir = @"C:\Users\SOPOT\Desktop\BUWork\WenWork\UnityProject\ml\";

    // Additional Args
    private const string m_colImageOutputPath = @"C:\users\SOPOT\Desktop\test.jpg";
    private const string m_matImageOutputPath = @"C:\users\SOPOT\Desktop\matc.png";

    bool m_showMaterialColours = false;
    private Texture2D m_materialColourTexture;
    private bool m_makeNewColourTexture = false;

    // Use this for initialization
    void Start () {

        // Must be added to game component, otherwise it will not be setup properly and coroutines won't work
        m_linkedPythonFile = gameObject.AddComponent<ExecutePythonFile>();

        // Build arg list
        List<string> args = new List<string>();
        args.Add(m_colImageOutputPath);
        args.Add(m_matImageOutputPath);

        // Build handler list
        List<System.EventHandler> handlers = new List<System.EventHandler>();
        handlers.Add(new System.EventHandler(MLFinishedCallback));

        // Setup ML Python
        m_linkedPythonFile.Setup(m_pythonExePath, m_pythonFilePath, m_pythonWorkingDir, args, handlers);

        // Export Materials
        m_exportFlipMaterial = new Material(Shader.Find("Hidden/ExportFlip"));

        // Store width etc to test for resize
        m_lastWidth = Screen.width;
        m_lastHeight = Screen.height;

        // Create new render texture
        var zDepth = m_textureOverlay.depthXYZZed;
        m_zedRT = new RenderTexture(zDepth.width, zDepth.height, 0, m_zedFormat);
    }

    public void ToggleCameraTexture()
    {
        if (m_materialColourTexture == null)
        {
            Debug.Log("Material Colour Texture not ready, trigger a ML to make one");
            return;
        }

        m_showMaterialColours = !m_showMaterialColours;

        if (m_showMaterialColours)
        {
            // have to pass in every time as texture gets updated
            m_textureOverlay.UseEncodedMaterial(m_materialColourTexture);
        }
        else
        {
            m_textureOverlay.UseStandardMaterial();
        }
    }

    public void TriggerRayCast()
    {
        m_rayCastTriggered = true;
        m_mousePositionWhenTriggered = Input.mousePosition;
    }

    private void MLFinishedCallback(object sender, System.EventArgs e)
    {
        Debug.Log("ML FINISHED YAY!");

        if (m_materialColourTexture != null)
        {

        }

        // Can't make Texture2D here as this is called from another thread
        m_makeNewColourTexture = true;
    }

    private void Update()
    {
        if (m_makeNewColourTexture)
        {
            m_materialColourTexture = LoadPNG(m_matImageOutputPath);
            m_makeNewColourTexture = false;
        }

    }

    public void TriggerML()
    {
        Debug.Log("Starting ML");

        // Output Colour Buffer
        WriteColourBufferToFile();

        // Run ML
        m_linkedPythonFile.RunPython();
    }

    private void WriteColourBufferToFile()
    {
        var zCol = m_textureOverlay.camZedLeft;

        // Store current RT
        RenderTexture currentRT = RenderTexture.active;
        {
            // Create New RT
            int nW = (int)((float)(zCol.width) * m_mlPercentage);
            int nH = (int)((float)(zCol.height) * m_mlPercentage);
            RenderTexture colRt = new RenderTexture(nW, nH, 0, RenderTextureFormat.ARGB32);

            // Material Sorts out the data
            // Flips image and channels
            Graphics.Blit(zCol, colRt, m_exportFlipMaterial);

            Texture2D zedReadable = new Texture2D(colRt.width, colRt.height, zCol.format, false);
            zedReadable.ReadPixels(new Rect(0, 0, colRt.width, colRt.height), 0, 0);
            zedReadable.Apply();

            var bytes = zedReadable.EncodeToJPG(100);

            File.WriteAllBytes(m_colImageOutputPath, bytes);
        }
        // Restore Active RT
        RenderTexture.active = currentRT;
    }

    // note: we may not actually need to do this
    private void OnResize()
    {
        if (m_zedRT != null)
        {
            m_lastWidth = Screen.width;
            m_lastHeight = Screen.height;
            m_zedRT.Release();
            var zDepth = m_textureOverlay.depthXYZZed;
            m_zedRT = new RenderTexture(zDepth.width, zDepth.height, 0, m_zedFormat);
        }
    }

    // Has to be OnPostRender so we can grab render targets safely
    private void OnPostRender()
    {
        if (m_lastWidth != Screen.width || m_lastHeight != Screen.height)
        {
            OnResize();
        }

        if (m_rayCastTriggered)
        {
            m_rayCastTriggered = false;
            var zDepth = m_textureOverlay.depthXYZZed;

            // Store current RT
            RenderTexture currentRT = RenderTexture.active;

            // Create new render target to copy
            {
                RenderTexture.active = m_zedRT;

                Graphics.Blit(zDepth, m_zedRT);

                Texture2D zedReadable = new Texture2D(zDepth.width, zDepth.height, zDepth.format, false);
                zedReadable.ReadPixels(new Rect(0, 0, m_zedRT.width, m_zedRT.height), 0, 0);
                zedReadable.Apply();

                Vector2 relMousePos = new Vector2();

                relMousePos.x = m_mousePositionWhenTriggered.x / Screen.width;
                relMousePos.y = m_mousePositionWhenTriggered.y / Screen.height;

                Vector2 dPixUv = relMousePos;

                dPixUv.y = 1.0f - dPixUv.y;

                dPixUv.x *= zedReadable.width;
                dPixUv.y *= zedReadable.height;

                // this only works for starting camera position and rotation
                var depthPixelData = zedReadable.GetPixel((int)dPixUv.x, (int)dPixUv.y);
                var matPixelData = new Color(0, 0, 0, 0);
                MaterialStruct matFound = null;

                if (m_materialColourTexture != null)
                {
                    Vector2 mPixUv = relMousePos;
                    mPixUv.y = 1.0f - mPixUv.y;

                    mPixUv.x *= m_materialColourTexture.width;
                    mPixUv.y *= m_materialColourTexture.height;

                    matPixelData = m_materialColourTexture.GetPixel((int)mPixUv.x, (int)mPixUv.y);

                    matFound = MaterialRayCastSystem.FindMaterialStructFromColour(matPixelData);
                }

                // Need to guard against uninit areas of pixels
                // This can happen if camera hasn't been able to do depth properly in area

                if(
                   float.IsNaN(depthPixelData.r) || 
                   float.IsInfinity(depthPixelData.r) ||
                   float.IsNaN(depthPixelData.g) ||
                   float.IsInfinity(depthPixelData.g) ||
                   float.IsNaN(depthPixelData.b) ||
                   float.IsInfinity(depthPixelData.b)
                   )
                    {
                    return;
                }

                // If changed process of getting this position, then remember to udpate for loop at bottom
                Vector3 cVsPos = new Vector3(depthPixelData.r, depthPixelData.g, -depthPixelData.b);

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
                        new Vector2( 0, 15),
                        new Vector2( 0,-15),
                        new Vector2( 15, 0),
                        new Vector2(-15, 0)
                    };

                    Vector3[] wsPositions = new Vector3[4];
                    float[] distToCenter = new float[4];

                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 uvOffset = dPixUv + offsets[i];
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

                    // Update Game Reaction
                    m_gameController.GameReaction(cWsPos, normalGen, matFound);
                }
            }

            // Restore original active render texture
            RenderTexture.active = currentRT;
        }
    }

    private Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

            // This flips vertically
            var pixels = tex.GetPixels();
            System.Array.Reverse(pixels, 0, pixels.Length);
            tex.SetPixels(pixels);

            // This then flips horizontally and uploads to GPU
            tex = FlipTexture(tex);
        }
        else
        {
            Debug.LogError("Could not find file!");
        }
        return tex;
    }

    private Texture2D FlipTexture(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);

        int xN = original.width;
        int yN = original.height;

        for (int i = 0; i < xN; i++)
        {
            for (int j = 0; j < yN; j++)
            {
                flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
            }
        }
        flipped.Apply();

        return flipped;
    }
}
